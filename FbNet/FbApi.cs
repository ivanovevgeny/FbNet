using System;
using Facebook;
using FbNet.Categories;
using FbNet.Exception;

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
            Client = new FacebookClient
            {
                AppId = appId,
                AppSecret = appSecretKey,
                IsSecureConnection = true,
                Version = "v4.0",
                UseFacebookBeta = false
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

        private static object HandleApiResult(dynamic result)
        {
            if (result != null && result.error != null) 
                throw new FbApiMethodInvokeException(result.error.message, (int)result.error.code);
            return result;
        }

        public string RefreshPageAccessToken(long pageId)
        {
            PageId = pageId;
            dynamic data = Client.Get($"{PageId}", new {fields = "access_token"});
            if (data == null) return null;
            PageAccessToken = data.access_token;

            return PageAccessToken;
        }
    }  
}