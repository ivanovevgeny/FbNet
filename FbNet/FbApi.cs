using Facebook;
using FbNet.Categories;

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

        /// <summary>
        /// Секретный ключ сессии
        /// </summary>
        public string SessionSecretKey { get; set; }

        //private readonly IFbClient _client;
        
        internal readonly FacebookClient Client;

        #region Категории
        ///// <summary>
        ///// API для работы с пользователями.
        ///// </summary>
        public UsersCategory Users { get; }

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
    }  
}