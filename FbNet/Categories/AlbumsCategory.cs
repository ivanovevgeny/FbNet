using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ReadOnlyCollection<Album> GetGroupAlbums(string groupId, string fields = "cover_photo{icon,source},name,can_upload")
        {
            dynamic data = _fb.Get($"{groupId}/albums", new {fields});

            if (data == null || data.data == null) return null;

            var res = new List<Album>();
            if (!(data.data is List<dynamic> list)) return null;

            foreach (var item in list)
            {
                var album = new Album
                {
                    Id = item.id,
                    Name = item.name,
                    Cover = item.cover == null ? null : new CoverPhoto{ Id = item.cover.id, Source = item.cover_photo.source, Icon = item.cover_photo.icon},
                    CanUpload = item.can_upload
                };
                res.Add(album);
            }

            return new ReadOnlyCollection<Album>(res);
        }

        public ReadOnlyCollection<Album> GetPageAlbums(string pageId, string fields = "cover_photo{icon,source},name,can_upload")
        {
            dynamic data = _fb.Get($"{pageId}/albums", new {fields});

            if (data == null || data.data == null) return null;

            var res = new List<Album>();
            if (!(data.data is List<dynamic> list)) return null;

            foreach (var item in list)
            {
                var album = new Album
                {
                    Id = item.id,
                    Name = item.name,
                    Cover = item.cover == null ? null : new CoverPhoto{ Id = item.cover.id, Source = item.cover_photo.source, Icon = item.cover_photo.icon},
                    CanUpload = item.can_upload
                };
                res.Add(album);
            }

            return new ReadOnlyCollection<Album>(res);
        }

        public Album Create(string title, string groupId)
        {
            dynamic data = _fb.Post($"{groupId}/albums", new {name=title});
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