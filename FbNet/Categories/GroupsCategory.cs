using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            dynamic data = _fb.Get("me/groups", new {fields});

            if (data == null || data.data == null) return null;

            var res = new List<Group>();
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

            return new ReadOnlyCollection<Group>(res);
        }
    }
}