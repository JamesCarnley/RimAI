using System.Text.RegularExpressions;
using Verse;

namespace RimAI
{
    public static class MarkdownUtils
    {
        public static string ToRichText(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return "";

            string text = markdown;

            // Headers: # Header -> <size=20><b>Header</b></size>\n
            text = Regex.Replace(text, @"^#\s+(.*?)$", "<size=20><b>$1</b></size>\n", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^##\s+(.*?)$", "<size=18><b>$1</b></size>\n", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^###\s+(.*?)$", "<size=16><b>$1</b></size>\n", RegexOptions.Multiline);

            // Bold: **text** -> <b>text</b>
            text = Regex.Replace(text, @"\*\*(.*?)\*\*", "<b>$1</b>");

            // Italic: *text* -> <i>text</i>
            text = Regex.Replace(text, @"\*(.*?)\*", "<i>$1</i>");

            // Lists: - Item -> • Item
            text = Regex.Replace(text, @"^\s*-\s+(.*?)$", "  • $1", RegexOptions.Multiline);

            // Paragraphs? Usually RimWorld handles newlines fine, but we might want to ensure double newlines are respected.
            // For now, let's keep it simple.

            return text;
        }
    }
}
