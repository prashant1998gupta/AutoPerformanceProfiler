using UnityEngine;
using UnityEditor;
using AutoPerformanceProfiler.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace AutoPerformanceProfiler.Editor
{
    /// <summary>
    /// Generates a beautiful, interactive HTML Performance Report Card
    /// that can be shared with team leads, producers, and stakeholders.
    /// </summary>
    public static class PerformanceReportCardGenerator
    {
        public static void GenerateReportCard(ProfilerReport report, List<ObjectOffender> sceneOffenders = null)
        {
            if (report == null)
            {
                EditorUtility.DisplayDialog("Report Card", "No profiler report loaded. Run a profiling session first.", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanel("Save Performance Report Card", "", report.sessionName + "_ReportCard", "html");
            if (string.IsNullOrEmpty(path)) return;

            EditorUtility.DisplayProgressBar("📄 Report Card", "Generating professional HTML report...", 0.5f);

            float healthScore = CalculateScore(report, sceneOffenders);
            string grade = GetGrade(healthScore);
            string gradeColor = GetGradeHexColor(grade);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html lang='en'><head><meta charset='utf-8'>");
            sb.AppendLine($"<title>Performance Report Card — {report.sessionName}</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(@"
* { margin: 0; padding: 0; box-sizing: border-box; }
body { background: linear-gradient(135deg, #0f0c29, #302b63, #24243e); color: #e0e0e0; font-family: 'Segoe UI', system-ui, sans-serif; padding: 40px; min-height: 100vh; }
.container { max-width: 1100px; margin: 0 auto; }
.header { text-align: center; margin-bottom: 40px; }
.header h1 { font-size: 36px; background: linear-gradient(90deg, #4fc3f7, #81c784); -webkit-background-clip: text; -webkit-text-fill-color: transparent; margin-bottom: 8px; }
.header p { color: #888; font-size: 14px; }
.grade-hero { display: flex; align-items: center; justify-content: center; gap: 40px; margin-bottom: 40px; padding: 40px; background: rgba(255,255,255,0.05); border-radius: 20px; backdrop-filter: blur(20px); border: 1px solid rgba(255,255,255,0.1); }
.grade-circle { width: 160px; height: 160px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 64px; font-weight: 900; border: 6px solid; }
.grade-info h2 { font-size: 28px; margin-bottom: 8px; }
.grade-info p { color: #aaa; font-size: 16px; line-height: 1.6; }
.progress-bar { width: 100%; height: 12px; background: #1a1a2e; border-radius: 6px; margin-top: 15px; overflow: hidden; }
.progress-fill { height: 100%; border-radius: 6px; transition: width 0.5s; }
.metrics { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 20px; margin-bottom: 40px; }
.metric-card { background: rgba(255,255,255,0.05); border-radius: 12px; padding: 24px; border: 1px solid rgba(255,255,255,0.08); }
.metric-card .label { font-size: 12px; color: #888; text-transform: uppercase; letter-spacing: 1px; margin-bottom: 8px; }
.metric-card .value { font-size: 32px; font-weight: 700; }
.metric-card .sub { font-size: 11px; color: #666; margin-top: 4px; }
.section { margin-bottom: 40px; }
.section h2 { font-size: 22px; margin-bottom: 16px; padding-bottom: 8px; border-bottom: 1px solid rgba(255,255,255,0.1); }
.issue-card { background: rgba(255,255,255,0.03); border-radius: 8px; padding: 16px; margin-bottom: 10px; border-left: 4px solid; }
.issue-card.high { border-color: #f44336; }
.issue-card.medium { border-color: #ff9800; }
.issue-card.low { border-color: #4fc3f7; }
.issue-card .title { font-weight: 600; margin-bottom: 4px; }
.issue-card .desc { font-size: 13px; color: #999; }
.issue-card .fix { font-size: 12px; color: #81c784; margin-top: 6px; }
.chart { display: flex; align-items: flex-end; height: 200px; gap: 1px; margin: 20px 0; padding: 10px; background: rgba(0,0,0,0.3); border-radius: 8px; }
.chart .bar { flex: 1; min-width: 2px; border-radius: 2px 2px 0 0; transition: height 0.3s; }
.chart .bar:hover { opacity: 0.8; }
.footer { text-align: center; padding: 30px; color: #444; font-size: 12px; margin-top: 40px; border-top: 1px solid rgba(255,255,255,0.05); }
.tip-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.tip { background: rgba(79,195,247,0.08); border-radius: 8px; padding: 14px; border: 1px solid rgba(79,195,247,0.15); }
.tip .icon { font-size: 20px; margin-bottom: 6px; }
.tip .text { font-size: 13px; color: #ccc; line-height: 1.5; }
.badge { display: inline-block; padding: 2px 8px; border-radius: 4px; font-size: 11px; font-weight: 600; }
");
            sb.AppendLine("</style></head><body>");
            sb.AppendLine("<div class='container'>");

            // ── Header ──
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h1>🚀 Performance Report Card</h1>");
            sb.AppendLine($"<p>{report.sessionName} &bull; {report.sceneName} &bull; {report.duration:F1}s &bull; {report.totalFramesRecorded} frames &bull; {System.DateTime.Now:yyyy-MM-dd HH:mm}</p>");
            sb.AppendLine("</div>");

            // ── Grade Hero ──
            sb.AppendLine("<div class='grade-hero'>");
            sb.AppendLine($"<div class='grade-circle' style='color:{gradeColor};border-color:{gradeColor}'>{grade}</div>");
            sb.AppendLine("<div class='grade-info'>");
            sb.AppendLine($"<h2 style='color:{gradeColor}'>Optimization Score: {healthScore:F0}%</h2>");
            sb.AppendLine($"<p>{GetGradeDescription(grade)}</p>");
            sb.AppendLine($"<div class='progress-bar'><div class='progress-fill' style='width:{healthScore:F0}%;background:{gradeColor}'></div></div>");
            sb.AppendLine("</div></div>");

            // ── Metrics Grid ──
            sb.AppendLine("<div class='metrics'>");
            AddMetricCard(sb, "AVG FPS", $"{report.averageFPS:F0}", report.averageFPS >= 55 ? "#4caf50" : (report.averageFPS >= 30 ? "#ff9800" : "#f44336"), "Target: 60 FPS");
            AddMetricCard(sb, "PEAK RAM", $"{report.maxMemoryMB} MB", "#ab47bc", "System Memory");
            AddMetricCard(sb, "PEAK VRAM", $"{report.maxTextureMemoryMB} MB", "#ff7043", "Texture Memory");
            AddMetricCard(sb, "AVG CPU", $"{report.averageMainThreadMs:F1} ms", report.averageMainThreadMs < 16.6f ? "#4caf50" : "#f44336", "Target: <16.6ms");
            AddMetricCard(sb, "SCRIPTS", $"{report.averageScriptsMs:F1} ms", "#4fc3f7", "C# Logic");
            AddMetricCard(sb, "RENDERING", $"{report.averageRenderMs:F1} ms", "#e040fb", "Draw Call Prep");
            AddMetricCard(sb, "PHYSICS", $"{report.averagePhysicsMs:F1} ms", "#ffa726", "Collision/Rigid");
            
            int spikeCount = report.frames != null ? report.frames.Count(f => f.isSpikeFrame) : 0;
            AddMetricCard(sb, "LAG SPIKES", spikeCount.ToString(), spikeCount == 0 ? "#4caf50" : "#f44336", "Frame drops");
            sb.AppendLine("</div>");

            // ── FPS Chart ──
            if (report.frames != null && report.frames.Count > 0)
            {
                sb.AppendLine("<div class='section'>");
                sb.AppendLine("<h2>📈 FPS Over Time</h2>");
                sb.AppendLine("<div class='chart'>");
                float maxFps = Mathf.Max(60, report.frames.Max(f => f.fps));
                foreach (var f in report.frames)
                {
                    float h = (f.fps / maxFps) * 180f;
                    string color = f.fps >= 55 ? "#4caf50" : (f.fps >= 30 ? "#ff9800" : "#f44336");
                    if (f.isSpikeFrame) color = "#ff1744";
                    sb.AppendLine($"<div class='bar' style='height:{h:F0}px;background:{color}' title='FPS: {f.fps:F0} | CPU: {f.mainThreadTimeMs:F1}ms'></div>");
                }
                sb.AppendLine("</div></div>");

                // ── CPU Chart ──
                sb.AppendLine("<div class='section'>");
                sb.AppendLine("<h2>📉 Main Thread CPU (ms)</h2>");
                sb.AppendLine("<div class='chart'>");
                float maxCpu = Mathf.Max(16.6f, report.frames.Max(f => f.mainThreadTimeMs));
                foreach (var f in report.frames)
                {
                    float h = (f.mainThreadTimeMs / maxCpu) * 180f;
                    string color = f.mainThreadTimeMs > 16.6f ? "#f44336" : "#4fc3f7";
                    sb.AppendLine($"<div class='bar' style='height:{h:F0}px;background:{color}' title='CPU: {f.mainThreadTimeMs:F1}ms'></div>");
                }
                sb.AppendLine("</div></div>");
            }

            // ── Issues / Offenders ──
            if (sceneOffenders != null && sceneOffenders.Count > 0)
            {
                sb.AppendLine("<div class='section'>");
                sb.AppendLine($"<h2>🎯 Detected Issues ({sceneOffenders.Count})</h2>");
                foreach (var o in sceneOffenders.Take(20))
                {
                    string cssClass = o.severity == "High" ? "high" : (o.severity == "Medium" ? "medium" : "low");
                    sb.AppendLine($"<div class='issue-card {cssClass}'>");
                    sb.AppendLine($"<div class='title'><span class='badge' style='background:{(o.severity == "High" ? "#f44336" : "#ff9800")};color:#fff'>{o.severity}</span> {o.gameObjectName} — {o.componentName}</div>");
                    sb.AppendLine($"<div class='desc'>{o.issueDescription}</div>");
                    sb.AppendLine($"<div class='fix'>💡 {o.recommendedFix}</div>");
                    sb.AppendLine("</div>");
                }
                sb.AppendLine("</div>");
            }

            // ── Pro Tips ──
            sb.AppendLine("<div class='section'>");
            sb.AppendLine("<h2>💡 Pro Optimization Tips</h2>");
            sb.AppendLine("<div class='tip-grid'>");
            AddTip(sb, "🎮", "Use IL2CPP over Mono for 2-4x faster C# execution on all platforms.");
            AddTip(sb, "📱", "Enable Incremental GC to spread garbage collection across frames instead of freezing.");
            AddTip(sb, "🖼️", "Never use 4K textures on mobile. 1024-2048 is virtually indistinguishable on small screens.");
            AddTip(sb, "🔊", "Long audio clips (>10s) should use 'Compressed In Memory' or 'Streaming', never 'Decompress On Load'.");
            AddTip(sb, "💡", "Bake your lighting! Static lights should never cast realtime shadows.");
            AddTip(sb, "📐", "Disable Read/Write on meshes and textures you don't modify at runtime — saves 50% RAM.");
            AddTip(sb, "🎯", "Disable Raycast Target on all non-interactive UI elements (labels, backgrounds).");
            AddTip(sb, "⚡", "Object Pool frequently spawned objects instead of Instantiate/Destroy — eliminates GC spikes.");
            sb.AppendLine("</div></div>");

            // ── Device Info ──
            sb.AppendLine("<div class='section'>");
            sb.AppendLine("<h2>💻 Device Information</h2>");
            sb.AppendLine("<div class='metrics'>");
            AddMetricCard(sb, "DEVICE", report.deviceModel, "#78909c", "Hardware");
            AddMetricCard(sb, "OS", report.osVersion, "#78909c", "Operating System");
            AddMetricCard(sb, "DURATION", $"{report.duration:F1}s", "#78909c", "Recording Length");
            AddMetricCard(sb, "FRAMES", report.totalFramesRecorded.ToString(), "#78909c", "Total Recorded");
            sb.AppendLine("</div></div>");

            // ── Footer ──
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine($"Generated by <strong>Auto Performance Profiler Pro</strong> &bull; {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("</div>");

            sb.AppendLine("</div></body></html>");

            File.WriteAllText(path, sb.ToString());
            EditorUtility.ClearProgressBar();
            EditorUtility.RevealInFinder(path);
            Debug.Log($"[Profiler] ✅ Report Card generated at: {path}");
        }

        private static void AddMetricCard(System.Text.StringBuilder sb, string label, string value, string color, string sub)
        {
            sb.AppendLine($"<div class='metric-card'><div class='label'>{label}</div><div class='value' style='color:{color}'>{value}</div><div class='sub'>{sub}</div></div>");
        }

        private static void AddTip(System.Text.StringBuilder sb, string icon, string text)
        {
            sb.AppendLine($"<div class='tip'><div class='icon'>{icon}</div><div class='text'>{text}</div></div>");
        }

        private static float CalculateScore(ProfilerReport report, List<ObjectOffender> offenders)
        {
            float score = 100;
            if (report.averageFPS < 60) score -= (60 - report.averageFPS) * 1.5f;
            if (offenders != null)
            {
                score -= offenders.Count(o => o.severity == "High") * 12;
                score -= offenders.Count(o => o.severity == "Medium") * 4;
            }
            return Mathf.Clamp(score, 0, 100);
        }

        private static string GetGrade(float score)
        {
            if (score >= 90) return "A+";
            if (score >= 80) return "A";
            if (score >= 70) return "B";
            if (score >= 60) return "C";
            if (score >= 40) return "D";
            return "F";
        }

        private static string GetGradeHexColor(string grade)
        {
            if (grade.StartsWith("A")) return "#4caf50";
            if (grade == "B") return "#8bc34a";
            if (grade == "C") return "#ff9800";
            if (grade == "D") return "#ff5722";
            return "#f44336";
        }

        private static string GetGradeDescription(string grade)
        {
            if (grade == "A+") return "Outstanding! This project is production-ready with optimal performance across all metrics.";
            if (grade == "A") return "Excellent performance. Only minor polishing needed before shipping.";
            if (grade == "B") return "Good overall but there are some optimization opportunities worth investigating.";
            if (grade == "C") return "Acceptable but noticeable issues. Address the flagged items before release.";
            if (grade == "D") return "Below standard. Several critical optimizations are needed.";
            return "Critical. Major performance issues detected. Immediate action required.";
        }
    }
}
