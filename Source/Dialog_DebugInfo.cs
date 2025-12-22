using UnityEngine;
using Verse;
using RimWorld;

namespace RimAI
{
    public class Dialog_DebugInfo : Window
    {
        private string text;
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(800f, 600f);

        public Dialog_DebugInfo(string text)
        {
            this.text = text;
            this.doCloseX = true;
            this.draggable = true;
            this.resizeable = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect btnRect = new Rect(0, 0, 150, 30);
            if (Widgets.ButtonText(btnRect, "Copy to Clipboard"))
            {
                GUIUtility.systemCopyBuffer = text;
                Messages.Message("Copied to clipboard.", MessageTypeDefOf.NeutralEvent, false);
            }

            Rect textRect = new Rect(0, 40, inRect.width, inRect.height - 40);
            float height = Text.CalcHeight(text, textRect.width - 16f);
            Rect viewRect = new Rect(0, 0, textRect.width - 16f, height);

            Widgets.BeginScrollView(textRect, ref scrollPosition, viewRect);
            Widgets.Label(viewRect, text);
            Widgets.EndScrollView();
        }
    }
}
