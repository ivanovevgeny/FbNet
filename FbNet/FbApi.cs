using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FbNet.Model;

namespace FbNet
{
    public interface IFbApi
    {
        Task<User> GetCurrentUserAsync();
        Task<ReadOnlyCollection<Group>> GetUserGroupsAsync(string userId = "");
        Task PostOnWallAsync(string accessToken, string message);
    }

    public class FbApi : IFbApi
    {
        /// <summary>
        /// Токен для доступа к методам API
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Секретный ключ сессии
        /// </summary>
        public string SessionSecretKey { get; set; }

        private readonly string _applicationKey;

        private readonly string _applicationSecretKey;

        private readonly IFbClient _client;
 
        public FbApi()
        {
            
        }

        public FbApi(string applicationKey, string applicationSecretKey, string userAgent = "")
        {
            _applicationKey = applicationKey;
            _applicationSecretKey = applicationSecretKey;
            _client = new FbClient();
        }
 
        public async Task<User> GetCurrentUserAsync()
        {
            var resp = await _client.GetAsync<dynamic>(
                AccessToken, "me", "fields=id,name,email,first_name,last_name,birthday,gender,locale");

            if (resp == null)
                return null;

            var user = new User
            {
                Id = resp.id,
                Email = resp.email,
                Name = resp.name,
                FirstName = resp.first_name,
                LastName = resp.last_name,
            };
 
            return user;
        }

        public async Task<ReadOnlyCollection<Group>> GetUserGroupsAsync(string userId = "")
        {
            var id = string.IsNullOrEmpty(userId) ? "me" : userId;
            var resp = await _client.GetAsync<dynamic>(
                AccessToken, $"{id}/groups", "fields=id,cover,icon,name,permissions,privacy");

            if (resp == null)
                return null;

            var res = new List<Group>();
            if (!(resp is List<dynamic> list)) return null;

            foreach (var item in list)
            {
                var group = new Group
                {
                    Id = item.id,
                    Cover = item.cover == null ? null : new CoverPhoto{ Id = item.cover.id, Source = item.cover.source },
                    Icon = item.icon,
                    Name = item.name,
                    Permissions = item.permissions,
                    Privacy = item.privacy
                };
                res.Add(group);
            }

            return new ReadOnlyCollection<Group>(res);
        }
 
        public async Task PostOnWallAsync(string accessToken, string message) => await _client.PostAsync(accessToken, "me/feed", new {message});
    }  
}