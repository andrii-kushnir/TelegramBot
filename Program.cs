using System;
using System.Collections.Generic;
using System.Globalization;
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
using TeleSharp.TL.Updates;
using TLSharp.Core;
using WordsMatching;

namespace TelegramBot
{
    class Program
    {
        private const string logFileName = "Log.txt";
        private static List<UserSQL> usersSQL;

        private const string txtStart = "Привіт! Ви хто?";
        private const string txtWorkerArc = "Вітаю працівника АРС!";
        private const string txtClientArc = "Вітаю клієнта АРС! Це бот компанії АРС під назвою АРСеній. Я буду присилати вам сюди цікаві пропозиції.";
        private const string txtNeither = "До побачення.";

        //бот Арсеній
        private const int apiId = 1805032362;
        private const string apiHash = "AAEmfvbU4lxx4WDCtwUhaVPofbg8vKa5QDI";

        private static TelegramBotClient _botClient;
        private static readonly List<User> _users = new List<User>();
        private static readonly List<Message> _messages = new List<Message>();

        //бот АндрійБот
        //private const int apiId = 1802156713;
        //private const string apiHash = "AAHA1ZNEMBW94UHppMMU6RrMP-nxjWuQsXw";

        //Andrii_My_API
        //private const int apiId = 6423473;
        //private const string apiHash = "37e7866209e483e9f35edb45e96950b1";
        private const string sessionPath = @"C:\Users\Andrii.Kushnir\source\repos\TelegramBot\bin\Debug\";
        private const string phone = "380689559241";
        private static string code = "46138";
        //Test configuration:149.154.167.40:443
        //Production configuration:149.154.167.50:443

        private static TelegramClient _botApi;

        static void Main(string[] args)
        {
            Console.Title = "Це АРСеній! Telegram-бот компанії АРС.";
            usersSQL = UsersFromSQL.Do();

            Do_Bot();

            //Do_TelegramAPI().Wait();
        }

        [Obsolete]
        static void Do_Bot()
        {
            var token = $"{apiId}:{apiHash}";
            _botClient = new TelegramBotClient(token);
            var me = _botClient.GetMeAsync().Result;

            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine($"Я АРСеній, бот компанії АРС. Увага, я працюю!!!\nI am user {me.Id} and my name is {me.FirstName}.");

            _botClient.OnMessage += Bot_OnMessage;
            _botClient.StartReceiving();

            Console.WriteLine();

            var fileStream = new StreamWriter(sessionPath + logFileName, true);

            // Main Handler
            EventHandler<CallbackQueryEventArgs> CallbackQueryEvent = (object sender, CallbackQueryEventArgs ev) =>
            {
                var chatId = ev.CallbackQuery.Message.Chat.Id;
                var user = _users.FirstOrDefault(u => u.UserId == ev.CallbackQuery.From.Id);

                _botClient.EditMessageReplyMarkupAsync(chatId, ev.CallbackQuery.Message.MessageId, null).Wait();

                switch (ev.CallbackQuery.Data)
                {
                    case "workingArc":
                        user.Type = ClientType.Worker;
                        _botClient.SendTextMessageAsync(chatId, txtWorkerArc).Wait();

                        if (user.LastName == null || user.FirstName == null)
                        {
                            _botClient.SendTextMessageAsync(chatId, $"Нажаль ви не вказали імені або прізвища в своєму акаунті Telegram, тому ми не змогли вас індентифікувати.").Wait();
                            _botClient.SendTextMessageAsync(chatId, $"Id нашої розмови: {chatId}.\nЗверніться в компютерний відділ для індентифікації.").Wait();
                        }
                        else
                        {
                            var usersSQLApproximate = GetApproximateUsers(ev.CallbackQuery.From);

                            var buttonWorkerIndefity = new InlineKeyboardMarkup(new[] {
                                                new[] { InlineKeyboardButton.WithCallbackData($"1. {usersSQLApproximate[0].LastName} {usersSQLApproximate[0].FirstName} {usersSQLApproximate[0].Surname}", "var1")},
                                                new[] { InlineKeyboardButton.WithCallbackData($"2. {usersSQLApproximate[1].LastName} {usersSQLApproximate[1].FirstName} {usersSQLApproximate[1].Surname}", "var2")},
                                                new[] { InlineKeyboardButton.WithCallbackData($"3. {usersSQLApproximate[2].LastName} {usersSQLApproximate[2].FirstName} {usersSQLApproximate[2].Surname}", "var3")},
                                                new[] { InlineKeyboardButton.WithCallbackData($"Тут мене немає", "nothing")}
                                            });
                            _botClient.SendTextMessageAsync(chatId, "Індентифікуйте себе:", replyMarkup: buttonWorkerIndefity).Wait();
                        }
                        break;
                    case "clientArc":
                        user.Type = ClientType.Client;
                        user.LastNameArc = "Клієнт"; // заповнити в майбутньому !!!!
                        user.FirstNameArc = "Клієнт"; // заповнити в майбутньому !!!!
                        user.SurnameArc = "Клієнт"; // заповнити в майбутньому !!!!
                        user.IdArc = 1; // заповнити в майбутньому !!!!
                        _botClient.SendTextMessageAsync(chatId, txtClientArc).Wait();
                        break;
                    case "nothingArc":
                        user.Type = ClientType.Neither;
                        _botClient.SendTextMessageAsync(chatId, txtNeither).Wait();
                        break;
                    case "var1":
                    case "var2":
                    case "var3":
                        var userSQL = GetApproximateUsers(ev.CallbackQuery.From)[Convert.ToInt32(ev.CallbackQuery.Data.Substring(3, 1)) - 1];
                        _botClient.SendTextMessageAsync(chatId, $"Вітаємо {userSQL.LastName} {userSQL.FirstName} {userSQL.Surname}. Ви індентифіковані і записані в базу.").Wait();
                        user.LastNameArc = userSQL.LastName;
                        user.FirstNameArc = userSQL.FirstName;
                        user.SurnameArc = userSQL.Surname;
                        user.IdArc = userSQL.Id;
                        userSQL.TelegramId = user.UserId;
                        break;
                    case "nothing":
                        _botClient.SendTextMessageAsync(chatId, $"Id нашої розмови: {chatId}.\nЗверніться в компютерний відділ для індентифікації.").Wait();
                        break;
                    default:
                        break;
                }
            };
            _botClient.OnCallbackQuery += CallbackQueryEvent;

            var infinity = true;
            while (infinity)
            {
                Thread.Sleep(100); // Забрати(зменшити) якщо велика нагрузка(багато повідомлень)
                var message = _messages.FirstOrDefault(m => (!m.Done));
                if (message != null)
                {
                    fileStream.WriteLine($"{message.ChatId},{message.User.FirstName},{message.User.LastName},{message.Text}");
                    fileStream.Flush();

                    if (message.Text == @"/start")
                    {
                        if (message.User.IdArc != 0)
                        {
                            switch (message.User.Type)
                            {
                                case ClientType.Worker:
                                    _botClient.SendTextMessageAsync(message.ChatId, $"Шановний {message.User.LastNameArc} {message.User.FirstNameArc} {message.User.SurnameArc}, ви вже індентифіковані. Якщо щось не так - зверніться в компютерний відділ.").Wait();
                                    break;
                                case ClientType.Client:
                                    _botClient.SendTextMessageAsync(message.ChatId, $"Шановний клієнте {message.User.LastNameArc} {message.User.FirstNameArc} {message.User.SurnameArc}, ви вже індентифіковані. Якщо щось не так - зверніться в компютерний відділ.").Wait();
                                    break;
                                case ClientType.Neither:
                                    break;
                            }
                        }
                        else
                        {
                            var buttonStart = new InlineKeyboardMarkup(new[] {
                                new[] { InlineKeyboardButton.WithCallbackData("Працівник компанії АРС", "workingArc")},
                                new[] { InlineKeyboardButton.WithCallbackData("Клієнт компанії АРС", "clientArc")},
                                new[] { InlineKeyboardButton.WithCallbackData("Випадково зайшов", "nothingArc")}
                            });
                            _botClient.SendTextMessageAsync(message.ChatId, txtStart, replyMarkup: buttonStart).Wait();
                        }
                    }

                    if (message.Text == @"/getid")
                    {
                        _botClient.SendTextMessageAsync(message.ChatId, $"Id нашої розмови: {message.ChatId}");
                        Console.WriteLine($"Send a message '{message.ChatId}' to {message.User.FirstName} {message.User.LastName}");
                    }

                    if (message.Text == @"/stop")
                    {
                        infinity = false;
                    }

                    message.Done = true;
                }
            }

            fileStream.Dispose();
            _botClient.StopReceiving();
        }

