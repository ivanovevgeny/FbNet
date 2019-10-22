using System;

namespace FbNet
{
    public class Utils
    {
        public static DateTime? ParseDate(dynamic value)
        {
            if (value == null) return null;
            var str = value.ToString();
            if (DateTime.TryParse(str, out DateTime res))
                return res;
            return null;
        }
    }
}