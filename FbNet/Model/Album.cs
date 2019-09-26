using System;

namespace FbNet.Model
{
    public class Album
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public CoverPhoto Cover { get; set; }
        public bool CanUpload { get; set; }
        public string Link { get; set; }
        public string Type { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}