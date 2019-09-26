using System;

namespace FbNet.Model
{
    public class Photo
    {
        public string Id { get; set; }
        public bool CanDelete { get; set; }
        public long Height { get; set; }
        public long Width { get; set; }
        public string Link  { get; set; }
        public string Name { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}