        private static object locker = new object();
        static List<UserSQL> GetApproximateUsers(Telegram.Bot.Types.User from)
        {
            List<UserSQL> result = null;
            lock (locker)
            {
                result = usersSQL.Where(user => user.TelegramId == 0)
                                    .Select(u =>
                                    {
                                        if (Transliteration.Translit(u.LastName) == Transliteration.Translit(from.LastName))
                                        {
                                            var match = new MatchsMaker(Transliteration.Translit(u.FirstName), Transliteration.Translit(from.FirstName));
                                            u.Vaga = 1 + match.GetScore();
                                        }
                                        else
                                        {
                                            var match = new MatchsMaker(Transliteration.Translit(u.LastName), Transliteration.Translit(from.LastName));
                                            u.Vaga = match.GetScore();
                                        }
                                        return u;
                                    })
                                    .OrderByDescending(x => x.Vaga)
                                    .Take(3)
                                    .ToList();
            }
            return result;
        }

        static void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Console.WriteLine($"Received a message  '{e.Message.Text}'  from {e.Message.From}");
                var user = _users.FirstOrDefault(u => u.UserId == e.Message.From.Id);
                if (user == null)
                {
                    user = new User()
                    {
                        UserId = e.Message.From.Id,
                        Username = e.Message.From.Username,
                        FirstName = e.Message.From.FirstName,
                        LastName = e.Message.From.LastName,
                    };
                    var userSQL = usersSQL.FirstOrDefault(u => u.TelegramId == e.Message.From.Id);
                    if (userSQL != null)
                    {
                        user.FirstNameArc = userSQL.FirstName;
                        user.LastNameArc = userSQL.LastName;
                        user.SurnameArc = userSQL.Surname;
                        user.IdArc = userSQL.Id;
                        user.Type = ClientType.Worker; // or ClientType.Client !!!!!!
                    };
                    _users.Add(user);
                }
                var message = new Message
                {
                    MessageId = e.Message.MessageId,
                    User = user,
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

            if (!_botApi.IsUserAuthorized())
            {
                AuthenticationAsync().Wait();
            }

            //SendMessage("380676722619", "Вперьод!").Wait();  //Третьяков
            //SendMessage("380930418206", "Test!!!").Wait(); //Дмитро
            //SendMessageToChannel("Група Арсенія", "Не хвилюйтесь, це тест").Wait();

            SendMessage("380352555555", "Test!!!").Wait(); //не існує

            Console.ReadLine();
            _botApi.Dispose();
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
                var result = await _botApi.ImportContactsAsync(list);
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

        //Not work
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
