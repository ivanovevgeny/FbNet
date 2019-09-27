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
            dynamic data = _fb.Client.Get("me/accounts", new {fields});

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
    }
}