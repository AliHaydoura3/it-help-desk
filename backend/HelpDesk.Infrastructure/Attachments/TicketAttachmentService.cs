using FluentValidation;
using FluentValidation.Results;
using HelpDesk.Application.Abstractions.Attachments;
using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Application.Abstractions.Communication;
using HelpDesk.Application.Common.Authorization;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Notifications;
using HelpDesk.Application.Features.Attachments;
using HelpDesk.Application.Features.Attachments.Download;
using HelpDesk.Application.Features.Attachments.List;
using HelpDesk.Application.Features.Attachments.Upload;
using HelpDesk.Domain;
using HelpDesk.Infrastructure.Identity;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Attachments;

public sealed class TicketAttachmentService(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IAttachmentPolicy attachmentPolicy,
    IAttachmentStorage attachmentStorage,
    INotificationQueue notificationQueue) : ITicketAttachmentService
{
    public async Task<TicketAttachmentResponse> UploadAsync(
        UploadTicketAttachmentCommand command,
        CancellationToken cancellationToken)
    {
        var ticket = await FindTicketAsync(command.TicketId, tracking: true, cancellationToken);
        EnsureCanRead(ticket);
        EnsureCanUpload(ticket);

        var attachmentCount = await dbContext.TicketAttachments.CountAsync(
            attachment => attachment.TicketId == ticket.Id,
            cancellationToken);
        if (attachmentCount >= attachmentPolicy.MaximumFilesPerTicket)
        {
            throw FileValidationException(
                $"A ticket can contain at most {attachmentPolicy.MaximumFilesPerTicket} attachments.");
        }

        var fileType = attachmentPolicy.Match(command.FileName, command.ContentType)
            ?? throw FileValidationException("The attachment type is not supported.");
        var uploader = await dbContext.Users.AsNoTracking().SingleAsync(
            user => user.Id == currentUser.UserId,
            cancellationToken);
        StoredAttachmentFile? storedFile = null;

        try
        {
            storedFile = await attachmentStorage.StoreAsync(
                command.Content,
                fileType,
                attachmentPolicy.MaximumFileSizeBytes,
                cancellationToken);
            if (storedFile.SizeBytes != command.DeclaredSizeBytes)
                throw FileValidationException("The uploaded file size does not match its declared size.");

            var now = DateTime.UtcNow;
            var historyCount = ticket.History.Count;
            var attachment = ticket.AddAttachment(
                currentUser.UserId,
                NormalizeFileName(command.FileName),
                storedFile.StorageKey,
                fileType.ContentType,
                fileType.Extension,
                storedFile.SizeBytes,
                storedFile.Sha256Hash,
                now);
            dbContext.TicketAttachments.Add(attachment);
            dbContext.TicketHistories.AddRange(ticket.History.Skip(historyCount));

            await notificationQueue.QueueAsync(
                new NotificationMessage(
                    currentUser.UserId,
                    ticket.Id,
                    NotificationType.TicketUpdated,
                    "New ticket attachment",
                    $"{attachment.OriginalFileName} was attached to ticket {ticket.ReferenceNumber}.",
                    TicketParticipantIds(ticket)),
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Map(attachment, uploader);
        }
        catch
        {
            if (storedFile is not null)
            {
                await attachmentStorage.DeleteIfExistsAsync(
                    storedFile.StorageKey,
                    CancellationToken.None);
            }
            throw;
        }
    }

    public async Task<GetTicketAttachmentsResponse> GetAllAsync(
        GetTicketAttachmentsQuery query,
        CancellationToken cancellationToken)
    {
        var ticket = await FindTicketAsync(query.TicketId, tracking: false, cancellationToken);
        EnsureCanRead(ticket);

        var attachments = dbContext.TicketAttachments.AsNoTracking()
            .Where(attachment => attachment.TicketId == ticket.Id);
        var totalCount = await attachments.CountAsync(cancellationToken);
        var items = await attachments
            .OrderByDescending(attachment => attachment.UploadedAtUtc)
            .ThenByDescending(attachment => attachment.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Join(
                dbContext.Users.AsNoTracking(),
                attachment => attachment.UploadedByUserId,
                uploader => uploader.Id,
                (attachment, uploader) => new TicketAttachmentResponse(
                    attachment.Id,
                    attachment.TicketId,
                    attachment.OriginalFileName,
                    attachment.ContentType,
                    attachment.Extension,
                    attachment.SizeBytes,
                    attachment.Sha256Hash,
                    new AttachmentUploaderResponse(
                        uploader.Id,
                        uploader.FirstName,
                        uploader.LastName),
                    attachment.UploadedAtUtc))
            .ToListAsync(cancellationToken);

        return new GetTicketAttachmentsResponse(
            items,
            query.PageNumber,
            query.PageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)query.PageSize));
    }

    public async Task<AttachmentDownloadResponse> DownloadAsync(
        DownloadTicketAttachmentQuery query,
        CancellationToken cancellationToken)
    {
        var ticket = await FindTicketAsync(query.TicketId, tracking: false, cancellationToken);
        EnsureCanRead(ticket);
        var attachment = await dbContext.TicketAttachments.AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == query.AttachmentId && item.TicketId == ticket.Id,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Attachment '{query.AttachmentId}' does not exist on this ticket.");
        var content = await attachmentStorage.OpenReadAsync(
            attachment.StorageKey,
            cancellationToken);

        return new AttachmentDownloadResponse(
            content,
            attachment.OriginalFileName,
            attachment.ContentType,
            attachment.SizeBytes,
            attachment.Sha256Hash,
            attachment.UploadedAtUtc);
    }

    private async Task<Ticket> FindTicketAsync(
        Guid ticketId,
        bool tracking,
        CancellationToken cancellationToken)
    {
        var tickets = tracking
            ? dbContext.Tickets.AsQueryable()
            : dbContext.Tickets.AsNoTracking();
        return await tickets.SingleOrDefaultAsync(
            ticket => ticket.Id == ticketId,
            cancellationToken)
            ?? throw new KeyNotFoundException($"Ticket '{ticketId}' does not exist.");
    }

    private void EnsureCanRead(Ticket ticket)
    {
        if (!currentUser.CanReadTicket(ticket))
            throw new ForbiddenAccessException("You cannot access attachments on this ticket.");
    }

    private void EnsureCanUpload(Ticket ticket)
    {
        if (ticket.IsCancelled)
            throw new InvalidOperationException("A cancelled ticket cannot receive attachments.");
        if (ticket.Status == TicketStatus.Closed)
            throw new InvalidOperationException("A closed ticket cannot receive attachments.");
        if (!currentUser.CanUploadAttachment(ticket))
        {
            throw new ForbiddenAccessException(
                "Your role has read-only access to attachments on this ticket.");
        }
    }

    private static string NormalizeFileName(string fileName)
    {
        var normalized = new string(fileName
            .Where(character => !char.IsControl(character))
            .ToArray())
            .Trim();
        return normalized.Length <= 255 ? normalized : normalized[..255];
    }

    private static IReadOnlyCollection<Guid> TicketParticipantIds(Ticket ticket)
    {
        var ids = new List<Guid> { ticket.CreatedByUserId };
        if (ticket.AssignedToUserId.HasValue) ids.Add(ticket.AssignedToUserId.Value);
        return ids;
    }

    private static TicketAttachmentResponse Map(
        TicketAttachment attachment,
        ApplicationUser uploader) =>
        new(
            attachment.Id,
            attachment.TicketId,
            attachment.OriginalFileName,
            attachment.ContentType,
            attachment.Extension,
            attachment.SizeBytes,
            attachment.Sha256Hash,
            new AttachmentUploaderResponse(
                uploader.Id,
                uploader.FirstName,
                uploader.LastName),
            attachment.UploadedAtUtc);

    private static ValidationException FileValidationException(string message) =>
        new([new ValidationFailure("File", message)]);
}
