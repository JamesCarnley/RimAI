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

            // Headers (### Header) -> <b><color=yellow>Header</color></b>
            // We use Regex to catch lines starting with #
            text = System.Text.RegularExpressions.Regex.Replace(text, @"^#{1,6}\s*(.+)$", "<b><color=#D4BA75>$1</color></b>", System.Text.RegularExpressions.RegexOptions.Multiline);

            // Bold (**text**)
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\*\*(.+?)\*\*", "<b>$1</b>");
            
            // Italic (*text*)
            // This can sometimes conflict with lists if not careful, but basic one works
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\*(.+?)\*", "<i>$1</i>");

            // Lists
            // - Item -> • Item (just purely visual cleanup)
            text = System.Text.RegularExpressions.Regex.Replace(text, @"^\s*-\s+(.+)$", "  • $1", System.Text.RegularExpressions.RegexOptions.Multiline);

            // Paragraphs? Usually RimWorld handles newlines fine, but we might want to ensure double newlines are respected.
            // For now, let's keep it simple.

            return text;
        }
    }
}
