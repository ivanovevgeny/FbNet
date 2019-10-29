using System.Collections.Generic;
using System.Collections.ObjectModel;
using FbNet.Model;

namespace FbNet.Categories
{
    public class AccountsCategory
    {
        private readonly FbApi _fb;

        internal AccountsCategory(FbApi fb)
        {
            _fb = fb;
        }

        public ReadOnlyCollection<Page> GetCurrentUserAccounts(string fields = "name,link,cover,can_post")
        {
            dynamic data = _fb.Get("me/accounts", new {fields});

            if (data == null || data.data == null) return null;

            var res = new List<Page>();
            if (!(data.data is List<dynamic> list)) return null;

            foreach (var item in list)
            {
                var page = new Page
                {
                    Id = item.id,
                    Cover = item.cover == null ? null : new CoverPhoto{ Id = item.cover.id, Source = item.cover.source, Icon = item.cover.icon},
                    Name = item.name,
                    CanPost = item.can_post,
                    Link = item.link,
                };
                res.Add(page);
            }

            return new ReadOnlyCollection<Page>(res);
        }
        
        public Page Get(long id, string fields = "name,link,cover,can_post")
        {
            var oldAccessToken = _fb.AccessToken;
            if (_fb.PageId != null)
                _fb.AccessToken = _fb.PageAccessToken;

            try
            {
                dynamic data = _fb.Get($"{id}", new {fields});
                if (data == null) return null;

                var info = new Page
                {
                    Id = data.id,
                    Cover = data.cover == null ? null : new CoverPhoto{ Id = data.cover.id, Source = data.cover.source, Icon = data.cover.icon},
                    Name = data.name,
                    CanPost = data.can_post,
                    Link = data.link,
                };

                return info;
            }
            finally
            {
                if (_fb.PageId != null)
                    _fb.AccessToken = oldAccessToken;
            }
        }
    }
}