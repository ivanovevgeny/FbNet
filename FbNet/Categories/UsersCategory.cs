using FbNet.Model;

namespace FbNet.Categories
{
    public class UsersCategory
    {
        private readonly FbApi _fb;

        internal UsersCategory(FbApi fb)
        {
            _fb = fb;
        }

        public User GetCurrentUser(string fields = "id,name,email,first_name,last_name,birthday,gender,locale,address")
        {
            dynamic data = _fb.Client.Get("me", new {fields});
            if (data == null) return null;

            var address = data.address;
            var user = new User
            {
                Id = data.id,
                Email = data.email,
                Name = data.name,
                FirstName = data.first_name,
                LastName = data.last_name
                // TODO
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