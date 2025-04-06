// Telegram Bot для просмотра курса валют

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramCurrencyBot
{
    public class Program
    {
        private static ITelegramBotClient bot;

        public static async Task Main(string[] args)
        {
            bot = new TelegramBotClient("7530994965:AAFBhQTLwYe3tKapmGQqVxwh7bHZHVP2FFk");
            using var cts = new CancellationTokenSource();

            // Настройка получения обновлений
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            bot.StartReceiving(Handlers.OnUpdate, Handlers.OnError, receiverOptions, cts.Token);

            var me = await bot.GetMeAsync();
            Console.WriteLine($"Бот @{me.Username} запущен. Нажмите Enter для завершения работы.");
            Console.ReadLine();
            cts.Cancel();
        }
    }

    public static class Handlers
    {
        private static readonly UserStateManager StateManager = new();

        public static async Task OnUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is { } message)
            {
                await MessageHandler.HandleMessage(botClient, message);
            }
            else if (update.CallbackQuery is { } query)
            {
                await CallbackHandler.HandleCallbackQuery(botClient, query);
            }
        }

        public static Task OnError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }
    }

    public static class MessageHandler
    {
        public static async Task HandleMessage(ITelegramBotClient botClient, Message msg)
        {
            if (msg.Text == "/start")
            {
                await botClient.SendTextMessageAsync(msg.Chat.Id, "Привет! Что вы хотите узнать?",
                    replyMarkup: Keyboards.MainMenu);
            }
            else if (DateTime.TryParse(msg.Text, out DateTime requestedDate))
            {
                if (UserStateManager.UserRequestsKeyRate.Contains(msg.Chat.Id))
                {
                    await KeyRateService.SendKeyRate(botClient, msg.Chat.Id, requestedDate);
                    UserStateManager.UserRequestsKeyRate.Remove(msg.Chat.Id);
                }
                else if (UserStateManager.UserCurrencies.TryGetValue(msg.Chat.Id, out string currency))
                {
                    await ExchangeRateService.SendExchangeRate(botClient, msg.Chat.Id, currency, requestedDate);
                }
                else
                {
                    await botClient.SendTextMessageAsync(msg.Chat.Id, "Сначала выберите валюту.", replyMarkup: Keyboards.MainMenu);
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(msg.Chat.Id, "Извините, я не понимаю этот запрос.", replyMarkup: Keyboards.MainMenu);
            }
        }
    }


    public static class CallbackHandler
    {
        public static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery query)
        {
            if (query.Data == "exchange_rate")
            {
                await botClient.AnswerCallbackQueryAsync(query.Id);
                await botClient.SendTextMessageAsync(query.Message!.Chat.Id, "Выберите валюту:", replyMarkup: Keyboards.CurrencyMenu);
            }
            else if (query.Data == "cb_rate")
            {
                UserStateManager.UserRequestsKeyRate.Add(query.Message!.Chat.Id);
                await botClient.AnswerCallbackQueryAsync(query.Id);
                await botClient.SendTextMessageAsync(query.Message.Chat.Id, "Введите дату или выберите сегодня:", replyMarkup: Keyboards.DateMenu);
            }
            else if (query.Data == "Сегодня")
            {
                var today = DateTime.Today;
                if (UserStateManager.UserRequestsKeyRate.Contains(query.Message!.Chat.Id))
                {
                    await KeyRateService.SendKeyRate(botClient, query.Message.Chat.Id, today);
                    UserStateManager.UserRequestsKeyRate.Remove(query.Message.Chat.Id);
                }
                else if (UserStateManager.UserCurrencies.TryGetValue(query.Message.Chat.Id, out string currency))
                {
                    await ExchangeRateService.SendExchangeRate(botClient, query.Message.Chat.Id, currency, today);
                }
                else
                {
                    await botClient.SendTextMessageAsync(query.Message.Chat.Id, "Сначала выберите валюту.", replyMarkup: Keyboards.MainMenu);
                }
                await botClient.AnswerCallbackQueryAsync(query.Id);
            }
            else
            {
                UserStateManager.UserCurrencies[query.Message!.Chat.Id] = query.Data;
                await botClient.AnswerCallbackQueryAsync(query.Id);
                await botClient.SendTextMessageAsync(query.Message.Chat.Id, "Введите дату, чтобы узнать курс (формат: DD.MM.YYYY).",
                    replyMarkup: Keyboards.DateMenu);
            }
        }
    }

    public static class Keyboards
    {
        public static InlineKeyboardMarkup MainMenu => new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("Ставка ЦБ РФ", "cb_rate"),
            InlineKeyboardButton.WithCallbackData("Курс валюты", "exchange_rate")
        });

        public static InlineKeyboardMarkup CurrencyMenu => new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("USD", "USD"),
                InlineKeyboardButton.WithCallbackData("EUR", "EUR"),
                InlineKeyboardButton.WithCallbackData("GBP", "GBP")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("CNY", "CNY"),
                InlineKeyboardButton.WithCallbackData("CAD", "CAD"),
                InlineKeyboardButton.WithCallbackData("JPY", "JPY")
            }
        });

        public static InlineKeyboardMarkup DateMenu => new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("Сегодня", "Сегодня"),
            InlineKeyboardButton.WithCallbackData("Ввести дату", "enter_date")
        });
    }

    public class UserStateManager
    {
        public static readonly Dictionary<long, string> UserCurrencies = new();
        public static readonly HashSet<long> UserRequestsKeyRate = new();
    }
}


