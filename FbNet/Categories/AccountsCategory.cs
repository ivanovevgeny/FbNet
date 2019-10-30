using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            var res = new List<Page>();
            var nextUrl = "me/accounts";

            while (!string.IsNullOrEmpty(nextUrl))
            {
                dynamic data = _fb.Get(nextUrl, new {fields});
                if (data == null || data.data == null) return null;    
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

                nextUrl = data.paging?.next;
            }

            return res.Any() ? new ReadOnlyCollection<Page>(res) : null;
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