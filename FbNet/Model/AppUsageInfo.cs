using System;

namespace FbNet.Model
{
    public class AppUsageInfo
    {
        public int CallCount { get; set; }
        public int TotalTime { get; set; }
        public int TotalCpuTime { get; set; }
        public int EstimatedTimeToRegainAccess { get; set; }
        public int GetMax() => Math.Max(Math.Max(CallCount, TotalTime), TotalCpuTime);
    }
}