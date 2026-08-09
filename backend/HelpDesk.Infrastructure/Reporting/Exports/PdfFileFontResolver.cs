using PdfSharp.Fonts;

namespace HelpDesk.Infrastructure.Reporting.Exports;

internal sealed class PdfFileFontResolver(
    string regularFontPath,
    string boldFontPath) : IFontResolver
{
    private const string RegularFace = "HelpDeskSans-Regular";
    private const string BoldFace = "HelpDeskSans-Bold";

    public FontResolverInfo ResolveTypeface(
        string familyName,
        bool isBold,
        bool isItalic) =>
        new(isBold ? BoldFace : RegularFace, false, isItalic);

    public byte[] GetFont(string faceName)
    {
        var path = faceName == BoldFace ? boldFontPath : regularFontPath;
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                "The configured reporting PDF font could not be found. " +
                "Configure Reporting:PdfRegularFontPath and Reporting:PdfBoldFontPath.",
                path);
        }
        return File.ReadAllBytes(path);
    }
}
