using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    class MyFunc
    {
        public static string GetFormattedTime()
        {
            DateTime now = DateTime.Now;
            string formattedTime = $"{now:dd-HH-mm-ss}";
            return formattedTime;
        }
    }
}
