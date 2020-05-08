using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FbNet.Model;

namespace FbNet.Categories
{
    public class AlbumsCategory
    {
        private readonly FbApi _fb;

        internal AlbumsCategory(FbApi fb)
        {
            _fb = fb;
        }

        public ReadOnlyCollection<Album> GetGroupAlbums(string groupId, string fields = "cover_photo{icon,source,images},name,can_upload", int? limit = null)
        {
            var res = new List<Album>();
            var nextUrl = $"{groupId}/albums";

            while (!string.IsNullOrEmpty(nextUrl))
            {
                object parameters = limit != null ? (object)(new {fields, limit}) : new {fields};
                dynamic data = _fb.Get(nextUrl, parameters);
                if (data == null || data.data == null) return null;
                if (!(data.data is List<dynamic> list)) return null;

                foreach (var item in list)
                {
                    var album = new Album
                    {
                        Id = item.id,
                        Name = item.name,
                        Cover = CoverPhoto.Create(item.cover_photo),
                        CanUpload = item.can_upload
                    };
                    res.Add(album);
                }

                nextUrl = data.paging?.next;
            }

            return res.Any() ? new ReadOnlyCollection<Album>(res) : null;
        }

        public Album Create(string title, string groupId)
        {
            dynamic data = _fb.Post($"{groupId}/albums", new {name = title});
            if (data == null) return null;

            return new Album {CanUpload = true, Id = data.id, Name = title};
        }

        public Album Get(string id, string fields = "can_upload,cover_photo,link,name,privacy,type,updated_time")
        {
            dynamic data = _fb.Get($"{id}", new {fields});
            if (data == null) return null;

            var info = new Album
            {
                Id = data.id,
                Cover = data.cover_photo == null ? null : new CoverPhoto{ Id = data.cover_photo.id, Source = data.cover_photo.source, Icon = data.cover_photo.icon},
                Name = data.name,
                CanUpload = data.can_upload,
                Link = data.link,
                Type = data.type,
                //Permissions = item.permissions,
                UpdatedTime = Utils.ParseDate(data.updated_time)
            };

            return info;
        }
    }
}