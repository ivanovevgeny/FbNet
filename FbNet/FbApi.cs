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
        public readonly ApiTokenType ApiTokenType;
        
        public event Action<AppUsageInfo> OnGetAppUsage;

        /// <summary>
        /// Токен для доступа к методам API
        /// </summary>
        private string _accessToken;
        public string AccessToken
        {
            get => _accessToken;
            set
            {
                _accessToken = value;
                if (ApiTokenType == ApiTokenType.User)
                    _client.AccessToken = _accessToken;
            }
        }

        private long? _pageId;
        public long? PageId
        {
            get => _pageId;
            set
            {
                _pageId = value;
                if (ApiTokenType == ApiTokenType.Page && _pageId != null)
                    RefreshPageAccessToken();
            }
        }

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

        private FbApi(ApiTokenType apiTokenType)
        {
            ApiTokenType = apiTokenType;
            Users = new UsersCategory(this);
            Pages = new AccountsCategory(this);
            Groups = new GroupsCategory(this);
            Albums = new AlbumsCategory(this);
            Photos = new PhotosCategory(this);
        }

        public FbApi(ApiTokenType apiTokenType, string appId, string appSecretKey, string accessToken, long? pageId = null) : this(apiTokenType)
        {
            AppUsageInfo = new AppUsageInfo {RateLimitType = FbRateLimitType.App, CallCount = 0, TotalTime = 0, TotalCpuTime = 0};

            _client = new FacebookClient
            {
                AppId = appId,
                AppSecret = appSecretKey,
                IsSecureConnection = true,
                Version = "v12.0",
                UseFacebookBeta = false,
                AlwaysReturnHeaders = true
            };

            AccessToken = accessToken;
            PageId = pageId;
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
                    throw new FacebookApiException((string)data.error.message, "ErrorInResponse", (int) data.error.code); //FbApiMethodInvokeException(data.error.message, (int) data.error.code);
                return data;
            }
            catch (FacebookApiException e)
            {
                SetAppUsage(e.Headers);
                throw; // new FbApiMethodInvokeException(e);
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

                if (ApiTokenType == ApiTokenType.User)
                {
                    if (!headers.TryGetValue("x-app-usage", out var appUsage)) return;
                    if (string.IsNullOrEmpty(appUsage)) return;

                    var json = JObject.Parse(appUsage);
                    AppUsageInfo.RateLimitType = FbRateLimitType.App;
                    AppUsageInfo.CallCount = int.Parse(json["call_count"].ToString());
                    AppUsageInfo.TotalTime = int.Parse(json["total_time"].ToString());
                    AppUsageInfo.TotalCpuTime = int.Parse(json["total_cputime"].ToString());

                    OnGetAppUsage?.Invoke(AppUsageInfo);

                    return;
                }

                if (ApiTokenType == ApiTokenType.Page)
                {
                    if (!headers.TryGetValue("x-business-use-case-usage", out var pageAppUsage)) return;
                    if (string.IsNullOrEmpty(pageAppUsage) || !PageId.HasValue) return;

                    var headerData = JObject.Parse(pageAppUsage);
                    var json = headerData[PageId.Value.ToString()];
                    foreach (dynamic item in json)
                    {
                        if (item["type"].Value != "pages") continue;

                        AppUsageInfo.BusinessObjectID = PageId.Value.ToString();
                        AppUsageInfo.RateLimitType = FbRateLimitType.Pages;
                        AppUsageInfo.CallCount = (int)(item["call_count"].Value);
                        AppUsageInfo.TotalTime = (int)item["total_time"].Value;
                        AppUsageInfo.TotalCpuTime = (int)item["total_cputime"].Value;
                        AppUsageInfo.EstimatedTimeToRegainAccess = (int)item["estimated_time_to_regain_access"].Value;

                        OnGetAppUsage?.Invoke(AppUsageInfo);

                        break;
                    }
                }
            }
            catch (System.Exception e)
            {
                // ignore
            }
        }

        public void RefreshPageAccessToken()
        {
            _client.AccessToken = AccessToken;
            try
            {
                dynamic data = Get($"{PageId}", new {fields = "access_token"});
                if (data == null) return;
                PageAccessToken = data.access_token;
                _client.AccessToken = PageAccessToken;
            }
            catch (System.Exception e)
            {
                _client.AccessToken = PageAccessToken;
                throw;
            }
        }
    }
}