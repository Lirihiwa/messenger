using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace messenger
{
    public class InfoClass
    {
        public static string? GlobalUser { get; set; }

        public static string? GlobalChatName { get; set; }

        public static int GlobalLastMessagesCount { get; set;}

        public static string GlobalDelta
        {
            get
            {
                return "\t\t\t\t";
            }
        }
    }
}
