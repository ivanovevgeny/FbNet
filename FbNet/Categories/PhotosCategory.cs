using System.Collections.Generic;
using FbNet.Model;

namespace FbNet.Categories
{
    public class PhotosCategory
    {
        private readonly FbApi _fb;
        private const string DefaultFields = "name,width,height,can_delete,link,updated_time";

        internal PhotosCategory(FbApi fb)
        {
            _fb = fb;
        }

        /*public Album CreateAlbum(string title, string groupId)
        {
            dynamic data = _fb.Client.Post($"{groupId}/albums", new {name=title});
            if (data == null) return null;

            return new Album {CanUpload = true, Id = data.id, Name = title};
        }*/

        public Photo Get(string id, string fields = DefaultFields)
        {
            dynamic data = _fb.Client.Get("{id}", new {fields});
            if (data == null) return null;

            var info = new Photo
            {
                Id = data.id,
                Name = data.name,
                Width = data.width,
                Height = data.height,
                CanDelete = data.can_delete,
                Link = data.link,
                UpdatedTime = data.updated_time
            };

            return info;
        }

        public List<Photo> Get(List<string> ids, string fields = DefaultFields)
        {
            dynamic data = _fb.Client.Get("{id}", new {fields});
            if (data == null) return null;

            var res = new List<Photo>();
            foreach (var id in ids)
            {
                var item = data[id];
                if (item == null) continue;

                var info = new Photo
                {
                    Id = item.id,
                    Name = item.name,
                    Width = item.width,
                    Height = item.height,
                    CanDelete = item.can_delete,
                    Link = item.link,
                    UpdatedTime = item.updated_time
                };
                res.Add(info);
            }

            return res;
        }

        public Photo Create(string albumId, string caption, string url, bool published = true, string fields = DefaultFields)
        {
            dynamic data = _fb.Client.Post($"{albumId}/photos", new {caption, url, published, fields});
            if (data == null) return null;

            var info = new Photo
            {
                Id = data.id,
                Name = data.name,
                Width = data.width,
                Height = data.height,
                CanDelete = data.can_delete,
                Link = data.link,
                UpdatedTime = data.updated_time
            };

            return info;
        }

        public bool Edit(string id, string name)
        {
            dynamic data = _fb.Client.Post($"{id}", new {name});
            return data != null;
        }
        
        public bool Delete(string id)
        {
            dynamic data = _fb.Client.Delete($"{id}");
            return data != null && data.success;
        }
    }
}