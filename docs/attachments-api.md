# Ticket attachments backend

Ticket attachments use authenticated, ticket-scoped endpoints. Files are never
published from `wwwroot`; clients must pass the normal JWT authorization check
for every metadata or download request.

## Access rules

| Operation | Employee | Manager | IT Support Agent | Admin |
| --- | --- | --- | --- | --- |
| List/download own ticket attachments | Yes | Yes | Yes | Yes |
| List/download another user's ticket attachments | No | Read-only | Yes | Yes |
| Upload to own open, in-progress, pending, or resolved ticket | Yes | Yes | Yes | Yes |
| Upload to another user's active ticket | No | No | Yes | Yes |
| Upload to closed or cancelled ticket | No | No | No | No |

Managers retain Employee capabilities for tickets they created but remain
read-only on other users' tickets.

## Endpoints

### Policy

`GET /api/attachments/policy`

Returns the configured size/count limits and canonical supported file types so
clients do not need to duplicate server configuration.

### Upload

`POST /api/tickets/{ticketId}/attachments`

Send `multipart/form-data` with one field named `file`. The response contains
attachment metadata; the physical storage key is deliberately never exposed.

### List

`GET /api/tickets/{ticketId}/attachments?pageNumber=1&pageSize=20`

The result is paginated and includes file metadata and uploader identity.

### Download

`GET /api/tickets/{ticketId}/attachments/{attachmentId}/download`

Downloads are streamed with range support, forced attachment disposition,
`nosniff`, private/no-store caching, a SHA-256 ETag, and restrictive content
security headers. The attachment must belong to the ticket in the route.

## Validation and storage

- The default maximum size is 10 MB, with a hard configurable ceiling of 25 MB.
- The default maximum is 25 attachments per ticket.
- Supported extensions are `.png`, `.jpg`, `.jpeg`, `.pdf`, `.txt`, `.log`,
  `.csv`, `.zip`, `.docx`, and `.xlsx`.
- Extension and declared MIME type must match the configured allow-list.
- Server-side signature inspection rejects renamed or disguised files.
- Text files must contain valid text and cannot contain null characters.
- DOCX and XLSX files must contain their expected Open XML package entries.
- Stored names are random GUIDs partitioned by year/month; original names are
  metadata only.
- Every file has a server-calculated SHA-256 hash.
- Partial files are deleted when validation or database persistence fails.
- On Unix, the storage root is owner-only and files are created owner-readable
  and owner-writable.

Configure the module in `HelpDesk.Api/appsettings.json`:

```json
{
  "Attachments": {
    "StorageRootPath": "App_Data/attachments",
    "MaximumFileSizeBytes": 10485760,
    "MaximumFilesPerTicket": 25,
    "AllowedExtensions": [".png", ".jpg", ".jpeg", ".pdf", ".txt", ".log", ".csv", ".zip", ".docx", ".xlsx"]
  }
}
```

For production, point `StorageRootPath` to a persistent volume outside the
deployed application bundle and public web root. Back up the attachment volume
together with the database so metadata and files remain consistent.

## Migration

From the `backend` directory:

```bash
dotnet ef database update \
  --project HelpDesk.Infrastructure/HelpDesk.Infrastructure.csproj \
  --startup-project HelpDesk.Api/HelpDesk.Api.csproj
```
