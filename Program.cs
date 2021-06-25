using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TeleSharp.TL;
using TLSharp.Core;

namespace TelegramBot
{
    class Program
    {
        //бот Арсеній
        //private const int apiId = 1805032362;
        //private const string apiHash = "AAEmfvbU4lxx4WDCtwUhaVPofbg8vKa5QDI";

        //бот АндрійБот
        //private const int apiId = 1802156713;
        //private const string apiHash = "AAHA1ZNEMBW94UHppMMU6RrMP-nxjWuQsXw";

        //AndriiBot
        private const int apiId = 6423473;
        private const string apiHash = "37e7866209e483e9f35edb45e96950b1";

        //Test configuration:149.154.167.40:443
        //Production configuration:149.154.167.50:443



        private static TelegramBotClient _botClient;
        private static readonly List<Message> _messages = new List<Message>();

        private static TelegramClient _botApi;

        static void Main(string[] args)
        {
            //Do_Bot();

            var phone = "380689559241";
            Do_TelegramAPI(phone).Wait();
            AuthenticationAsync(phone).Wait();

            SendMessage("380676722619", "Вперьод!").Wait();

            Console.ReadLine();
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


        static async Task Do_TelegramAPI(string phone)
        {
            var token = $"{apiId}:{apiHash}";

            var store = new FileSessionStore(new DirectoryInfo(@"C:\Users\Andrii.Kushnir\source\repos\TelegramBot\bin\Debug"));

            _botApi = new TelegramClient(apiId, apiHash, store, phone);
            await _botApi.ConnectAsync();
        }

        static async Task AuthenticationAsync(string phone)
        {
            var hash = await _botApi.SendCodeRequestAsync(phone); // запам"ятати цей код!!!!!!!!!!!!!!!!!!!!!!!!

            var code = "72592"; // you can change code in debugger

            var user = await _botApi.MakeAuthAsync(phone, hash, code);
        }

        static async Task SendMessage(string phone, string message)
        {
            var result = await _botApi.GetContactsAsync();

            var user = result.Users
                .Where(x => x.GetType() == typeof(TLUser))
                .Cast<TLUser>()
                .FirstOrDefault(x => x.Phone == phone);

            if (user == null)
            {
                var list = new List<TLInputPhoneContact> {new TLInputPhoneContact() { Phone = phone, FirstName = "Саша", LastName = "Третьяков", ClientId = 0}};
                await _botApi.ImportContactsAsync(list);
                result = await _botApi.GetContactsAsync();
                user = result.Users
                            .Where(x => x.GetType() == typeof(TLUser))
                            .Cast<TLUser>()
                            .FirstOrDefault(x => x.Phone == phone);
            }

            await _botApi.SendMessageAsync(new TLInputPeerUser() { UserId = user.Id }, message);
        }
    }
}
