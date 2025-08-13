using UnityEngine;
using System;

namespace ProjectInfoStatsTools {
    [Serializable]
    public class CodeQuality {
        public float CyclomaticComplexity;
        public int MaxNestingDepth;
        public float MaintainabilityIndex;
    }
}
