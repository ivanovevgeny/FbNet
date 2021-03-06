﻿namespace FbNet.Model
{
    public class User
    {
        public string Id { get; set; }
        public Location Address { get; set; }
        /// <summary>
        /// MM/DD/YYYY
        /// </summary>
        public string Birthday { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public UserPicture Picture { get; set; }
    }

    public class UserPicture
    {
        public long Height { get; set; }
        public long Width { get; set; }
        public string Url { get; set; }
    }
}