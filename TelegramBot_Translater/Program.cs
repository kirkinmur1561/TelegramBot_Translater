using System;
using System.Threading.Tasks;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Diagnostics;

namespace TelegramBot_Translater
{
    class Program
    {
        /// <summary>
        /// кратность часа
        /// </summary>
        const int MultipleOf = 3;

        /// <summary>
        /// Список клиентов
        /// </summary>
        static List<Client> Clients = new List<Client>();

        /// <summary>
        /// объект бота
        /// </summary>
        public static TelegramBotClient Bot;

        /// <summary>
        /// Текст, который будет отправлен в случаи промоха скрипта
        /// </summary>
        static string[] text_default = new string[] 
        {
            "Это Вы о чем?",
            "Моя твоя не понимать...",
            "... там будет лестница вверх. Спускайся. Поняли? А я вас тем более...",
            "Говорили мне в школе-ботов: \"Учи человеческие языки! Пригодится... \". Простите, я Вас не понимаю.",
            "Мои экстрасенсорные способности исчерпаны. Я не понимаю, что хотят духи..."
        };

        /// <summary>
        /// Словарь
        /// </summary>
        static List<Word> Dictionary = new List<Word>()
        {
            new Word("красный","red"),
            new Word("оранжевый","orange"),
            new Word("желтый","yellow"),
            new Word("зеленый","green"),
            new Word("голубой","blue"),
            new Word("синий","blue"),
            new Word("фиолетовый","violet"),
            new Word("школа","school"),
            new Word("машина","vehicle"),
            new Word("учитель","teacher"),
            new Word("семья","family"),
            new Word("традиция","tradition"),
            new Word("брат","brother"),
            new Word("сестра","sister"),
            new Word("отец","father"),
            new Word("мать","mather"),
            new Word("бабушка","grandmother"),
            new Word("дедушка","grandfather")
        };

        /// <summary>
        /// Отправка словаря 
        /// </summary>
        static async void SendWord() => await Task.Run(async () => 
        {
            while (true)
            {
                DateTime date = DateTime.Now;
                List<int> randomNum = new List<int>();
                while (true)
                {
                    int num = new Random().Next(Dictionary.Count);
                    if (!randomNum.Contains(num)) randomNum.Add(num);
                    if (randomNum.Count == 9) break;
                }

                
                Clients
                .Where(w => w.IsProceed &&
                   w.StartMessage != null &&
                   w.EndMessage != null &&
                   date.Hour >= w.StartMessage.Value.Hours &&
                   date.Hour <= w.EndMessage.Value.Hours)?
                .ToList()?
                .ForEach(f => 
                {
                    string text = "";
                  
                    for (int i = 0; i < f.CountWord; i++)
                    {
                        Word w = Dictionary[randomNum[i]];
                        text += $"{w.Eng} - {w.Ru}\n";
                    }
                    Bot.SendTextMessageAsync(f.id, $"{text}");
                });
                await Task.Delay(TimeSpan.Parse("01:00:00"));
            }
        });

        /// <summary>
        /// Генерация временного диапозона
        /// </summary>
        /// <param name="client">Ссылка на клиента</param>
        /// <param name="isStart">Если параметр true (необязательный), то время генерируется для стартового времяни диапозона. Если параметр false - то для конечного времянного диапозона</param>
        /// <returns>Массив времянного диапозона</returns>
        static IEnumerable<TimeSpan> GenerateTime(Client client,bool isStart = true)
        {
            IEnumerable<TimeSpan> timeSpans = Enumerable
                .Range(6, 18)
                .Where(w => w % MultipleOf == 0)
                .Select(s => TimeSpan.FromHours(s));
          

            if (isStart)
            {
                if (client?.EndMessage == null)
                    return timeSpans.Take(timeSpans.Count()-1);
                else
                    return timeSpans.Where(w => w < client?.EndMessage);
            }
            else
            {
                if (client?.StartMessage == null)
                    return timeSpans.Skip(1);
                else
                    return timeSpans.Where(w => w > client?.StartMessage);
            }
        }

        
        /// <summary>
        /// Вход в программу
        /// </summary>
        static async Task Main(string[] args)
        {
            string? tel_token = ConfigurationManager.AppSettings.Get("botToken");
            if(tel_token == null || tel_token == "")
            {
                Console.WriteLine("Токен для бота отсутствует.\n" +
                    "В app.config в value напишите свой токен");
                Console.Read();
                return;
            }
            Bot = new TelegramBotClient(tel_token);
            Bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync);


