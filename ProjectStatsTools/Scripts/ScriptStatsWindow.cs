#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProjectInfoStatsTools {
    public class ScriptStatsWindow : EditorWindow {
        private enum Tab {
            Overview,
            CodeAnalysis,
            Quality,
            History,
            Settings,
        }
        private Tab currentTab = Tab.Overview;
        private ScriptStats stats;
        private HistoricalData history = new HistoricalData();
        private Vector2 scrollPos;
        private string folderPath = "Assets";
        private static string dataLocalPathEditor = "Editor";
        private static string dataLocalPathPlugin = "ProjectStatsToolsData";
        private List<string> ignoreFolders = new List<string> {
            "Test",
            "Editor",
        };
        private string[] allFiles, prefabs, materials, scenes, textures, audioClips, videoClips, shaders, animationClips = null;
        private Dictionary<int, string> allTypesTextures = new Dictionary<int, string> {
            { 1, "png" },
            { 2, "jpg" },
            { 3, "jpeg" },
            { 4, "svg" },
            { 5, "gif" },
            { 6, "webp" },
            { 7, "avif" },
            { 8, "heif" },
        };
        private Dictionary<string, bool> fileTypeFilters = new Dictionary<string, bool> {
            { ".cs", true },
            { ".shader", false },
            { ".json", false },
        };
        private string searchQuery = "";
        private bool enableRealTimeUpdates = false;
        private string logFilePath = Path.Combine(getPathLocal(), "ProjectStatsLog.txt");

        [MenuItem("Tools/Project Info Stats")]
        public static void ShowWindow() {
            GetWindow<ScriptStatsWindow>("Project Info Stats");
        }

        private void OnEnable() {
            EditorApplication.update += RealTimeUpdate;
        }

        private void OnDisable() {
            EditorApplication.update -= RealTimeUpdate;
        }

        private void OnGUI() {
            currentTab = (Tab)GUILayout.Toolbar((int)currentTab, Enum.GetNames(typeof(Tab)));
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            switch(currentTab) {
                case Tab.Overview: DrawOverview(); break;
                case Tab.CodeAnalysis: DrawCodeAnalysis(); break;
                case Tab.Quality: DrawQuality(); break;
                case Tab.History: DrawHistory(); break;
                case Tab.Settings: DrawSettings(); break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawOverview() {
            if(GUILayout.Button("Analyze Project")) {
                AnalyzeProject();
            }

            GUILayout.Label("Project Metadata", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Project Name:", stats == null ? "No data" : stats.projectName);
            EditorGUILayout.LabelField("Project Version:", stats == null ? "No data" : stats.projectVersion);
            EditorGUILayout.LabelField("Target Platform:", stats == null ? "No data" : stats.targetPlatform);

            GUILayout.Space(20);

            EditorGUILayout.LabelField("Files Analysis", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Total Files:", stats == null ? "No data" : stats.TotalFiles.ToString());
            EditorGUILayout.LabelField("Total Lines:", stats == null ? "No data" : stats.TotalLines.ToString());
            EditorGUILayout.LabelField("Average Lines:", stats == null ? "No data" : stats.TotalAverageLines.ToString());
            EditorGUILayout.LabelField("Empty Lines:", stats == null ? "No data" : stats.TotalEmptyLines.ToString());
            EditorGUILayout.LabelField("Comment Lines:", stats == null ? "No data" : stats.TotalCommentLines.ToString());

            GUILayout.Space(20);

            EditorGUILayout.LabelField("Code Analysis", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Total Namespaces:", stats == null ? "No data" : stats.TotalNamespaces.ToString());
            EditorGUILayout.LabelField("Total Classes:", stats == null ? "No data" : stats.TotalClasses.ToString());
            EditorGUILayout.LabelField("Total Interfaces:", stats == null ? "No data" : stats.TotalInterface.ToString());
            EditorGUILayout.LabelField("Total Methods:", stats == null ? "No data" : stats.TotalMethods.ToString());
            EditorGUILayout.LabelField("Total Enums:", stats == null ? "No data" : stats.TotalEnum.ToString());
            EditorGUILayout.LabelField("Total Variables:", stats == null ? "No data" : stats.TotalVariables.ToString());
            EditorGUILayout.LabelField("Total Structs:", stats == null ? "No data" : stats.TotalStruct.ToString());

            GUILayout.Space(20);

            GUILayout.Label("Asset Analysis", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Total Prefabs:", stats == null ? "No data" : stats.TotalPrefabs.ToString());
            EditorGUILayout.LabelField("Total Materials:", stats == null ? "No data" : stats.TotalMaterials.ToString());
            EditorGUILayout.LabelField("Total Scenes:", stats == null ? "No data" : stats.TotalScenes.ToString());
            EditorGUILayout.LabelField("Total Textures:", stats == null ? "No data" : stats.TotalTextures.ToString());
            EditorGUILayout.LabelField("Total Audio Clips:", stats == null ? "No data" : stats.TotalAudioClips.ToString());
            EditorGUILayout.LabelField("Total Video Clips:", stats == null ? "No data" : stats.TotalVideoClips.ToString());
            EditorGUILayout.LabelField("Total Shaders:", stats == null ? "No data" : stats.TotalShaders.ToString());
            EditorGUILayout.LabelField("Total Animation Clips:", stats == null ? "No data" : stats.TotalAnimationClips.ToString());
            EditorGUILayout.LabelField("Total Asset Size:", stats == null ? "No data" : TransformSize(stats.TotalAssetSizes));

            GUILayout.Space(20);

            if(stats != null) {
                GUILayout.Label("Results saved to ProjectAnalysisLog.txt:", EditorStyles.boldLabel);
                if(File.Exists(logFilePath)) {
                    if(GUILayout.Button("Open Log File")) {
                        EditorUtility.RevealInFinder(logFilePath);
                    }
                }

                GUILayout.Space(20);

                GUILayout.Label("Export to select formats:", EditorStyles.boldLabel);
                if(GUILayout.Button("Export to CSV")) {
                    ExportToCSV();
                }

                if(GUILayout.Button("Generate HTML Export")) {
                    GenerateHTMLExport();
                }
            }
        }

        private void DrawCodeAnalysis() {
            if(stats == null) {
                return;
            }

            EditorGUILayout.LabelField("Code Analysis", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Classes: {stats.TotalClasses}");
            EditorGUILayout.LabelField($"Methods: {stats.TotalMethods}");
            EditorGUILayout.LabelField($"Variables: {stats.TotalVariables}");
            EditorGUILayout.LabelField($"Namespaces: {stats.TotalNamespaces}");
            DrawFileList();
        }

        private void DrawQuality() {
            if(stats == null) {
                return;
            }

            EditorGUILayout.LabelField("Code Quality", EditorStyles.boldLabel);
            DrawFileList(file =>
                $"Complexity: {file.Quality.CyclomaticComplexity:F2}, " +
                $"Nesting: {file.Quality.MaxNestingDepth}, " +
                $"Maintainability: {file.Quality.MaintainabilityIndex:F2}");
        }

        private void DrawHistory() {
            if(File.Exists(GetHistoryPath())) {
                if(GUILayout.Button("Load History")) {
                    if(File.Exists(GetHistoryPath())) {
                        history = JsonUtility.FromJson<HistoricalData>(File.ReadAllText(GetHistoryPath()));
                    }

                    if(stats == null) {
                        stats = new ScriptStats();

                        foreach(var last in history.History) {
                            // Project Metadata
                            stats.projectName = last.projectName;
                            stats.projectVersion = last.projectVersion;
                            stats.targetPlatform = last.targetPlatform;

                            // Files Analysis
                            stats.TotalFiles = last.TotalFiles;
                            stats.TotalLines = last.TotalLines;
                            stats.TotalAverageLines = last.TotalAverageLines;
                            stats.TotalEmptyLines = last.TotalEmptyLines;
                            stats.TotalCommentLines = last.TotalCommentLines;

                            // Code Analysis
                            stats.TotalNamespaces = last.TotalNamespaces;
                            stats.TotalClasses = last.TotalClasses;
                            stats.TotalInterface = last.TotalInterface;
                            stats.TotalMethods = last.TotalMethods;
                            stats.TotalEnum = last.TotalEnum;
                            stats.TotalVariables = last.TotalVariables;
                            stats.TotalStruct = last.TotalStruct;

                            // Asset Analysis
                            stats.TotalPrefabs = last.TotalPrefabs;
                            stats.TotalMaterials = last.TotalMaterials;
                            stats.TotalScenes = last.TotalScenes;
                            stats.TotalTextures = last.TotalTextures;
                            stats.TotalAudioClips = last.TotalAudioClips;
                            stats.TotalVideoClips = last.TotalVideoClips;
                            stats.TotalShaders = last.TotalShaders;
                            stats.TotalAnimationClips = last.TotalAnimationClips;
                            stats.TotalAssetSizes = last.TotalAssetSizes;

                            // Code Files Analysis
                            stats.FileStats = last.FileStats;
                        }
                    }
                }

                foreach(var pastStats in history.History) {
                    EditorGUILayout.LabelField($"Date: {pastStats.LastAnalyzed}");
                    EditorGUILayout.LabelField($"Files: {pastStats.TotalFiles}, Lines: {pastStats.TotalLines}");
                }
            }
        }

        private void DrawSettings() {
            GUILayout.Label("Folder Path", EditorStyles.boldLabel);
            folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

            GUILayout.Space(20);

            EditorGUILayout.LabelField("File Type Filters", EditorStyles.boldLabel);
            foreach(var kvp in fileTypeFilters.ToList()) {
                fileTypeFilters[kvp.Key] = EditorGUILayout.Toggle(kvp.Key, kvp.Value);
            }

            EditorGUILayout.LabelField("Ignore Folders", EditorStyles.boldLabel);
            for(int i = 0; i < ignoreFolders.Count; i++) {
                ignoreFolders[i] = EditorGUILayout.TextField(ignoreFolders[i]);
            }

            if(GUILayout.Button("Add Pattern")) {
                ignoreFolders.Add("");
            }

            enableRealTimeUpdates = EditorGUILayout.Toggle("Real-Time Updates", enableRealTimeUpdates);
        }

        private void DrawFileList(Func<FileStat, string> details = null) {
            searchQuery = EditorGUILayout.TextField("Search", searchQuery);

            foreach(var file in stats.FileStats.Where(f => string.IsNullOrEmpty(searchQuery) ||
                Path.GetFileName(f.Path).Contains(searchQuery, StringComparison.OrdinalIgnoreCase))) {
                    EditorGUILayout.LabelField(Path.GetFileName(file.Path) + (details != null ? $": {details(file)}" : ""));
            }
        }

        private void AnalyzeProject() {
            stats = new ScriptStats();
            EditorUtility.DisplayProgressBar("Analyzing", "Scanning files...", 0f);

            // Code Files Analysis
            allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => fileTypeFilters.Any(ft => ft.Value && f.EndsWith(ft.Key)) &&
                           !ignoreFolders.Any(p => f.Contains(p))).ToArray();

            // Asset Analysis
            prefabs = Directory.GetFiles(folderPath, "*.prefab", SearchOption.AllDirectories)
                .Where(f => !ignoreFolders.Any(p => f.Contains(p))).ToArray();

            materials = Directory.GetFiles(folderPath, "*.mat", SearchOption.AllDirectories)
                .Where(f => !ignoreFolders.Any(p => f.Contains(p))).ToArray();

            scenes = Directory.GetFiles(folderPath, "*.unity", SearchOption.AllDirectories)
                .Where(f => !ignoreFolders.Any(p => f.Contains(p))).ToArray();

            textures = null;
            foreach(var item in allTypesTextures) {
                if(item.Key > 1) {
                    textures = textures.Concat(Directory.GetFiles(folderPath, "*." + item.Value, SearchOption.AllDirectories))
                        .Where(f => !ignoreFolders.Any(p => f.Contains(p))).ToArray();
                } else {
                    textures = Directory.GetFiles(folderPath, "*." + item.Value, SearchOption.AllDirectories)
                        .Where(f => !ignoreFolders.Any(p => f.Contains(p))).ToArray();
                }
            }

            audioClips = Directory.GetFiles(folderPath, "*.wav", SearchOption.AllDirectories)
                .Where(f => !ignoreFolders.Any(p => f.Contains(p))).ToArray();
            audioClips = audioClips.Concat(Directory.GetFiles(folderPath, "*.mp3", SearchOption.AllDirectories)).ToArray()
                .Where(f => !ignoreFolders.Any(p => f.Contains(p))).ToArray();

            videoClips = Directory.GetFiles(folderPath, "*.mp4", SearchOption.AllDirectories)
                .Where(f => !ignoreFolders.Any(p => f.Contains(p))).ToArray();
            videoClips = videoClips.Concat(Directory.GetFiles(folderPath, "*.avi", SearchOption.AllDirectories)).ToArray()
                .Where(f => !ignoreFolders.Any(p => f.Contains(p))).ToArray();
            videoClips = videoClips.Concat(Directory.GetFiles(folderPath, "*.mkv", SearchOption.AllDirectories)).ToArray()
                .Where(f => !ignoreFolders.Any(p => f.Contains(p))).ToArray();

            shaders = Directory.GetFiles(folderPath, "*.shader", SearchOption.AllDirectories)
                .Where(f => !ignoreFolders.Any(p => f.Contains(p))).ToArray();

            animationClips = Directory.GetFiles(folderPath, "*.anim", SearchOption.AllDirectories)
                .Where(f => !ignoreFolders.Any(p => f.Contains(p))).ToArray();

            for(int i = 0; i < allFiles.Length; i++) {
                string file = allFiles[i];
                FileStat fileStat = new FileStat{Path = file};
                string[] lines = File.ReadAllLines(file);
                bool inBlockComment = false;
                FileInfo fileInfo = new FileInfo(file);

                fileStat.LineCount = lines.Length;
                fileStat.Weight = fileInfo.Length;
                fileStat.Quality = CalculateQuality(lines);
                AnalyzeCodeComplexity(lines, fileStat);

                // Code Files Analysis
                stats.FileStats.Add(fileStat);
                stats.TotalLines += lines.Length;
                stats.TotalEmptyLines += lines.Count(line => string.IsNullOrWhiteSpace(line));

                foreach(string line in lines) {
                    string trimmedLine = line.Trim();

                    if(inBlockComment) {
                        if(trimmedLine.Contains("*/")) {
                            inBlockComment = false;
                            stats.TotalCommentLines += lines.Count(line => line.Trim().EndsWith("*/"));
                        }

                        continue;
                    }

                    if(trimmedLine.StartsWith("/*")) {
                        inBlockComment = true;
                        stats.TotalCommentLines += lines.Count(line => line.Trim().StartsWith("/*"));
                        continue;
                    }
                }
                stats.TotalCommentLines += lines.Count(line => line.Trim().StartsWith("//"));

                // Total Asset Analysis
                stats.TotalAssetSizes += fileInfo.Length;

                // Time click button Analysis
                stats.LastAnalyzed = $"{DateTime.Now}";

                EditorUtility.DisplayProgressBar("Analyzing", $"Processing {Path.GetFileName(file)}", (float)i / allFiles.Length);
            }

            // Total Asset Analysis
            if(prefabs.Length > 0) {
                for(int i = 0; i < prefabs.Length; i++) {
                    string file = prefabs[i];
                    FileInfo fileInfo = new FileInfo(file);

                    stats.TotalAssetSizes += fileInfo.Length;
                    EditorUtility.DisplayProgressBar("Analyzing", $"Processing {Path.GetFileName(file)}", (float)i / prefabs.Length);
                }
            }

            if(materials.Length > 0) {
                for(int i = 0; i < materials.Length; i++) {
                    string file = materials[i];
                    FileInfo fileInfo = new FileInfo(file);

                    stats.TotalAssetSizes += fileInfo.Length;
                    EditorUtility.DisplayProgressBar("Analyzing", $"Processing {Path.GetFileName(file)}", (float)i / materials.Length);
                }
            }

            if(scenes.Length > 0) {
                for(int i = 0; i < scenes.Length; i++) {
                    string file = scenes[i];
                    FileInfo fileInfo = new FileInfo(file);

                    stats.TotalAssetSizes += fileInfo.Length;
                    EditorUtility.DisplayProgressBar("Analyzing", $"Processing {Path.GetFileName(file)}", (float)i / scenes.Length);
                }
            }

            if(textures.Length > 0) {
                for(int i = 0; i < textures.Length; i++) {
                    string file = textures[i];
                    FileInfo fileInfo = new FileInfo(file);

                    stats.TotalAssetSizes += fileInfo.Length;
                    EditorUtility.DisplayProgressBar("Analyzing", $"Processing {Path.GetFileName(file)}", (float)i / textures.Length);
                }
            }

            if(audioClips.Length > 0) {
                for(int i = 0; i < audioClips.Length; i++) {
                    string file = audioClips[i];
                    FileInfo fileInfo = new FileInfo(file);

                    stats.TotalAssetSizes += fileInfo.Length;
                    EditorUtility.DisplayProgressBar("Analyzing", $"Processing {Path.GetFileName(file)}", (float)i / audioClips.Length);
                }
            }

            if(videoClips.Length > 0) {
                for(int i = 0; i < videoClips.Length; i++) {
                    string file = videoClips[i];
                    FileInfo fileInfo = new FileInfo(file);

                    stats.TotalAssetSizes += fileInfo.Length;
                    EditorUtility.DisplayProgressBar("Analyzing", $"Processing {Path.GetFileName(file)}", (float)i / videoClips.Length);
                }
            }

            if(shaders.Length > 0) {
                for(int i = 0; i < shaders.Length; i++) {
                    string file = shaders[i];
                    FileInfo fileInfo = new FileInfo(file);

                    stats.TotalAssetSizes += fileInfo.Length;
                    EditorUtility.DisplayProgressBar("Analyzing", $"Processing {Path.GetFileName(file)}", (float)i / shaders.Length);
                }
            }

            if(animationClips.Length > 0) {
                for(int i = 0; i < animationClips.Length; i++) {
                    string file = animationClips[i];
                    FileInfo fileInfo = new FileInfo(file);

                    stats.TotalAssetSizes += fileInfo.Length;
                    EditorUtility.DisplayProgressBar("Analyzing", $"Processing {Path.GetFileName(file)}", (float)i / animationClips.Length);
                }
            }

            // Project Metadata
            stats.projectName = PlayerSettings.productName;
            stats.projectVersion = PlayerSettings.bundleVersion;
            stats.targetPlatform = EditorUserBuildSettings.activeBuildTarget.ToString();

            // Files Analysis
            stats.TotalFiles = allFiles.Length;
            stats.TotalAverageLines = stats.TotalFiles > 0 ? (float)stats.TotalLines / stats.TotalFiles : 0;

            // Asset Analysis
            stats.TotalPrefabs = prefabs.Length;
            stats.TotalMaterials = materials.Length;
            stats.TotalScenes = scenes.Length;
            stats.TotalTextures = textures.Length;
            stats.TotalAudioClips = audioClips.Length;
            stats.TotalVideoClips = videoClips.Length;
            stats.TotalShaders = shaders.Length;
            stats.TotalAnimationClips = animationClips.Length;

            LogBuilder();
            SaveHistory();
            EditorUtility.ClearProgressBar();
            Repaint();
        }

        private void AnalyzeCodeComplexity(string[] lines, FileStat fileStat) {
            foreach(string line in lines) {
                string trimmed = line.Trim();

                if(trimmed.StartsWith("class ") || trimmed.Contains("class ")) {
                    stats.TotalClasses++;
                }

                if(trimmed.StartsWith("namespace ") || trimmed.Contains("namespace ")) {
                    stats.TotalNamespaces++;
                }

                if(trimmed.StartsWith("public interface ") || trimmed.Contains(" interface ")) {
                    stats.TotalInterface++;
                }

                if(trimmed.StartsWith("public enum ") || trimmed.Contains(" enum ")) {
                    stats.TotalEnum++;
                }

                if(trimmed.StartsWith("public struct ") || trimmed.Contains(" struct ")) {
                    stats.TotalStruct++;
                }

                if(trimmed.Contains("(") && trimmed.Contains(")") &&
                    !trimmed.StartsWith("if") && !trimmed.StartsWith("for") &&
                    !trimmed.StartsWith("while")) {
                        stats.TotalMethods++;
                }

                if(trimmed.Contains("=") && (trimmed.StartsWith("public") ||
                    trimmed.StartsWith("private") || trimmed.StartsWith("protected"))) {
                        stats.TotalVariables++;
                }
            }
        }

        private CodeQuality CalculateQuality(string[] lines) {
            CodeQuality quality = new CodeQuality();
            int decisionPoints = 0;
            int currentDepth = 0;

            foreach(string line in lines) {
                string trimmed = line.Trim();
                if(trimmed.Contains("if") || trimmed.Contains("for") || trimmed.Contains("while") ||
                    trimmed.Contains("case")) {
                        decisionPoints++;
                }

                if(trimmed.Contains("{")) {
                    currentDepth++;
                }

                if(trimmed.Contains("}")) {
                    currentDepth--;
                }

                quality.MaxNestingDepth = Mathf.Max(quality.MaxNestingDepth, currentDepth);
            }

            quality.CyclomaticComplexity = decisionPoints + 1;
            quality.MaintainabilityIndex = Mathf.Max(0, 171 - 5.2f * Mathf.Log(quality.CyclomaticComplexity));

            return quality;
        }

        private void LogBuilder() {
            StringBuilder logBuilder = new StringBuilder();

            logBuilder.AppendLine($"Project Stats LogFile - {DateTime.Now}");

            // Project Metadata
            logBuilder.AppendLine();
            logBuilder.AppendLine("Project Metadata");
            logBuilder.AppendLine($"Project Name: {stats.projectName}");
            logBuilder.AppendLine($"Project Version: {stats.projectVersion}");
            logBuilder.AppendLine($"Target Platform: {stats.targetPlatform}");

            // Files Analysis
            logBuilder.AppendLine();
            logBuilder.AppendLine("Files Analysis");
            logBuilder.AppendLine($"Total Files: {stats.TotalFiles}");
            logBuilder.AppendLine($"Total Lines: {stats.TotalLines}");
            logBuilder.AppendLine($"Average Lines: {stats.TotalAverageLines}");
            logBuilder.AppendLine($"Empty Lines: {stats.TotalEmptyLines}");
            logBuilder.AppendLine($"Comment Lines: {stats.TotalCommentLines}");

            // Code Analysis
            logBuilder.AppendLine();
            logBuilder.AppendLine("Code Analysis");
            logBuilder.AppendLine($"Total Namespaces: {stats.TotalNamespaces}");
            logBuilder.AppendLine($"Total Classes: {stats.TotalClasses}");
            logBuilder.AppendLine($"Total Interfaces: {stats.TotalInterface}");
            logBuilder.AppendLine($"Total Methods: {stats.TotalMethods}");
            logBuilder.AppendLine($"Total Enums: {stats.TotalEnum}");
            logBuilder.AppendLine($"Total Variables: {stats.TotalVariables}");
            logBuilder.AppendLine($"Total Structs: {stats.TotalStruct}");

            // Asset Analysis
            logBuilder.AppendLine();
            logBuilder.AppendLine("Asset Analysis");
            logBuilder.AppendLine($"Total Prefabs: {stats.TotalPrefabs}");
            logBuilder.AppendLine($"Total Materials: {stats.TotalMaterials}");
            logBuilder.AppendLine($"Total Scenes: {stats.TotalScenes}");
            logBuilder.AppendLine($"Total Textures: {stats.TotalTextures}");
            logBuilder.AppendLine($"Total Audio Clips: {stats.TotalAudioClips}");
            logBuilder.AppendLine($"Total Video Clips: {stats.TotalVideoClips}");
            logBuilder.AppendLine($"Total Shaders: {stats.TotalShaders}");
            logBuilder.AppendLine($"Total Animation Clips: {stats.TotalAnimationClips}");
            logBuilder.AppendLine($"Total Asset Size: {TransformSize(stats.TotalAssetSizes)}");

            // Code Files Analysis
            logBuilder.AppendLine();
            logBuilder.AppendLine("Code Files Analysis");
            logBuilder.AppendLine("File, Lines, Weight, Complexity, Nesting, Maintainability");

            foreach(var file in stats.FileStats) {
                logBuilder.AppendLine($"{Path.GetFileName(file.Path)}, {file.LineCount}, {TransformSize(file.Weight)}, " +
                    $"{file.Quality.CyclomaticComplexity}, {file.Quality.MaxNestingDepth}, " +
                    $"{file.Quality.MaintainabilityIndex}");
            }

            // Asset Files Analysis
            if(prefabs.Length > 0) {
                logBuilder.AppendLine();
                logBuilder.AppendLine("Prefabs:");

                var prefabFilesWithSizes = prefabs.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                foreach(var fileInfo in prefabFilesWithSizes) {
                    logBuilder.AppendLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                }
            }

            if(materials.Length > 0) {
                logBuilder.AppendLine();
                logBuilder.AppendLine("Materials:");

                var materialFilesWithSizes = materials.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                foreach(var fileInfo in materialFilesWithSizes) {
                    logBuilder.AppendLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                }
            }

            if(scenes.Length > 0) {
                logBuilder.AppendLine();
                logBuilder.AppendLine("Scenes:");

                var sceneFilesWithSizes = scenes.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                foreach(var fileInfo in sceneFilesWithSizes) {
                    logBuilder.AppendLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                }
            }

            if(textures.Length > 0) {
                logBuilder.AppendLine();
                logBuilder.AppendLine("Textures:");

                var textureFilesWithSizes = textures.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                foreach(var fileInfo in textureFilesWithSizes) {
                    logBuilder.AppendLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                }
            }

            if(audioClips.Length > 0) {
                logBuilder.AppendLine();
                logBuilder.AppendLine("Audio Clips:");

                var audioFilesWithSizes = audioClips.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                foreach(var fileInfo in audioFilesWithSizes) {
                    logBuilder.AppendLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                }
            }

            if(videoClips.Length > 0) {
                logBuilder.AppendLine();
                logBuilder.AppendLine("Video Clips:");

                var videoFilesWithSizes = videoClips.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                foreach(var fileInfo in videoFilesWithSizes) {
                    logBuilder.AppendLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                }
            }

            if(shaders.Length > 0) {
                logBuilder.AppendLine();
                logBuilder.AppendLine("Shaders:");

                var shaderFilesWithSizes = shaders.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                foreach(var fileInfo in shaderFilesWithSizes) {
                    logBuilder.AppendLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                }
            }

            if(animationClips.Length > 0) {
                logBuilder.AppendLine();
                logBuilder.AppendLine("Animation Clips:");

                var animationFilesWithSizes = animationClips.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                foreach(var fileInfo in animationFilesWithSizes) {
                    logBuilder.AppendLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                }
            }

            File.WriteAllText(logFilePath, logBuilder.ToString());
        }

        private void ExportToCSV() {
            string path = EditorUtility.SaveFilePanel("Save Stats", "", "ProjectInfo.csv", "csv");

            if(string.IsNullOrEmpty(path)) {
                return;
            }

            using (StreamWriter sw = new StreamWriter(path)) {
                sw.WriteLine("Project Stats Export - " + DateTime.Now);

                // Project Metadata
                sw.WriteLine();
                sw.WriteLine("Project Metadata");
                sw.WriteLine($"Project Name: {stats.projectName}, "
                    + $"Project Version: {stats.projectVersion}, "
                    + $"Target Platform: {stats.targetPlatform}");

                // Files Analysis
                sw.WriteLine();
                sw.WriteLine("Files Analysis");
                sw.WriteLine($"Total Files: {stats.TotalFiles}, "
                    + $"Total Lines: {stats.TotalLines}, "
                    + $"Average Lines: {stats.TotalAverageLines}, "
                    + $"Empty Lines: {stats.TotalEmptyLines}, "
                    + $"Comment Lines: {stats.TotalCommentLines}");

                // Code Analysis
                sw.WriteLine();
                sw.WriteLine("Code Analysis");
                sw.WriteLine($"Total Namespaces: {stats.TotalNamespaces}, "
                    + $"Total Classes: {stats.TotalClasses}, "
                    + $"Total Interfaces: {stats.TotalInterface}, "
                    + $"Total Methods: {stats.TotalMethods}, "
                    + $"Total Enums: {stats.TotalEnum}, "
                    + $"Total Variables: {stats.TotalVariables}, "
                    + $"Total Structs: {stats.TotalStruct}");

                // Asset Analysis
                sw.WriteLine();
                sw.WriteLine("Asset Analysis");
                sw.WriteLine($"Total Prefabs: {stats.TotalPrefabs}, "
                    + $"Total Materials: {stats.TotalMaterials}, "
                    + $"Total Scenes: {stats.TotalScenes}, "
                    + $"Total Textures: {stats.TotalTextures}, "
                    + $"Total Audio Clips: {stats.TotalAudioClips}, "
                    + $"Total Video Clips: {stats.TotalVideoClips}, "
                    + $"Total Shaders: {stats.TotalShaders}, "
                    + $"Total Animation Clips: {stats.TotalAnimationClips}, "
                    + $"Total Asset Size: {TransformSize(stats.TotalAssetSizes)}");

                // Code Files Analysis
                sw.WriteLine();
                sw.WriteLine("Code Files Analysis");
                sw.WriteLine("File, Lines, Weight, Complexity, Nesting, Maintainability");

                foreach(var file in stats.FileStats) {
                    sw.WriteLine($"{Path.GetFileName(file.Path)}, {file.LineCount}, {TransformSize(file.Weight)}, " +
                        $"{file.Quality.CyclomaticComplexity}, {file.Quality.MaxNestingDepth}, " +
                        $"{file.Quality.MaintainabilityIndex}");
                }

                // Asset Files Analysis
                if(prefabs.Length > 0) {
                    sw.WriteLine();
                    sw.WriteLine("Prefabs:");

                    var prefabFilesWithSizes = prefabs.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in prefabFilesWithSizes) {
                        sw.WriteLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                    }
                }

                if(materials.Length > 0) {
                    sw.WriteLine();
                    sw.WriteLine("Materials:");

                    var materialFilesWithSizes = materials.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in materialFilesWithSizes) {
                        sw.WriteLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                    }
                }

                if(scenes.Length > 0) {
                    sw.WriteLine();
                    sw.WriteLine("Scenes:");

                    var sceneFilesWithSizes = scenes.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in sceneFilesWithSizes) {
                        sw.WriteLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                    }
                }

                if(textures.Length > 0) {
                    sw.WriteLine();
                    sw.WriteLine("Textures:");

                    var textureFilesWithSizes = textures.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in textureFilesWithSizes) {
                        sw.WriteLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                    }
                }

                if(audioClips.Length > 0) {
                    sw.WriteLine();
                    sw.WriteLine("Audio Clips:");

                    var audioFilesWithSizes = audioClips.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in audioFilesWithSizes) {
                        sw.WriteLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                    }
                }

                if(videoClips.Length > 0) {
                    sw.WriteLine();
                    sw.WriteLine("Video Clips:");

                    var videoFilesWithSizes = videoClips.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in videoFilesWithSizes) {
                        sw.WriteLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                    }
                }

                if(shaders.Length > 0) {
                    sw.WriteLine();
                    sw.WriteLine("Shaders:");

                    var shaderFilesWithSizes = shaders.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in shaderFilesWithSizes) {
                        sw.WriteLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                    }
                }

                if(animationClips.Length > 0) {
                    sw.WriteLine();
                    sw.WriteLine("Animation Clips:");

                    var animationFilesWithSizes = animationClips.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in animationFilesWithSizes) {
                        sw.WriteLine($"{fileInfo.Name} - {TransformSize(fileInfo.Length)}");
                    }
                }
            }

            EditorUtility.RevealInFinder(path);
        }

        private void GenerateHTMLExport() {
            string path = EditorUtility.SaveFilePanel("Save HTML Export", "", "ProjectInfo.html", "html");

            if(string.IsNullOrEmpty(path)) {
                return;
            }

            using (StreamWriter sw = new StreamWriter(path)) {
                sw.WriteLine("<html><head><style>body {font-family: Arial;} table {border-collapse: collapse;} " +
                    "th, td {border: 1px solid gray; padding: 5px;}</style></head><body>");

                sw.WriteLine($"<h2 style='margin: 20px 0;'>Project Stats Export - {DateTime.Now}</h2>");

                // Project Metadata
                sw.WriteLine("<table>");
                sw.WriteLine("<tr style='text-align: center;'><td colspan='2'><strong>Project Metadata</strong></td></tr>");
                sw.WriteLine($"<tr><td>Project Name:</td> <td>{stats.projectName}</td></tr>");
                sw.WriteLine($"<tr><td>Project Version:</td> <td>{stats.projectVersion}</td></tr>");
                sw.WriteLine($"<tr><td>Target Platform:</td> <td>{stats.targetPlatform}</td></tr>");

                // Files Analysis
                sw.WriteLine("<tr style='text-align: center;'><td colspan='2'><strong>Files Analysis</strong></td></tr>");
                sw.WriteLine($"<tr><td>Total Files:</td> <td>{stats.TotalFiles}</td></tr>");
                sw.WriteLine($"<tr><td>Total Lines:</td> <td>{stats.TotalLines}</td></tr>");
                sw.WriteLine($"<tr><td>Average Lines:</td> <td>{stats.TotalAverageLines}</td></tr>");
                sw.WriteLine($"<tr><td>Empty Lines:</td> <td>{stats.TotalEmptyLines}</td></tr>");
                sw.WriteLine($"<tr><td>Comment Lines:</td> <td>{stats.TotalCommentLines}</td></tr>");

                // Code Analysis
                sw.WriteLine("<tr style='text-align: center;'><td colspan='2'><strong>Code Analysis</strong></td></tr>");
                sw.WriteLine($"<tr><td>Total Namespaces:</td> <td>{stats.TotalNamespaces}</td></tr>");
                sw.WriteLine($"<tr><td>Total Classes:</td> <td>{stats.TotalClasses}</td></tr>");
                sw.WriteLine($"<tr><td>Total Interfaces:</td> <td>{stats.TotalInterface}</td></tr>");
                sw.WriteLine($"<tr><td>Total Methods:</td> <td>{stats.TotalMethods}</td></tr>");
                sw.WriteLine($"<tr><td>Total Enums:</td> <td>{stats.TotalEnum}</td></tr>");
                sw.WriteLine($"<tr><td>Total Variables:</td> <td>{stats.TotalVariables}</td></tr>");
                sw.WriteLine($"<tr><td>Total Structs:</td> <td>{stats.TotalStruct}</td></tr>");

                // Asset Analysis
                sw.WriteLine("<tr style='text-align: center;'><td colspan='2'><strong>Asset Analysis</strong></td></tr>");
                sw.WriteLine($"<tr><td>Total Prefabs:</td> <td>{stats.TotalPrefabs}</td></tr>");
                sw.WriteLine($"<tr><td>Total Materials:</td> <td>{stats.TotalMaterials}</td></tr>");
                sw.WriteLine($"<tr><td>Total Scenes:</td> <td>{stats.TotalScenes}</td></tr>");
                sw.WriteLine($"<tr><td>Total Textures:</td> <td>{stats.TotalTextures}</td></tr>");
                sw.WriteLine($"<tr><td>Total Audio Clips:</td> <td>{stats.TotalAudioClips}</td></tr>");
                sw.WriteLine($"<tr><td>Total Video Clips:</td> <td>{stats.TotalVideoClips}</td></tr>");
                sw.WriteLine($"<tr><td>Total Shaders:</td> <td>{stats.TotalShaders}</td></tr>");
                sw.WriteLine($"<tr><td>Total Animation Clips:</td> <td>{stats.TotalAnimationClips}</td></tr>");
                sw.WriteLine($"<tr><td>Total Asset Size:</td> <td>{TransformSize(stats.TotalAssetSizes)}</td></tr>");
                sw.WriteLine("</table>");

                // Code Files Analysis
                sw.WriteLine("<br />");
                sw.WriteLine("<table>");
                sw.WriteLine("<tr style='text-align: center;'><td colspan='6'><strong>Code Files Analysis</strong></td></tr>");
                sw.WriteLine("<tr style='text-align: center;'><td>File</td><td>Lines</td><td>Weight</td><td>Complexity</td><td>Nesting</td><td>Maintainability</tr>");

                foreach(var file in stats.FileStats) {
                    sw.WriteLine($"<tr><td>{Path.GetFileName(file.Path)}</td><td>{file.LineCount}</td><td>{TransformSize(file.Weight)}</td>" +
                        $"<td>{file.Quality.CyclomaticComplexity}</td><td>{file.Quality.MaxNestingDepth}</td>" +
                        $"<td>{file.Quality.MaintainabilityIndex}</td></tr>");
                }

                sw.WriteLine("</table>");

                // Asset Files Analysis
                if(prefabs.Length > 0) {
                    sw.WriteLine("<br />");
                    sw.WriteLine("<table>");
                    sw.WriteLine("<tr style='text-align: center;'><td colspan='2'><strong>Prefabs</strong></td></tr>");

                    var prefabFilesWithSizes = prefabs.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in prefabFilesWithSizes) {
                        sw.WriteLine($"<tr><td>{fileInfo.Name}</td> <td>{TransformSize(fileInfo.Length)}</td></tr>");
                    }

                    sw.WriteLine("</table>");
                }

                if(materials.Length > 0) {
                    sw.WriteLine("<br />");
                    sw.WriteLine("<table>");
                    sw.WriteLine("<tr style='text-align: center;'><td colspan='2'><strong>Materials</strong></td></tr>");

                    var materialFilesWithSizes = materials.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in materialFilesWithSizes) {
                        sw.WriteLine($"<tr><td>{fileInfo.Name}</td> <td>{TransformSize(fileInfo.Length)}</td></tr>");
                    }

                    sw.WriteLine("</table>");
                }

                if(scenes.Length > 0) {
                    sw.WriteLine("<br />");
                    sw.WriteLine("<table>");
                    sw.WriteLine("<tr style='text-align: center;'><td colspan='2'><strong>Scenes</strong></td></tr>");

                    var sceneFilesWithSizes = scenes.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in sceneFilesWithSizes) {
                        sw.WriteLine($"<tr><td>{fileInfo.Name}</td> <td>{TransformSize(fileInfo.Length)}</td></tr>");
                    }

                    sw.WriteLine("</table>");
                }

                if(textures.Length > 0) {
                    sw.WriteLine("<br />");
                    sw.WriteLine("<table>");
                    sw.WriteLine("<tr style='text-align: center;'><td colspan='2'><strong>Textures</strong></td></tr>");

                    var textureFilesWithSizes = textures.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in textureFilesWithSizes) {
                        sw.WriteLine($"<tr><td>{fileInfo.Name}</td> <td>{TransformSize(fileInfo.Length)}</td></tr>");
                    }

                    sw.WriteLine("</table>");
                }

                if(audioClips.Length > 0) {
                    sw.WriteLine("<br />");
                    sw.WriteLine("<table>");
                    sw.WriteLine("<tr style='text-align: center;'><td colspan='2'><strong>Audio Clips</strong></td></tr>");

                    var audioFilesWithSizes = audioClips.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in audioFilesWithSizes) {
                        sw.WriteLine($"<tr><td>{fileInfo.Name}</td> <td>{TransformSize(fileInfo.Length)}</td></tr>");
                    }

                    sw.WriteLine("</table>");
                }

                if(videoClips.Length > 0) {
                    sw.WriteLine("<br />");
                    sw.WriteLine("<table>");
                    sw.WriteLine("<tr style='text-align: center;'><td colspan='2'><strong>Video Clips</strong></td></tr>");

                    var videoFilesWithSizes = videoClips.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in videoFilesWithSizes) {
                        sw.WriteLine($"<tr><td>{fileInfo.Name}</td> <td>{TransformSize(fileInfo.Length)}</td></tr>");
                    }

                    sw.WriteLine("</table>");
                }

                if(shaders.Length > 0) {
                    sw.WriteLine("<br />");
                    sw.WriteLine("<table>");
                    sw.WriteLine("<tr style='text-align: center;'><td colspan='2'><strong>Shaders</strong></td></tr>");

                    var shaderFilesWithSizes = shaders.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in shaderFilesWithSizes) {
                        sw.WriteLine($"<tr><td>{fileInfo.Name}</td> <td>{TransformSize(fileInfo.Length)}</td></tr>");
                    }

                    sw.WriteLine("</table>");
                }

                if(animationClips.Length > 0) {
                    sw.WriteLine("<br />");
                    sw.WriteLine("<table>");
                    sw.WriteLine("<tr style='text-align: center;'><td colspan='2'><strong>Animation Clips</strong></td></tr>");

                    var animationFilesWithSizes = animationClips.Select(file => new FileInfo(file)).OrderByDescending(fi => fi.Length);

                    foreach(var fileInfo in animationFilesWithSizes) {
                        sw.WriteLine($"<tr><td>{fileInfo.Name}</td> <td>{TransformSize(fileInfo.Length)}</td></tr>");
                    }

                    sw.WriteLine("</table>");
                }

                sw.WriteLine("</body></html>");
            }

            EditorUtility.RevealInFinder(path);
        }

        private void SaveHistory() {
            history.History.Add(stats);
            File.WriteAllText(GetHistoryPath(), JsonUtility.ToJson(history));
        }

        private string GetHistoryPath() {
            return Path.Combine(getPathLocal(), "ProjectInfoHistory.json");
        }

        private void RealTimeUpdate() {
            if(!enableRealTimeUpdates || stats == null) {
                return;
            }

            if(EditorApplication.timeSinceStartup % 5 < 0.1){ // Check every 5 seconds
                AnalyzeProject();
            }
        }

        private string TransformSize(float value) {
            string res;
            string valSize = "KB";

            value = value / 1024; // KB

            if(value >= 1000 && value < (1000 * 1024)) {
                value = value / 1000;
                valSize = "MB";
            } else if(value >= (1000 * 1024) && value < (1000 * 1024 * 1024)) {
                value = value / (1000 * 1024);
                valSize = "GB";
            } else if(value >= (1000 * 1024 * 1024)) {
                value = value / (1000 * 1024 * 1024);
                valSize = "TB";
            }

            res = Math.Round(value, 2).ToString() + " (" + valSize + ")";

            return res;
        }

        // Get path for given file
        private static string getPathLocal() {
            string path;

            #if UNITY_EDITOR
                path = Application.dataPath;
            #elif UNITY_ANDROID
                path = Application.persistentDataPath;
            #elif UNITY_IPHONE
                path = GetiPhoneDocumentsPath();
            #else
                path = Application.dataPath;
            #endif

            if(!Directory.Exists(path + "/" + dataLocalPathEditor)) {
                Directory.CreateDirectory(path + "/" + dataLocalPathEditor);
            }

            if(!Directory.Exists(path + "/" + dataLocalPathEditor + "/" + dataLocalPathPlugin)) {
                Directory.CreateDirectory(path + "/" + dataLocalPathEditor + "/" + dataLocalPathPlugin);
            }

            path = path + "/" + dataLocalPathEditor + "/" + dataLocalPathPlugin;

            return path;
        }

        // Get the path in iOS device
        private static string GetiPhoneDocumentsPath() {
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
            path = path.Substring(0, path.LastIndexOf("/"));

            return path + "/Documents";
        }
    }
}
#endif
