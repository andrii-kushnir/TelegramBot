using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    class Program
    {
        //бот Арсеній
        private const int apiId = 1805032362;
        private const string apiHash = "AAEmfvbU4lxx4WDCtwUhaVPofbg8vKa5QDI";

        //бот АндрійБот
        //private const int apiId = 1802156713;
        //private const string apiHash = "AAHA1ZNEMBW94UHppMMU6RrMP-nxjWuQsXw";

        private static TelegramBotClient _botClient;
        private static readonly List<Message> _messages = new List<Message>();

        static void Main(string[] args)
        {
            Do_Bot();
        }

        [Obsolete]
        static void Do_Bot()
        {
            var token = $"{apiId}:{apiHash}";
            var _botClient = new TelegramBotClient(token);
            var me = _botClient.GetMeAsync().Result;
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");


            //Message message = _botClient.SendTextMessageAsync(chatId: 1803378664, // or a chat id: 123456789
            //                                                  text: "Trying *all the parameters* of `sendMessage` method",
            //                                                  parseMode: ParseMode.Markdown,
            //                                                  disableNotification: true,
            //                                                  //replyToMessageId: e.Message.MessageId,
            //                                                  contact: 
            //                                                  replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Check sendMessage method", "https://core.telegram.org/bots/api#sendmessage"))
            //                                                  ).Result;

            _botClient.OnMessage += Bot_OnMessage;
            _botClient.StartReceiving();

            Console.WriteLine();

            var infinity = true;
            while (infinity)
            {
                //var message = Console.ReadLine();
                //if (message != "")
                //{
                //    _botClient.SendTextMessageAsync(_messages[_messages.Count - 1].ChatId, message);
                //}

                Thread.Sleep(100);
                var message = _messages.FirstOrDefault(m => (!m.Done));
                if (message != null)
                {
                    if (message.Text == @"/getid")
                    {
                        _botClient.SendTextMessageAsync(message.ChatId, message.ChatId.ToString());
                        Console.WriteLine($"Send a message '{message.ChatId.ToString()}' to {message.FirstName} {message.LastName}");
                    }

                    if (message.Text == @"/stop")
                    {
                        infinity = false;
                    }

                    message.Done = true;
                }
            }

            _botClient.StopReceiving();
        }

        static void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Console.WriteLine($"Received a message  '{e.Message.Text}'  from {e.Message.From}");
                var message = new Message
                {
                    UserId = e.Message.From.Id,
                    Username = e.Message.From.Username,
                    FirstName = e.Message.From.FirstName,
                    LastName = e.Message.From.LastName,
                    ChatId = e.Message.Chat.Id,
                    Text = e.Message.Text,
                    Done = false
                };
                _messages.Add(message);
            }
        }


        //static void Do(int apiId, string apiHash)
        //{
        //    _client = new TelegramClient(apiId, apiHash);
        //    _client.ConnectAsync().Wait();
        //}

        //static void Authentication(string phone)
        //{
        //    var hash = _client.SendCodeRequestAsync(phone).Result;
        //    var code = "<code_from_telegram>"; // you can change code in debugger

        //    var user = _client.MakeAuthAsync(phone, hash, code).Result;
        //}

        //static async Task SendMessage(string phone, string message)
        //{
        //    var result = await _client.GetContactsAsync();

        //    //find recipient in contacts
        //    var user = result.Users
        //        .Where(x => x.GetType() == typeof(TLUser))
        //        .Cast<TLUser>()
        //        .FirstOrDefault(x => x.Phone == phone);

        //    //send message
        //    await _client.SendMessageAsync(new TLInputPeerUser() { UserId = user.Id }, message);
        //}
    }
}
