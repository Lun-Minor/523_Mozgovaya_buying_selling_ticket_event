using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _523_Mozgovaya
{
    public static class CurrentUser
    {
        public static int IdUser { get; set; }
        public static string Login { get; set; }
        public static string FullName { get; set; }
        public static int IdRole { get; set; }
        public static bool IsLoggedIn => IdUser > 0;
    }
}