            await Bot.SetMyCommandsAsync(new List<BotCommand>()//
            {
                new BotCommand()
                {
                    Command="info",
                    Description = "Посмотреть настройки"
                },
                new BotCommand()
                {
                    Command = "pause",
                    Description = "Пауза"
                },
                new BotCommand()
                {
                    Command = "proceed",
                    Description = "Продолжить"
                }

            });

            SendWord();

            Console.Read();
        }

        private static Task HandleErrorAsync(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)=> 
            Task.CompletedTask;


        /// <summary>
        /// Обработка текста и команд
        /// </summary>
        private static async void WordProcessing(ITelegramBotClient arg1, Update arg2) 
        {
            Debug.WriteLine($"participant:{arg2.Message.Chat.FirstName}\n\tMessage:{arg2.Message.Text??"**Null**"}");
            switch (arg2.Message.Text)
            {
                #region info
                case "/info":
                    Client cl = Clients.FirstOrDefault(f => f.id == arg2.Message.Chat.Id);
                    if (cl == null) return;
                    if (cl.CountWord == null    || 
                        cl.EndMessage == null   || 
                        cl.StartMessage == null)
                        await arg1.SendTextMessageAsync(arg2.Message.Chat, $"Еще не все настройки применены.");

                    else
                        await arg1.SendTextMessageAsync(arg2.Message.Chat,
                                                    $"Время старта: {cl.StartMessage?.ToString(@"hh\:mm")}\n" +
                                                    $"Время окончания:{cl.EndMessage?.ToString(@"hh\:mm")}\n" +
                                                    $"Количество слов:{cl.CountWord}");
                    break;
                #endregion
                #region start
                case "/start":
                    Client client = new Client() { id = arg2.Message.Chat.Id};
                    if (!Clients.Contains<Client>(client)) 
                    {
                        await arg1
                           .SendTextMessageAsync(arg2.Message.Chat,
                                               "Этот чат-бот поможет Вам запомнить новые слова.\nЧерез команды можно настроить бота.",
                                               replyMarkup: new InlineKeyboardMarkup
                                               (
                                                   new List<List<InlineKeyboardButton>>()
                                                   {
                                                            new List<InlineKeyboardButton>()
                                                            {
                                                                InlineKeyboardButton.WithCallbackData("Настройка времяни старта")
                                                            },
                                                            new List<InlineKeyboardButton>()
                                                            {
                                                                InlineKeyboardButton.WithCallbackData("Настройка времяни окончания")
                                                            },
                                                            new List<InlineKeyboardButton>()
                                                            {
                                                                InlineKeyboardButton.WithCallbackData("Количество слов")
                                                            }
                                                   }
                                               ));
                        Clients.Add(client);
                    }
                    
                    

                    else await arg1.SendTextMessageAsync(arg2.Message.Chat,
                                                    "Этот чат-бот поможет Вам запомнить новые слова.\n" +
                                                    "Через команды можно настроить бота.\n" +
                                                    "C возвращением!");
                    
                    break;
                #endregion
                #region pause
                case "/pause":
                    Client cl_pause = Clients.FirstOrDefault(f => f.id == arg2.Message.Chat.Id);
                    if (cl_pause == null)
                        return;
                    if (cl_pause.IsProceed == false)
                        await arg1.SendTextMessageAsync(arg2.Message.Chat, "Вы уже ставили на паузу");
                    else
                    {
                        cl_pause.IsProceed = false;
                        await arg1.SendTextMessageAsync(arg2.Message.Chat, "Бот пока что не будет Вам писать");
                    }
                    
                    break;
                #endregion
                #region proceed
                case "/proceed":
                    Client cl_proceed = Clients.FirstOrDefault(f => f.id == arg2.Message.Chat.Id);
                    if (cl_proceed == null)
                        return;
                    if(cl_proceed.IsProceed == true)
                        await arg1.SendTextMessageAsync(arg2.Message.Chat, "Вы уже ставили на продолжение");
                    else
                    {
                        cl_proceed.IsProceed = true;
                        await arg1.SendTextMessageAsync(arg2.Message.Chat, "Бот будет Вам писать");
                    }
                    
                    break;
                #endregion
                #region default
                default:
                    bool isText = Regex.IsMatch(arg2.Message.Text.ToLower(), "^([a-z]{1,}|[а-яё]{1,})$");
                    if (!isText)
                    {
                        await arg1.SendTextMessageAsync(arg2.Message.Chat,
                                           text_default[new Random().Next(text_default.Length)]//,
                                           /*replyToMessageId: arg2.Message.MessageId*/);
                        return;
                    }

                    List<Word> translate;

                    bool isEng = Regex.IsMatch(arg2.Message.Text.ToLower(), "^[a-z]{1,}$");
                    if (isEng)
                    {
                        translate = Dictionary.Where(w => w.Eng == arg2.Message.Text.ToLower()).ToList();
                        if (translate == null || translate.Count() == 0) await arg1.SendTextMessageAsync(arg2.Message.Chat, "Бот не знает Вашего слова");
                        else await arg1.SendTextMessageAsync(arg2.Message.Chat, $"Возможный перевод: {string.Join("; ",translate.Select(s=>s.Ru))}");
                    }
                    else
                    {
                        translate = Dictionary.Where(w => w.Ru == arg2.Message.Text.ToLower()).ToList();
                        if (translate == null || translate.Count() == 0) await arg1.SendTextMessageAsync(arg2.Message.Chat, "Бот не знает Вашего слова");
                        else await arg1.SendTextMessageAsync(arg2.Message.Chat, $"Возможный перевод: {string.Join("; ",translate.Select(s=>s.Eng))}");
                    }
                    
                    break;
                    #endregion
            }
        }

