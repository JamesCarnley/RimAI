# RimAI: Colony Advisor

**RimAI** is a RimWorld mod that uses **Google Gemini 3.0 Flash** to analyze your colony and provide intelligent, narrative updates and recommendations. It scrapes data from your save state (Colonists, Resources, Events) and sends it to the AI for a situation report.

## Features
- **Intelligent Colony Analysis:** Reads your colonists' moods, health, and skills.
- **Narrative Reports:** Generates a story-based overview of your current situation.
- **Strategic Recommendations:** Provides 3 actionable bullet points based on your current threats and shortages.
- **Gemini 3.0 Flash:** Uses the latest, fastest model for near-instant responses.

## Installation
1. Download the latest release.
2. Unzip into your `RimWorld/Mods` folder.
3. Activate in the Mod Manager.
4. Go to **Mod Options > RimAI** and enter your Google Gemini API Key.
   - Get a key here: [Google AI Studio](https://aistudio.google.com/)

## For Developers: The "Zero-Touch" Networking Architecture

This mod includes a custom networking architecture designed to solve the **"Mono SIGKILL"** crash on macOS.

### The Problem
On macOS (Apple Silicon/Intel), the Mono runtime used by RimWorld crashes (`SIGKILL`/`Abort`) if you attempt to use `UnityWebRequest` with large JSON payloads on the main thread. 
- **Cause:** Large memory allocation/marshalling conflicts between Managed Mono and Unity's Native networking stack.
- **Symptoms:** Instant crash to desktop when sending the POST request.

### The Solution
We implemented a **Synchronous Background HttpClient** pattern that completely bypasses Unity's networking and the main thread's memory limits for large strings.

1.  **Background Threading:** All networking happens in a `Task.Run()`.
2.  **Standard .NET Library:** We use `System.Net.Http.HttpClient` instead of `UnityWebRequest`.
3.  **Synchronous Execution:** Inside the background thread, we use synchronous calls (`.GetAwaiter().GetResult()`) because the Mono runtime's Async State Machine generation is also unstable on background threads in this environment.
4.  **Background Parsing:** We deserialize the massive JSON response from Google *inside the background thread* using `Newtonsoft.Json.Linq.JObject`. We extract only the small summary text.
5.  **Clean Hand-off:** Only the small summary text is passed back to the Main Thread via a static flag, preventing memory marshalling crashes.

### Code Example
```csharp
// APIClient.cs
Task.Run(() => {
    using (var client = new HttpClient()) {
        // Synchronous Send
        var response = client.PostAsync(url, content).GetAwaiter().GetResult();
        var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        
        // Background Parse
        var json = JObject.Parse(body);
        string text = json["candidates"][0]["content"]["parts"][0]["text"].ToString();
        
        // Hand-off
        PendingResult = text;
    }
});
```

## License
MIT
