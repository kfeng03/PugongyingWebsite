using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace fyp
{
    public static class CategoryUtility
    {
        public static List<string> GetAvailableCategories()
        {
            return new List<string>
        {
            "History",
            "Leadership",
            "Language",
            "Camping",
            "Technology"
        };
        }
    }

}