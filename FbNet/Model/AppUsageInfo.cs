using System;

namespace FbNet.Model
{
    public class AppUsageInfo
    {
        public string BusinessObjectID { get; set; }
        public FbRateLimitType RateLimitType { get; set; }
        public int CallCount { get; set; }
        public int TotalTime { get; set; }
        public int TotalCpuTime { get; set; }
        public int EstimatedTimeToRegainAccess { get; set; }
        public int GetMax() => Math.Max(Math.Max(CallCount, TotalTime), TotalCpuTime);
    }

    public enum FbRateLimitType { App = 0, Pages = 1, Instagram = 2 }
}