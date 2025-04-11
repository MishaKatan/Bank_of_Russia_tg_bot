// Telegram Bot для просмотра курса валют

using Bank_of_Russia_tg_bot.Handlers;
using Bank_of_Russia_tg_bot.Keyboards;
using Bank_of_Russia_tg_bot.Services;
using Bank_of_Russia_tg_bot.StateManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
using static Telegram.Bot.TelegramBotClient;

namespace TelegramCurrencyBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(
                    Environment.GetEnvironmentVariable("BOT_TOKEN")
                    ?? throw new InvalidOperationException("BOT_TOKEN environment variable is not set")));
                    services.AddSingleton<IUserStateManager, UserStateManager>();
                    services.AddScoped<ExchangeRateService>();
                    services.AddScoped<KeyRateService>();
                    services.AddScoped<KeyboardBuilder>();
                    services.AddScoped<CallbackHandler>();
                    services.AddScoped<MessageHandler>();
                    services.AddScoped<UpdateHandler>();
                    services.AddHttpClient();
                    services.AddHostedService<BotBackgroundService>();
                })
                .Build();

            await host.RunAsync();
        }
    }

    public class BotBackgroundService : BackgroundService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly UpdateHandler _updateHandler;
        public BotBackgroundService(
            ITelegramBotClient botClient,
            UpdateHandler updateHandler)
        {
            _botClient = botClient;
            _updateHandler = updateHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var me = await _botClient.GetMe(stoppingToken);
            Console.WriteLine($"Bot @{me.Username} started");

            _botClient.StartReceiving(
                _updateHandler.HandleUpdateAsync,
                _updateHandler.HandleErrorAsync,
                receiverOptions: null,
                cancellationToken: stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}


