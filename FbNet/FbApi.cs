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
            get => _client.AccessToken;
            set => _client.AccessToken = value;
        }

        public long? PageId { get; set; }
        public string PageAccessToken { get; private set; }

        /// <summary>
        /// Секретный ключ сессии
        /// </summary>
        public string SessionSecretKey { get; set; }

        private readonly FacebookClient _client;

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

            _client = new FacebookClient
            {
                AppId = appId,
                AppSecret = appSecretKey,
                IsSecureConnection = true,
                Version = "v4.0",
                UseFacebookBeta = false,
                AlwaysReturnHeaders = true
            };
        }

        private object Invoke(Func<string, object, dynamic> func, string path, object parameters = null)
        {
            try
            {
                dynamic result = func.Invoke(path, parameters);
                if (result == null) return null;
                SetAppUsage(result.headers);

                var data = result.body;
                if (data?.error != null)
                    throw new FbApiMethodInvokeException(data.error.message, (int) data.error.code);
                return data;
            }
            catch (FacebookApiException e)
            {
                SetAppUsage(e.Headers);
                throw new FbApiMethodInvokeException(e.ErrorType, e.ErrorCode);
            }
        }

        internal object Get(string path, object parameters)
        {
            return Invoke(_client.Get, path, parameters);
        }

        internal object Post(string path, object parameters)
        {
            return Invoke(_client.Post, path, parameters);
        }

        internal object Delete(string path, object parameters = null)
        {
            return Invoke(_client.Delete, path, parameters);
        }

        public void SetAppUsage(dynamic headersStr)
        {
            if (headersStr == null) return;
            try
            {
                var headers = (Dictionary<string, string>) headersStr;
                if (headers.TryGetValue("x-app-usage", out var appUsage))
                {
                    if (string.IsNullOrEmpty(appUsage)) return;

                    var json = JObject.Parse(appUsage);
                    AppUsageInfo.CallCount = int.Parse(json["call_count"].ToString());
                    AppUsageInfo.TotalTime = int.Parse(json["total_time"].ToString());
                    AppUsageInfo.TotalCpuTime = int.Parse(json["total_cputime"].ToString());
                }
                else if (headers.TryGetValue("x-business-use-case-usage", out var pageAppUsage))
                {
                    if (string.IsNullOrEmpty(pageAppUsage) || !PageId.HasValue) return;

                    var headerData = JObject.Parse(pageAppUsage);
                    var json = headerData[PageId.Value.ToString()];
                    foreach (dynamic item in json)
                    {
                        if (item["type"].Value != "pages") continue;
                        
                        AppUsageInfo.CallCount = (int)(item["call_count"].Value);
                        AppUsageInfo.TotalTime = (int)item["total_time"].Value;
                        AppUsageInfo.TotalCpuTime = (int)item["total_cputime"].Value;
                        AppUsageInfo.EstimatedTimeToRegainAccess = (int)item["estimated_time_to_regain_access"].Value;

                        break;
                    }
                }
            }
            catch (System.Exception e)
            {
                // ignore
            }
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