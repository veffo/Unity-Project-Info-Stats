using UnityEngine;
using System;

namespace ProjectInfoStatsTools {
    [Serializable]
    public class FileStat {
        public string Path;
        public int LineCount;
        public float Weight;
        public CodeQuality Quality = new CodeQuality();
    }
}
