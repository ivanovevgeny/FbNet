﻿using FbNet.Model;

namespace FbNet.Categories
{
    public class UsersCategory
    {
        private readonly FbApi _fb;

        internal UsersCategory(FbApi fb)
        {
            _fb = fb;
        }

        public User GetCurrentUser(string fields = "id,name,first_name,last_name,picture")
        {
            dynamic data = _fb.Get("me", new {fields});
            if (data == null) return null;

            //var address = data.address;
            var user = new User
            {
                Id = data.id,
                Name = data.name,
                FirstName = data.first_name,
                LastName = data.last_name,
                Picture = data.picture?.data == null ? null : new UserPicture { Url = data.picture.data.url, Height = data.picture.data.height, Width = data.picture.data.width }
            };

            /*if (address != null)
            {
                user.Address = new Location
                {
                    Id = address.id,
                    City = address.city
                }
            }*/

            return user;
        }
    }
}