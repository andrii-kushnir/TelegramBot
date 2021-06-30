using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class User
    {
        public ClientType Type { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LastNameArc { get; set; }
        public string FirstNameArc { get; set; }
        public string SurnameArc { get; set; }
        public int IdArc { get; set; }
    }

    public enum ClientType
    {
        Worker,
        Client,
        Neither
    }
}
