using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Facebook;
using FbNet.Categories;
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

        /// <summary>
        /// Кол-во пробуемых при 1 запросе прокси
        /// </summary>
        public static int MAX_PROXY_TRY_COUNT = 3;

        /// <summary>
        /// Интервал повтора проверки невалидных прокси, в минутах
        /// </summary>
        public static int INVALID_PROXY_RETRY_MINUTES = 10;

        /// <summary>
        /// Получение списка прокси, определяется пользователем библиотеки, если задано запросы выполняются с указанными прокси
        /// </summary>
        public static Func<ICollection<WebProxy>> GetProxyListHandler;

        private static readonly ConcurrentDictionary<string, DateTime> _invalidProxies = new ConcurrentDictionary<string, DateTime>(); //статик, т.к. списки недействующих прокси необходимо поддерживать от запроса к запросу

        private WebProxy _proxy = null;

        private bool _useProxy = false;

        private ICollection<WebProxy> GetProxyList()
        {
            if (GetProxyListHandler == null) return null;

            return GetProxyListHandler();
        }

        private WebProxy GetNextProxy()
        {
            var proxies = GetProxyList();

            if (proxies == null || proxies.Count == 0) return null;

            //удаляем возможно устаревшие невалидные прокси
            if (_invalidProxies.Any())
            {
                var keys = _invalidProxies.Keys.ToList();
                var validBefore = DateTime.UtcNow.AddMinutes(-INVALID_PROXY_RETRY_MINUTES); //добавленные до этой даты прокси считаются валидными, пробуем повторно

                foreach (var k in keys)
                {
                    if (_invalidProxies.TryGetValue(k, out DateTime keyDate))
                    {
                        if (keyDate <= validBefore)
                            _invalidProxies.TryRemove(k, out DateTime v);
                    }
                }
            }
            //.удаляем возможно устаревшие невалидные прокси

            var validProxies = proxies.Where(x => !_invalidProxies.ContainsKey(x.Address.ToString())).ToList();

            if (!validProxies.Any()) return null;

            var rnd = new Random(DateTime.UtcNow.GetHashCode());
            var proxyIndex = rnd.Next(0, validProxies.Count() - 1);

            return validProxies[proxyIndex];
        }

        private bool ChangeProxy()
        {
            if (_proxy != null)
            {
                _invalidProxies.TryAdd(_proxy.Address.ToString(), DateTime.UtcNow);
            }

            return SetProxy();
        }

        private bool SetProxy()
        {
            _proxy = GetNextProxy();

            if (_proxy != null)
            {
                _client.SetHttpWebRequestFactory(uri =>
                {
                    var request = new HttpWebRequestWrapper((HttpWebRequest)WebRequest.Create(uri))
                    {
                        Proxy = _proxy
                    };
                    return request;
                });
            }

            return _proxy != null;
        }

        public static List<string> InvalidProxies { get { return _invalidProxies?.Keys.ToList(); } }

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
            AppUsageInfo = new AppUsageInfo { RateLimitType = FbRateLimitType.App, CallCount = 0, TotalTime = 0, TotalCpuTime = 0 };

            _client = new FacebookClient
            {
                AppId = appId,
                AppSecret = appSecretKey,
                IsSecureConnection = true,
                Version = "v12.0",
                UseFacebookBeta = false,
                AlwaysReturnHeaders = true
            };

            _useProxy = SetProxy();

            AccessToken = accessToken;
            PageId = pageId;
        }

        private object Invoke(Func<string, object, dynamic> func, string path, object parameters = null)
        {
            int tryCount = _useProxy ? MAX_PROXY_TRY_COUNT : 1;
            while (tryCount > 0)
            {
                tryCount--;

                try
                {
                    dynamic result = func.Invoke(path, parameters);
                    if (result == null) return null;
                    SetAppUsage(result.headers);

                    var data = result.body;
                    if (data?.error != null)
                        throw new FacebookApiException((string)data.error.message, "ErrorInResponse", (int)data.error.code); //FbApiMethodInvokeException(data.error.message, (int) data.error.code);
                    return data;
                }
                catch (FacebookApiException e)
                {
                    SetAppUsage(e.Headers);
                    throw; // new FbApiMethodInvokeException(e);
                }
                catch (System.Exception e)
                {
                    if (tryCount == 0 || _proxy == null) throw; //попытки выполнения запроса исчерпаны (что для прокси что без)

                    if (!IsProxyAlive(_proxy))
                    {
                        if (!ChangeProxy()) //не удалось сменить прокси, далее не выполняем
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw; //ошибка не связана с прокси
                    }

                    /* 27.04.2022 проверка ранее, не точная, вызывает блокировку прокси, вероятно статусный код возвращается и в случае валидного прокси при неправильных параметрах запроса
                    if (((WebExceptionWrapper)e).Status == WebExceptionStatus.ConnectFailure || ((WebExceptionWrapper)e).Status == WebExceptionStatus.ProtocolError) // в случае ошибки, связанной с прокси меняем прокси и повторяем запрос, в случае нерабочего прокси исключение вылетит при tryCount == 0
                    {
                        if (!ChangeProxy())
                        {
                            throw; //не удалось установить прокси, смысла выполнять дальше нет
                        }
                    }
                    else
                    {
                        throw; //ошибка не связанная с прокси
                    }
                    */
                }
            }

            return null;
        }

        private bool IsProxyAlive(WebProxy proxy)
        {
            const string checkUrl = "https://graph.facebook.com/facebook/picture?redirect=false";

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(checkUrl);
                request.Method = "HEAD";
                request.Proxy = proxy;
                request.Timeout = 10000;
                request.ReadWriteTimeout = 10000;
                var response = (HttpWebResponse)request.GetResponse();
                var statusCode = (int)response.StatusCode;
                return statusCode == 200;
            }
            catch { }

            return false;
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
                var headers = (Dictionary<string, string>)headersStr;

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
                dynamic data = Get($"{PageId}", new { fields = "access_token" });
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