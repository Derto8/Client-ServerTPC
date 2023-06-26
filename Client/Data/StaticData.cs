using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client.Data
{
    public static class StaticData
    {
        public static IPAddress IP { get; set; }
        public static int Port { get; set; }
        public static Guid UCode { get; set; }
        public static Socket Socket { get; set; }
    }
}
