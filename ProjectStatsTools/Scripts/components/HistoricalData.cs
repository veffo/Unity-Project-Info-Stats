using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProjectInfoStatsTools {
    [Serializable]
    public class HistoricalData {
        public List<ScriptStats> History = new List<ScriptStats>();
    }
}
