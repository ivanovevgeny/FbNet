using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            var oldAccessToken = _fb.AccessToken;
            if (_fb.PageId != null)
                _fb.AccessToken = _fb.PageAccessToken;

            try
            {
                dynamic data = _fb.Get($"{id}", new {fields});
                if (data == null) return null;

                var info = new Photo
                {
                    Id = data.id,
                    Name = data.name,
                    Width = data.width,
                    Height = data.height,
                    CanDelete = data.can_delete,
                    Link = data.link,
                    UpdatedTime = Utils.ParseDate(data.updated_time)
                };

                return info;
            }
            finally
            {
                if (_fb.PageId != null)
                    _fb.AccessToken = oldAccessToken;
            }
        }

        public ReadOnlyCollection<Photo> Get(List<string> ids, string fields = DefaultFields)
        {
            var res = new List<Photo>();

            var oldAccessToken = _fb.AccessToken;
            if (_fb.PageId != null)
                _fb.AccessToken = _fb.PageAccessToken;

            try
            {
                dynamic data = _fb.Get($"?ids={string.Join(",",ids)}", new {fields});
                if (data == null) return null;

                foreach (var id in ids)
                {
                    try
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
                            UpdatedTime = Utils.ParseDate(item.updated_time)
                        };
                        res.Add(info);
                    }
                    catch (System.Exception e)
                    {
                        // ignored
                    }
                }

                return res.Any() ? new ReadOnlyCollection<Photo>(res) : null;
            }
            finally
            {
                if (_fb.PageId != null)
                    _fb.AccessToken = oldAccessToken;
            }
        }

        public ReadOnlyCollection<Photo> GetAlbumPhotos(string albumId, string fields = DefaultFields)
        {
            var res = new List<Photo>();

            var oldAccessToken = _fb.AccessToken;
            if (_fb.PageId != null)
                _fb.AccessToken = _fb.PageAccessToken;

            try
            {
                var nextUrl = $"{albumId}/photos";

                while (!string.IsNullOrEmpty(nextUrl))
                {
                    dynamic data = _fb.Get(nextUrl, new {fields});
                    if (data == null || data.data == null) break;

                    var items = data.data;

                    foreach (var item in items)
                    {
                        var info = new Photo
                        {
                            Id = item.id,
                            Name = item.name,
                            Width = item.width,
                            Height = item.height,
                            CanDelete = item.can_delete,
                            Link = item.link,
                            UpdatedTime = Utils.ParseDate(item.updated_time)
                        };
                        res.Add(info);
                    }

                    nextUrl = data.paging?.next;
                }

                return res.Any() ? new ReadOnlyCollection<Photo>(res) : null;
            }
            finally
            {
                if (_fb.PageId != null)
                    _fb.AccessToken = oldAccessToken;
            }
        }

        public Photo Create(string albumId, string caption, string url, bool published = true, string fields = DefaultFields)
        {
            var oldAccessToken = _fb.AccessToken;
            if (_fb.PageId != null)
                _fb.AccessToken = _fb.PageAccessToken;

            try
            {
                dynamic data = _fb.Post($"{albumId}/photos", new {caption, url, published, fields});
                if (data == null) return null;

                var info = new Photo
                {
                    Id = data.id,
                    Name = data.name,
                    Width = data.width,
                    Height = data.height,
                    CanDelete = data.can_delete,
                    Link = data.link,
                    UpdatedTime = Utils.ParseDate(data.updated_time)
                };

                return info;
            }
            finally
            {
                if (_fb.PageId != null)
                    _fb.AccessToken = oldAccessToken;
            }
        }

        // нельзя редактировать, только удалять и заново вставлять
        /*public bool Edit(string id, string name)
        {
            var oldAccessToken = _fb.AccessToken;
            if (_fb.PageId != null)
                _fb.AccessToken = _fb.PageAccessToken;

            try
            {
                dynamic data = _fb.Post($"{id}", new {name});
                return data != null;
            }
            finally
            {
                if (_fb.PageId != null)
                    _fb.AccessToken = oldAccessToken;
            }
        }*/
        
        public bool Delete(string id)
        {
            var oldAccessToken = _fb.AccessToken;
            if (_fb.PageId != null)
                _fb.AccessToken = _fb.PageAccessToken;

            try
            {
                dynamic data = _fb.Delete($"{id}");
                return data != null && data.success;
            }
            finally
            {
                if (_fb.PageId != null)
                    _fb.AccessToken = oldAccessToken;
            }
        }
    }
}