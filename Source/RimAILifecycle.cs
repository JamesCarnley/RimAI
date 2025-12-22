using System;
using UnityEngine;
using UnityEngine.Networking;
using Verse;

namespace RimAI
{
    // A hidden GameObject that survives scene loads and runs the Update loop
    public class RimAILifecycle : MonoBehaviour
    {
        private static RimAILifecycle instance;
        private UnityWebRequestAsyncOperation currentOperation;
        private UnityWebRequest currentRequest; // Hold reference to prevent GC

        public static void EnsureCreated()
        {
            if (instance != null) return;

            GameObject go = new GameObject("RimAILifecycle");
            UnityEngine.Object.DontDestroyOnLoad(go);
            instance = go.AddComponent<RimAILifecycle>();
            Log.Message("[RimAI] Lifecycle Manager Created.");
        }

        public void Update()
        {
            // Poll for results from HttpClient Thread
            if (APIClient.PendingResult != null)
            {
                CompleteSuccess(APIClient.PendingResult);
                APIClient.PendingResult = null;
            }
            else if (APIClient.PendingErrorText != null)
            {
                CompleteError(APIClient.PendingErrorText);
                APIClient.PendingErrorText = null;
            }
        }

        private void CompleteSuccess(string finalResponseText)
        {
            Log.Message("[RimAI] Lifecycle: HttpClient Success.");
            try 
            {
                // Already parsed in background. Pass directly to UI.
                APIClient.PendingSuccessCallback?.Invoke(finalResponseText);
            }
            catch (Exception ex)
            {
                Log.Error("[RimAI] Lifecycle Callback Error: " + ex);
                APIClient.PendingErrorCallback?.Invoke("Callback Error: " + ex.Message);
            }
            finally
            {
                Cleanup();
            }
        }

        private void CompleteError(string errorMsg)
        {
            Log.Error("[RimAI] Lifecycle: HttpClient Error: " + errorMsg);
            APIClient.PendingErrorCallback?.Invoke(errorMsg);
            Cleanup();
        }

        private void Cleanup()
        {
            APIClient.PendingSuccessCallback = null;
            APIClient.PendingErrorCallback = null;
        }
    }
}
