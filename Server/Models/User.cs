using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    internal class User
    {
        public int Id { get; set; }
        public Guid UCode { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public bool Banned { get; set; }
        public IPAddress? Ip { get; set; }
        public int Port { get; set; }
        public DateTime ConnectionOpen { get; set; }
    }
}
