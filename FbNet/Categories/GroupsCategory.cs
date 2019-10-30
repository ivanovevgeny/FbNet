using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FbNet.Model;

namespace FbNet.Categories
{
    public class GroupsCategory
    {
        private readonly FbApi _fb;

        internal GroupsCategory(FbApi fb)
        {
            _fb = fb;
        }

        public ReadOnlyCollection<Group> GetCurrentUserGroups(string fields = "administrator,cover,icon,name,permissions,privacy")
        {
            var res = new List<Group>();
            var nextUrl = "me/groups";

            while (!string.IsNullOrEmpty(nextUrl))
            {
                dynamic data = _fb.Get(nextUrl, new {fields});
                if (data == null || data.data == null) return null;
                if (!(data.data is List<dynamic> list)) return null;

                foreach (var item in list)
                {
                    var group = new Group
                    {
                        Id = item.id,
                        Cover = item.cover == null ? null : new CoverPhoto{ Id = item.cover.id, Source = item.cover.source, Icon = item.cover.icon},
                        Icon = item.icon,
                        Name = item.name,
                        //Permissions = item.permissions,
                        Privacy = item.privacy,
                        Administrator = item.administrator
                    };
                    res.Add(group);
                }

                nextUrl = data.paging?.next;
            }

            return res.Any() ? new ReadOnlyCollection<Group>(res) : null;
        }
    }
}