namespace FbNet.Model
{
    /// <summary>
    /// Группа
    /// <see cref="https://developers.facebook.com/docs/graph-api/reference/user/accounts/"/>
    /// <seealso cref="https://developers.facebook.com/docs/graph-api/reference/page/"/>
    /// </summary>
    public class Page
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool CanPost { get; set; }
        public CoverPhoto Cover { get; set; }
        public string Link { get; set; }
    }
}