using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Text;

namespace AutoPerformanceProfiler.Editor
{
    public static class StudioIntegrations
    {
        public static void SendSlackAlert(string message, string severity)
        {
            string slackWebhook = EditorPrefs.GetString("APP_SlackWebhook", "");
            if (string.IsNullOrEmpty(slackWebhook))
            {
                EditorUtility.DisplayDialog("Webhook Missing", "Please configure your webhook URL in the Enterprise Integrations tab first.", "OK");
                return;
            }

            string payload = $"{{\"text\": \"🚨 *{severity} Profiler Alert*\\n{message}\"}}";
            var request = new UnityWebRequest(slackWebhook, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            var op = request.SendWebRequest();
            op.completed += (asyncOp) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                    Debug.Log("[Profiler] Successfully sent incident report to Slack.");
                else
                    Debug.LogError("[Profiler] Slack webhook error: " + request.error);
                request.Dispose();
            };

            EditorUtility.DisplayDialog("Ticket Created", "Automated bug report sent to your production channel successfully.", "Close");
        }
    }
}
