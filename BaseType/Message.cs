using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class Message
    {
        public int MessageId { get; set; }
        public User User { get; set; }
        public long ChatId { get; set; }
        public string Text { get; set; }
        public bool Done { get; set; }
    }
}
