using System.Collections.Generic;
using System.Linq;

namespace FbNet.Model
{
    public class CoverPhoto
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public string Icon { get; set; }
        public List<Image> Images { get; set; }

        public static CoverPhoto Create(dynamic cover)
        {
            if (cover == null) return null;
            var res = new CoverPhoto {Id = cover.id, Source = cover.source, Icon = cover.icon};
            if (cover.images == null) return res;

            if (!(cover.images is List<dynamic> list)) return res;
            if (!list.Any()) return res;

            res.Images = new List<Image>();

            foreach (var img in list)
                res.Images.Add(new Image {Height = img.height, Width = img.width, Source = img.source});

            return res;
        }
    }
}