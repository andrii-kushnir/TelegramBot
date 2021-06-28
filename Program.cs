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
using TeleSharp.TL.Messages;
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

        //Andrii_My_API
        private const string sessionPath = @"C:\Users\Andrii.Kushnir\source\repos\TelegramBot\bin\Debug\";
        private const int apiId = 6423473;
        private const string apiHash = "37e7866209e483e9f35edb45e96950b1";
        private const string phone = "380689559241";
        private static string code = "46138"; 

        //Test configuration:149.154.167.40:443
        //Production configuration:149.154.167.50:443



        private static TelegramBotClient _botClient;
        private static readonly List<Message> _messages = new List<Message>();

        private static TelegramClient _botApi;

        static void Main(string[] args)
        {
            //var token = $"{apiId}:{apiHash}";
            //Do_Bot();

            Do_TelegramAPI().Wait();
            if (!_botApi.IsUserAuthorized())
            {
                AuthenticationAsync().Wait();
            }

            //SendMessage("380676722619", "Вперьод!").Wait();  //Третьяков
            //SendMessage("380930418206", "По чому помідори?").Wait(); 
            SendMessageToChannel("Група Арсенія", "Не хвилюйтесь, це тест").Wait();

            Console.ReadLine();
            _botApi.Dispose();
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


        static async Task Do_TelegramAPI()
        {
            _botApi = new TelegramClient(apiId, apiHash, null, sessionPath + phone);
            await _botApi.ConnectAsync();
        }

        static async Task AuthenticationAsync()
        {
            var hash = await _botApi.SendCodeRequestAsync(phone); // запам"ятати код і прописати в константу code!

            if (String.IsNullOrWhiteSpace(code))
            {
                code = Console.ReadLine();
            }
            try
            {
                await _botApi.MakeAuthAsync(phone, hash, code);
            }
            catch (Exception)
            {
                code = Console.ReadLine();
                await _botApi.MakeAuthAsync(phone, hash, code);
            }
        }

        static async Task SendMessage(string phone, string message)
        {
            var user = await GetUser(phone);
            if (user != null)
            {
                await _botApi.SendMessageAsync(new TLInputPeerUser() { UserId = user.Id }, message);
            }
            else
            {
                Console.WriteLine($"Користувач з телефоном {phone} не знайдений");
            }
        }

        static async Task<TLUser> GetUser(string phone)
        {
            var users = await _botApi.GetContactsAsync();

            var user = users.Users
                .Where(x => x.GetType() == typeof(TLUser))
                .Cast<TLUser>()
                .FirstOrDefault(x => x.Phone == phone);
            if (user == null)
            {
                Console.Write("Новий користувач. Введіть ім'я: ");
                var firstName = Console.ReadLine();
                Console.Write("Введіть прізвище: ");
                var lastName = Console.ReadLine();
                var list = new List<TLInputPhoneContact> { new TLInputPhoneContact() { Phone = phone, FirstName = firstName, LastName = lastName, ClientId = 0 } };
                await _botApi.ImportContactsAsync(list);
                users = await _botApi.GetContactsAsync();
                user = users.Users
                            .Where(x => x.GetType() == typeof(TLUser))
                            .Cast<TLUser>()
                            .FirstOrDefault(x => x.Phone == phone);
            }
            return user;
        }

        static async Task SendMessageToChannel(string channel, string message)
        {
            var channels = (TLDialogs)await _botApi.GetUserDialogsAsync();
            if (channels != null)
            {
                //var chats = channels.Chats.ToList(); // 3 і більше людей
                //var dialogs = channels.Dialogs.ToList(); // Chats + Users
                //var users = channels.Users.ToList(); // розмови 1 на 1
                //var messages = channels.Messages.ToList(); // останні повідомлення кожної розмови

                var chat = channels.Chats
                            .Where(x => x.GetType() == typeof(TLChat))
                            .Cast<TLChat>()
                            .FirstOrDefault(a => a.Title == channel);
                if (chat != null)
                {
                    await _botApi.SendMessageAsync(new TLInputPeerChat() { ChatId = chat.Id }, message);
                }
                else
                {
                    Console.WriteLine($"Канал {channel} не знайдений");
                }
            }
        }

        static async Task JoinChannel(string phone, string message)
        {
            TLInputPhoneContact canal;
            var request = new TeleSharp.TL.Channels.TLRequestJoinChannel()
            {
                Channel = new TLInputChannel
                {
                    //ChannelId = channelInfo.Id,
                    //AccessHash = (long)channelInfo.AccessHash
                }
            };

            var responsjoin = await _botApi.SendRequestAsync<TLUpdates>(request);
        }
    }
}
