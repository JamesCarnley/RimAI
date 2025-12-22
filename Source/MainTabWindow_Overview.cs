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
            
            Rect debugBtnRect = new Rect(210, y, 150, 30);
            if (Widgets.ButtonText(debugBtnRect, "Debug Data"))
            {
                var data = DataScraper.Scrape();
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                Find.WindowStack.Add(new Dialog_DebugInfo(json));
            }

            y += 40f;

            // Chat Area
            Rect chatRect = new Rect(0, y, inRect.width, inRect.height - y - 110f); // Reserve more space (110f)
            Widgets.DrawMenuSection(chatRect);
            
            float contentHeight = 0f;
            foreach (var msg in chatSession.messages)
            {
                // Header (20) + Text + Padding (15) = 35 overhead
                contentHeight += 20f + Text.CalcHeight(msg.text, chatRect.width - 26f) + 15f; 
            }
            
            if (isLoading) contentHeight += 40f;

            // Ensure viewRect is at least the size of the chatRect so it looks okay empty
            if (contentHeight < chatRect.height) contentHeight = chatRect.height;
            
            Rect viewRect = new Rect(0, 0, chatRect.width - 16f, contentHeight);
            
            Widgets.BeginScrollView(chatRect, ref scrollPosition, viewRect);
            float curY = 5f;
            foreach (var msg in chatSession.messages)
            {
                string displayRole = msg.role == "user" ? "You" : "AI";
                
                // Header
                Rect rollRect = new Rect(5f, curY, viewRect.width - 10f, 20f);
                GUI.color = msg.role == "user" ? new Color(0.6f, 0.9f, 1f) : GenUI.MouseoverColor; // Brighter Ice Blue for user, Highlight for AI
                Widgets.Label(rollRect, displayRole);
                GUI.color = Color.white;
                curY += 20f;
                
                // Content
                float textHeight = Text.CalcHeight(msg.text, viewRect.width - 10f);
                Rect textRect = new Rect(5f, curY, viewRect.width - 10f, textHeight);
                Widgets.Label(textRect, msg.text);
                
                curY += textHeight + 15f;
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
