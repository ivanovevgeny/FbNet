using System;
using System.Collections.Generic;
using Facebook;
using FbNet.Categories;
using FbNet.Exception;
using FbNet.Model;
using Newtonsoft.Json.Linq;

namespace FbNet
{
    public class FbApi
    {
        /// <summary>
        /// Токен для доступа к методам API
        /// </summary>
        public string AccessToken
        {
            get => Client.AccessToken;
            set => Client.AccessToken = value;
        }

        public long? PageId { get; set; }
        public string PageAccessToken { get; private set; }

        /// <summary>
        /// Секретный ключ сессии
        /// </summary>
        public string SessionSecretKey { get; set; }

        //private readonly IFbClient _client;

        private readonly FacebookClient Client;

        #region Категории

        ///// <summary>
        ///// API для работы с пользователями.
        ///// </summary>
        public UsersCategory Users { get; }

        ///// <summary>
        ///// API для работы с группами.
        ///// </summary>
        public AccountsCategory Pages { get; }

        ///// <summary>
        ///// API для работы с группами.
        ///// </summary>
        public GroupsCategory Groups { get; }

        /// <summary>
        /// API для работы с альбомами
        /// </summary>
        public AlbumsCategory Albums { get; }

        /// <summary>
        /// API для работы с фотографиями
        /// </summary>
        public PhotosCategory Photos { get; }
        
        public AppUsageInfo AppUsageInfo { get; }

        #endregion

        public FbApi()
        {
            Users = new UsersCategory(this);
            Pages = new AccountsCategory(this);
            Groups = new GroupsCategory(this);
            Albums = new AlbumsCategory(this);
            Photos = new PhotosCategory(this);
        }

        public FbApi(string appId, string appSecretKey, string userAgent = "") : this()
        {
            AppUsageInfo = new AppUsageInfo {CallCount = 0, TotalTime = 0, TotalCpuTime = 0};

            Client = new FacebookClient
            {
                AppId = appId,
                AppSecret = appSecretKey,
                IsSecureConnection = true,
                Version = "v4.0",
                UseFacebookBeta = false,
                AlwaysReturnHeaders = true
            };
        }

        internal object Get(string path, object parameters)
        {
            return HandleApiResult(Client.Get(path, parameters));
        }

        internal object Post(string path, object parameters)
        {
            return HandleApiResult(Client.Post(path, parameters));
        }

        internal object Delete(string path)
        {
            return HandleApiResult(Client.Delete(path));
        }

        private object HandleApiResult(dynamic result)
        {
            if (result == null) return null;
            if (result.headers != null)
            {
                var headers = (Dictionary<string, string>) result.headers;
                if (headers.TryGetValue("x-app-usage", out var appUsage))
                    if (!string.IsNullOrEmpty(appUsage))
                    {
                        try
                        {
                            var json = JObject.Parse(appUsage);
                            AppUsageInfo.CallCount = int.Parse(json["call_count"].ToString());
                            AppUsageInfo.TotalTime = int.Parse(json["total_time"].ToString());
                            AppUsageInfo.TotalCpuTime = int.Parse(json["total_cputime"].ToString());
                        }
                        catch (System.Exception e)
                        {
                            // ignore
                        }
                    }
            }

            var data = result.body;
            if (data?.error != null)
                throw new FbApiMethodInvokeException(data.error.message, (int) data.error.code);
            return data;
        }

        public string RefreshPageAccessToken(long pageId)
        {
            PageId = pageId;
            dynamic data = Get($"{PageId}", new {fields = "access_token"});
            if (data == null) return null;
            PageAccessToken = data.access_token;

            return PageAccessToken;
        }
    }
}