        /// <summary>
        /// Возврат в начальное меню
        /// </summary>
        private static async void PressBack(ITelegramBotClient arg1, Update arg2)
        {
            await arg1
                .EditMessageReplyMarkupAsync(arg2.CallbackQuery.Message.Chat,
                    arg2.CallbackQuery.Message.MessageId,
                    new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
                    {
                        new List<InlineKeyboardButton>()
                        {
                        InlineKeyboardButton.WithCallbackData("Настройка времяни старта"),
                        },
                        new List<InlineKeyboardButton>()
                        {
                            InlineKeyboardButton.WithCallbackData("Настройка времяни окончания")
                        },
                        new List<InlineKeyboardButton>()
                        {
                            InlineKeyboardButton.WithCallbackData("Количество слов")
                        }
                    }));
        }

        /// <summary>
        /// Если была нажата кнопка
        /// </summary>
        private static async void ButtonProcessing(ITelegramBotClient arg1, Update arg2)
        {
            if (Clients.FirstOrDefault(f => f.id == arg2.CallbackQuery.From.Id) == null) return;
            if (arg2.CallbackQuery.Data == "< Назад")
            {
                PressBack(arg1, arg2);
                return;
            }

            if (arg2.CallbackQuery.Message.ReplyMarkup?.InlineKeyboard?.Count() == 2)
            {
                TimeSpan time = TimeSpan.Parse(arg2
                    .CallbackQuery
                    .Message
                    .ReplyMarkup
                    .InlineKeyboard
                    .FirstOrDefault()
                    .FirstOrDefault()
                    .CallbackData);

                if (time == GenerateTime(null).FirstOrDefault())
                    Clients.FirstOrDefault(f => f.id == arg2.CallbackQuery.From.Id).StartMessage = TimeSpan.Parse(arg2.CallbackQuery.Data);

                else
                    Clients.FirstOrDefault(f => f.id == arg2.CallbackQuery.From.Id).EndMessage = TimeSpan.Parse(arg2.CallbackQuery.Data);

                PressBack(arg1, arg2);
            }
            else if(arg2.CallbackQuery.Message.ReplyMarkup?.InlineKeyboard.Count()==4)
            {
                int cw = int.Parse(arg2.CallbackQuery.Data);
                Clients.FirstOrDefault(f => f.id == arg2.CallbackQuery.From.Id).CountWord = cw;
                PressBack(arg1, arg2);
            }
            else
            {

            }
            

            switch (arg2.CallbackQuery.Data)
            {
                case "Настройка времяни старта":
                    await arg1
                        .EditMessageReplyMarkupAsync(arg2.CallbackQuery.Message.Chat,
                                                    arg2.CallbackQuery.Message.MessageId,
                                                    new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
                                                    {
                                                        GenerateTime( Clients.FirstOrDefault(f => f.id == arg2.CallbackQuery.From.Id))
                                                        .Select(s=>InlineKeyboardButton.WithCallbackData($"{s.ToString(@"hh\:mm")}"))
                                                        .ToList(),

                                                        new List<InlineKeyboardButton>()
                                                        {
                                                            InlineKeyboardButton.WithCallbackData("< Назад")
                                                        }
                                                    }));
                    break;
                case "Настройка времяни окончания":
                    await arg1
                        .EditMessageReplyMarkupAsync(arg2.CallbackQuery.Message.Chat,
                                                    arg2.CallbackQuery.Message.MessageId,
                                                    new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
                                                    {
                                                         GenerateTime(Clients.FirstOrDefault(f => f.id == arg2.CallbackQuery.From.Id),
                                                                      false)
                                                        .Select(s=>InlineKeyboardButton.WithCallbackData($"{s.ToString(@"hh\:mm")}"))
                                                        .ToList(),
                                                        new List<InlineKeyboardButton>(){InlineKeyboardButton.WithCallbackData("< Назад") }
                                                    }));
                  
                    break;
                case "Количество слов":
                    await arg1.EditMessageReplyMarkupAsync(arg2.CallbackQuery.Message.Chat,
                        arg2.CallbackQuery.Message.MessageId,
                        new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
                        {
                            Enumerable
                            .Range(1,3)
                            .Select(s=> InlineKeyboardButton.WithCallbackData(s.ToString()))
                            .ToList(),

                            Enumerable
                            .Range(4,3)
                            .Select(s=> InlineKeyboardButton.WithCallbackData(s.ToString()))
                            .ToList(),

                            Enumerable
                            .Range(7,3)
                            .Select(s=> InlineKeyboardButton.WithCallbackData(s.ToString()))
                            .ToList(),

                            new List<InlineKeyboardButton>()
                            {
                                 InlineKeyboardButton.WithCallbackData("< Назад")
                            }
                        }));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// обновление данных
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        private static async Task HandleUpdateAsync(ITelegramBotClient arg1, Update arg2, CancellationToken arg3) => await Task.Run(() =>
          {
              switch (arg2.Type)
              {
                  case UpdateType.Unknown:
                      break;
                  case UpdateType.Message:
                      WordProcessing(arg1, arg2);
                      break;
                  case UpdateType.CallbackQuery:
                      ButtonProcessing(arg1, arg2);
                      break;
                  default:
                      break;
              }
          });
    }
}
