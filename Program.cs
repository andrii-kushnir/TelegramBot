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
        private readonly MatchsMaker match;
        private static List<UserSQL> usersSQL;
        //бот Арсеній
        private const int apiId = 1805032362;
        private const string apiHash = "AAEmfvbU4lxx4WDCtwUhaVPofbg8vKa5QDI";

        private const string txtStart = "Привіт! Ви хто?";
        private const string txtWorkerArc = "Вітаю працівника АРС!";
        private const string txtClientArc = "Вітаю клієнта АРС! Це бот компанії АРС під назвою АРСеній. Я буду присилати вам сюди цікаві пропозиції.";
        private const string txtNeither = "Привіт! Ви хто?";


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



        private static TelegramBotClient _botClient;
        private static readonly List<User> _users = new List<User>();
        private static readonly List<Message> _messages = new List<Message>();

        private static TelegramClient _botApi;

        static void Main(string[] args)
        {
            usersSQL = UsersFromSQL.Do();
            //foreach(var user in usersSQL)
            //{
            //    user.FirstName = Transliteration.Translit(user.FirstName);
            //    user.LastName = Transliteration.Translit(user.LastName);
            //    user.Surname = Transliteration.Translit(user.Surname);
            //}

            Do_Bot();

            //Do_TelegramAPI().Wait();
        }

        [Obsolete]
        static void Do_Bot()
        {
            var token = $"{apiId}:{apiHash}";
            var _botClient = new TelegramBotClient(token);
            var me = _botClient.GetMeAsync().Result;

            Console.OutputEncoding = Encoding.UTF8;
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

            var fileStream = new StreamWriter(sessionPath + logFileName, true);

            EventHandler<CallbackQueryEventArgs> CallbackQueryEvent = null;
            var buttonStart = new InlineKeyboardMarkup(new[] {
                            new[] { InlineKeyboardButton.WithCallbackData("Працівник компанії АРС", "workingArc")},
                            new[] { InlineKeyboardButton.WithCallbackData("Клієнт компанії АРС", "clientArc")},
                            new[] { InlineKeyboardButton.WithCallbackData("Випадково зайшов", "nothingArc")}
                        });

            var infinity = true;
            while (infinity)
            {
                Thread.Sleep(10);
                var message = _messages.FirstOrDefault(m => (!m.Done));
                if (message != null)
                {
                    fileStream.WriteLine($"{message.ChatId},{message.User.FirstName},{message.User.LastName},{message.Text}");
                    fileStream.Flush();

                    if (message.Text == @"/start")
                    {
                        _botClient.SendTextMessageAsync(message.ChatId, txtStart, replyMarkup: buttonStart).Wait();

                        CallbackQueryEvent = (object sender, CallbackQueryEventArgs ev) =>
                        {
                            var chatId = ev.CallbackQuery.Message.Chat.Id;

                            _botClient.EditMessageReplyMarkupAsync(chatId, ev.CallbackQuery.Message.MessageId, null).Wait();

                            if (ev.CallbackQuery.Data == "workingArc")
                            {
                                //foreach (var user in usersSQL)
                                //{
                                //    var match = new MatchsMaker(Transliteration.Translit(user.LastName), ev.CallbackQuery.From.LastName);
                                //    user.Vaga = match.GetScore();
                                //}
                                //var usersLike = usersSQL.OrderByDescending(u => u.Vaga).Take(3).ToList();

                                var user = _users.FirstOrDefault(u => u.UserId == ev.CallbackQuery.From.Id);
                                user.Type = ClientType.Worker;

                                if (user.LastName == null || user.FirstName == null)
                                {
                                    _botClient.OnCallbackQuery -= CallbackQueryEvent;
                                    _botClient.SendTextMessageAsync(chatId, txtWorkerArc).Wait();
                                    _botClient.SendTextMessageAsync(chatId, $"Нажаль ви не вказали імені або прізвища в своєму акаунті Telegram, тому ми не змогли вас індентифікувати.").Wait();
                                    _botClient.SendTextMessageAsync(chatId, $"Id нашої розмови: {chatId}.\nЗверніться в компютерний відділ для індентифікації.").Wait();
                                }
                                else
                                {
                                    var usersLike = usersSQL.Select(u => { var match = new MatchsMaker(Transliteration.Translit(u.LastName), Transliteration.Translit(ev.CallbackQuery.From.LastName)); u.Vaga = match.GetScore(); return u; })
                                                            .OrderByDescending(x => x.Vaga)
                                                            .Take(3)
                                                            .ToList();

                                    var buttonWorkerIndefity = new InlineKeyboardMarkup(new[] {
                                        new[] { InlineKeyboardButton.WithCallbackData($"1. {usersLike[0].LastName} {usersLike[0].FirstName} {usersLike[0].Surname}", "var1")},
                                        new[] { InlineKeyboardButton.WithCallbackData($"2. {usersLike[1].LastName} {usersLike[1].FirstName} {usersLike[1].Surname}", "var2")},
                                        new[] { InlineKeyboardButton.WithCallbackData($"3. {usersLike[2].LastName} {usersLike[2].FirstName} {usersLike[2].Surname}", "var3")},
                                        new[] { InlineKeyboardButton.WithCallbackData($"Тут мене немає", "nothing")}
                                        });
                                    _botClient.SendTextMessageAsync(chatId, txtWorkerArc).Wait();
                                    _botClient.SendTextMessageAsync(chatId, "Індентифікуйте себе:", replyMarkup: buttonWorkerIndefity).Wait();
                                }
                            }
                            else if (ev.CallbackQuery.Data == "clientArc")
                            {
                                _botClient.OnCallbackQuery -= CallbackQueryEvent;
                                _users.FirstOrDefault(u => u.UserId == ev.CallbackQuery.From.Id).Type = ClientType.Client;
                                _botClient.SendTextMessageAsync(chatId, txtClientArc).Wait();
                            }
                            else if (ev.CallbackQuery.Data == "nothingArc")
                            {
                                _botClient.OnCallbackQuery -= CallbackQueryEvent;
                                _users.FirstOrDefault(u => u.UserId == ev.CallbackQuery.From.Id).Type = ClientType.Neither;
                                _botClient.SendTextMessageAsync(chatId, txtNeither).Wait();
                            }
                            else if (ev.CallbackQuery.Data == "var1")
                            {
                                _botClient.OnCallbackQuery -= CallbackQueryEvent;
                                var userSQL = usersSQL.Select(u => { var match = new MatchsMaker(Transliteration.Translit(u.LastName), Transliteration.Translit(ev.CallbackQuery.From.LastName)); u.Vaga = match.GetScore(); return u; })
                                                        .OrderByDescending(x => x.Vaga)
                                                        .First();
                                _botClient.SendTextMessageAsync(chatId, $"Вітаємо {userSQL.LastName} {userSQL.FirstName} {userSQL.Surname}. Ви індентифіковані і записані в базу.").Wait();
                                var user = _users.FirstOrDefault(u => u.UserId == ev.CallbackQuery.From.Id);
                                user.LastNameArc = userSQL.LastName;
                                user.FirstNameArc = userSQL.FirstName;
                                user.SurnameArc = userSQL.Surname;
                                user.IdArc = userSQL.Id;
                            }
                            else if (ev.CallbackQuery.Data == "var2")
                            {
                                _botClient.OnCallbackQuery -= CallbackQueryEvent;
                                var userSQL = usersSQL.Select(u => { var match = new MatchsMaker(Transliteration.Translit(u.LastName), Transliteration.Translit(ev.CallbackQuery.From.LastName)); u.Vaga = match.GetScore(); return u; })
                                                        .OrderByDescending(x => x.Vaga)
                                                        .Skip(1)
                                                        .First();
                                _botClient.SendTextMessageAsync(chatId, $"Вітаємо {userSQL.LastName} {userSQL.FirstName} {userSQL.Surname}. Ви індентифіковані і записані в базу.").Wait();
                                var user = _users.FirstOrDefault(u => u.UserId == ev.CallbackQuery.From.Id);
                                user.LastNameArc = userSQL.LastName;
                                user.FirstNameArc = userSQL.FirstName;
                                user.SurnameArc = userSQL.Surname;
                                user.IdArc = userSQL.Id;
                            }
                            else if (ev.CallbackQuery.Data == "var3")
                            {
                                _botClient.OnCallbackQuery -= CallbackQueryEvent;
                                var userSQL = usersSQL.Select(u => { var match = new MatchsMaker(Transliteration.Translit(u.LastName), Transliteration.Translit(ev.CallbackQuery.From.LastName)); u.Vaga = match.GetScore(); return u; })
                                                        .OrderByDescending(x => x.Vaga)
                                                        .Skip(2)
                                                        .First();
                                _botClient.SendTextMessageAsync(chatId, $"Вітаємо {userSQL.LastName} {userSQL.FirstName} {userSQL.Surname}. Ви індентифіковані і записані в базу.").Wait();
                                var user = _users.FirstOrDefault(u => u.UserId == ev.CallbackQuery.From.Id);
                                user.LastNameArc = userSQL.LastName;
                                user.FirstNameArc = userSQL.FirstName;
                                user.SurnameArc = userSQL.Surname;
                                user.IdArc = userSQL.Id;
                            }
                            else if (ev.CallbackQuery.Data == "nothing")
                            {
                                _botClient.OnCallbackQuery -= CallbackQueryEvent;
                                _botClient.SendTextMessageAsync(chatId, $"Id нашої розмови: {chatId}.\nЗверніться в компютерний відділ для індентифікації.").Wait();
                            }

                        };
                        _botClient.OnCallbackQuery += CallbackQueryEvent;

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
            SendMessage("380930418206", "Test!!!").Wait(); //Дмитро
            //SendMessageToChannel("Група Арсенія", "Не хвилюйтесь, це тест").Wait();

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
            //Not work
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
