namespace FbNet.Model
{
    /// <summary>
    /// Группа
    /// <see cref="https://developers.facebook.com/docs/graph-api/reference/v4.0/group"/>
    /// </summary>
    public class Group
    {
        public string Id { get; set; }
        public CoverPhoto Cover { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string Icon { get; set; }
        public int MemberCount { get; set; }
        public int MemberRequestCount { get; set; }
        public string Permissions { get; set; }
        public string Privacy { get; set; }
        public string Name { get; set; }
        public bool Administrator { get; set; }
    }
}