using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProjectInfoStatsTools {
    [Serializable]
    public class ScriptStats {
        public string projectName, projectVersion, targetPlatform = null;
        public int TotalFiles, TotalLines, TotalEmptyLines, TotalCommentLines = 0;
        public float TotalAverageLines, TotalAssetSizes = 0;
        public int TotalClasses, TotalMethods, TotalVariables, TotalNamespaces, TotalInterface, TotalEnum, TotalStruct = 0;
        public int TotalPrefabs, TotalMaterials, TotalScenes, TotalTextures, TotalAudioClips, TotalVideoClips, TotalShaders, TotalAnimationClips = 0;
        public List<FileStat> FileStats = new List<FileStat>();
        public string LastAnalyzed;
    }
}
