using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace RimAI
{
    public class MainTabWindow_Overview : MainTabWindow
    {
        private bool isLoading = false;
        private ChatSession chatSession = new ChatSession();
        private Vector2 scrollPosition = Vector2.zero;
        private string currentInput = "";
        
        public override Vector2 RequestedTabSize => new Vector2(600f, 700f);

        public MainTabWindow_Overview()
        {
            this.doCloseX = true;
            this.closeOnAccept = false;
            this.closeOnCancel = true; 
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, 0, inRect.width, 35), "Colony Overview & Chat");
            Text.Font = GameFont.Small;

            float y = 45f;

            // Buttons Row
            Rect btnRect = new Rect(0, y, 200, 30);
            if (Widgets.ButtonText(btnRect, "Analyze Colony (New Chat)"))
            {
                if (!isLoading)
                {
                    StartNewAnalysis();
                }
            }
            
            Rect debugBtnRect = new Rect(210, y, 100, 30);
            if (Widgets.ButtonText(debugBtnRect, "Debug Data"))
            {
                var data = DataScraper.Scrape();
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                Find.WindowStack.Add(new Dialog_DebugInfo(json));
            }

            Rect settingsBtnRect = new Rect(320, y, 100, 30);
            if (Widgets.ButtonText(settingsBtnRect, "Settings"))
            {
                 Find.WindowStack.Add(new Dialog_ModSettings(LoadedModManager.GetMod<RimAIMod>()));
            }

            y += 40f;

            // Chat Area
            Rect chatRect = new Rect(0, y, inRect.width, inRect.height - y - 110f); // Reserve more space (110f)
            Widgets.DrawMenuSection(chatRect);
            
            // Layout Constants
            float scrollBarWidth = 16f;
            float textPadding = 10f;
            float textWidth = chatRect.width - scrollBarWidth - textPadding;

            float contentHeight = 0f;
            foreach (var msg in chatSession.messages)
            {
                Text.Font = GameFont.Small;
                // Use consistent width. 
                // We add a moderate buffer (+10f) to the calculated height to account for font rendering differences
                // and rich text tags that might slightly alter line height.
                contentHeight += 24f + Text.CalcHeight(msg.text, textWidth) + 10f; 
            }
            
            if (isLoading) contentHeight += 40f;
            // No extra global buffer needed if per-message buffer is correct

            // Ensure viewRect is at least the size of the chatRect so it looks okay empty
            if (contentHeight < chatRect.height) contentHeight = chatRect.height;
            
            Rect viewRect = new Rect(0, 0, chatRect.width - scrollBarWidth, contentHeight);
            
            Widgets.BeginScrollView(chatRect, ref scrollPosition, viewRect);
            float curY = 5f;
            foreach (var msg in chatSession.messages)
            {
                Text.Font = GameFont.Small;
                string displayRole = msg.role == "user" ? "You" : "AI";
                
                // Header
                Rect rowRect = new Rect(5f, curY, textWidth, 24f);
                GUI.color = msg.role == "user" ? new Color(0.6f, 0.9f, 1f) : GenUI.MouseoverColor; 
                Widgets.Label(new Rect(rowRect.x, rowRect.y, 100f, rowRect.height), displayRole);
                GUI.color = Color.white;
                
                // Copy Button
                if (Widgets.ButtonText(new Rect(rowRect.xMax - 60f, rowRect.y, 60f, 18f), "Copy"))
                {
                    GUIUtility.systemCopyBuffer = msg.text;
                    Messages.Message("Copied to clipboard", MessageTypeDefOf.TaskCompletion, false);
                }

                curY += 24f;
                
                // Content
                float textHeight = Text.CalcHeight(msg.text, textWidth); 
                
                // Draw in the full 'textWidth'
                // Add the same buffer to the rect to ensure it acts as a container that won't clip
                Rect textRect = new Rect(5f, curY, textWidth, textHeight + 10f);
                Widgets.Label(textRect, msg.text);
                
                // Move cursor by the *actual* spacing logic used in contentHeight
                curY += textHeight + 10f; 
            }

            if (isLoading)
            {
                // Simple dot animation based on time
                int dots = (int)(Time.realtimeSinceStartup * 3) % 4;
                string dotStr = new string('.', dots);
                Widgets.Label(new Rect(5f, curY, viewRect.width, 25), $"<i>Contacting AI Satellite{dotStr}</i>");
            }
            
            Widgets.EndScrollView();

            // Input Area
            // Anchor to bottom of window to prevent floating
            float inputHeight = 80f;
            float inputY = inRect.height - inputHeight - 15f; // 15f padding from bottom
            Rect inputRect = new Rect(0, inputY, inRect.width - 85f, inputHeight);
            
            // 1. Handle Enter Key (Send)
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                if (!Event.current.shift && GUI.GetNameOfFocusedControl() == "ChatInput")
                {
                    Event.current.Use(); // Consume KeyDown
                    if (!currentInput.Trim().NullOrEmpty() && !isLoading)
                    {
                        SendMessage(currentInput.Trim());
                        currentInput = "";
                    }
                }
            }
            
            // 2. Consume stray character events for newline to prevent it appearing in the next frame
            // This is needed because Unity IMGUI sometimes fires a separate Character event for Return.
            if (Event.current.type == EventType.KeyDown && Event.current.character == '\n' && !Event.current.shift && GUI.GetNameOfFocusedControl() == "ChatInput")
            {
                 Event.current.Use();
            }
            
            GUI.SetNextControlName("ChatInput");
            currentInput = Widgets.TextArea(inputRect, currentInput);

            Rect sendRect = new Rect(inRect.width - 80f, inputY, 80f, 80f);
            if (Widgets.ButtonText(sendRect, "Send"))
            {
                if (!currentInput.Trim().NullOrEmpty() && !isLoading)
                {
                    SendMessage(currentInput.Trim());
                    currentInput = "";
                }
            }
        }
        
        public void StartNewAnalysis()
        {
            if (string.IsNullOrEmpty(RimAIMod.settings.apiKey))
            {
                Find.WindowStack.Add(new Dialog_MessageBox(
                    "You need to configure your Gemini API Key in the settings to use this feature.",
                    "Open Settings", 
                    () => Find.WindowStack.Add(new Dialog_ModSettings(LoadedModManager.GetMod<RimAIMod>())),
                    "Cancel", 
                    null, 
                    "Missing API Key"
                ));
                return;
            }

            isLoading = true;
            chatSession.Clear();
            
            try
            {
                // 1. Scrape Data
                var data = DataScraper.Scrape();
                chatSession.colonyContextJson = JsonConvert.SerializeObject(data);
                
                // 2. Initial Prompt
                string systemPrompt = $"Analyze this RimWorld colony state and provide a succinct, narrative overview (2 paragraphs max) and 3 bullet point recommendations. State:\n{chatSession.colonyContextJson}";
                
                SendToAPI(systemPrompt, isInitialAnalysis: true);
            }
            catch (System.Exception ex)
            {
                Log.Error($"[RimAI] Generation Error: {ex}");
                chatSession.AddModelMessage($"Error: {ex.Message}");
                isLoading = false;
            }
        }

        public void SendMessage(string text)
        {
            chatSession.AddUserMessage(text);
            isLoading = true;
            
            // Force scroll to bottom immediately so user sees their message
            scrollPosition.y = 99999f; 
            
            SendToAPI(text, isInitialAnalysis: false);
        }

        private void SendToAPI(string newText, bool isInitialAnalysis)
        {
            string apiKey = RimAIMod.settings.apiKey;
            
            var contentsList = new List<object>();

            if (isInitialAnalysis)
            {
                // For the very first request, we send the prompt + data as the user message.
                // We do NOT add this to the chat history visible to the user (it's too big).
                // The response will be added as the first Model message.
                contentsList.Add(new { role = "user", parts = new[] { new { text = newText } } });
            }
            else
            {
                // For follow ups, we must reconstruct the conversation properly.
                // The first message in the payload must be the Context+Prompt.
                
                string initialPrompt = $"Analyze this RimWorld colony state and provide a succinct, narrative overview (2 paragraphs max) and 3 bullet point recommendations. State:\n{chatSession.colonyContextJson}";
                contentsList.Add(new { role = "user", parts = new[] { new { text = initialPrompt } } });
                
                // Now append the rest of the history.
                // chatSession.messages contains:
                // [Model: Overview]
                // [User: Follow up]
                // ...
                
                foreach (var msg in chatSession.messages)
                {
                    contentsList.Add(new { role = msg.role, parts = new[] { new { text = msg.text } } });
                }
            }

            var payload = new { contents = contentsList };
            string finalJson = JsonConvert.SerializeObject(payload);

            APIClient.SendWithHttpClient(apiKey, finalJson,
                onSuccess: (result) => 
                {
                    string richText = MarkdownUtils.ToRichText(result);
                    chatSession.AddModelMessage(richText);
                    isLoading = false;
                    scrollPosition.y = 99999f; 
                },
                onError: (error) =>
                {
                    chatSession.AddModelMessage($"[Error] {error}");
                    isLoading = false;
                    scrollPosition.y = 99999f;
                }
            );
        }
    }
}
