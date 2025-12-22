using System;
using System.Collections.Generic;
using Verse;

namespace RimAI
{
    public class ChatMessage : IExposable
    {
        public string role; // "user" or "model"
        public string text;

        public ChatMessage() { }

        public ChatMessage(string role, string text)
        {
            this.role = role;
            this.text = text;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref role, "role");
            Scribe_Values.Look(ref text, "text");
        }
    }

    public class ChatSession : IExposable
    {
        public List<ChatMessage> messages = new List<ChatMessage>();
        public string colonyContextJson;

        public void AddUserMessage(string text)
        {
            messages.Add(new ChatMessage("user", text));
        }

        public void AddModelMessage(string text)
        {
            messages.Add(new ChatMessage("model", text));
        }

        public void Clear()
        {
            messages.Clear();
            colonyContextJson = null;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref messages, "messages", LookMode.Deep);
            Scribe_Values.Look(ref colonyContextJson, "colonyContextJson");
        }
    }
}
