using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Telegram.Bot.TelegramBotClient;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace Bank_of_Russia_tg_bot.Handlers
{
    public class UpdateHandler
    {
        private readonly MessageHandler _messageHandler;
        private readonly CallbackHandler _callbackHandler;

        public UpdateHandler(MessageHandler messageHandler, CallbackHandler callbackHandler)
        {
            _messageHandler = messageHandler;
            _callbackHandler = callbackHandler;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is { } message)
            {
                await _messageHandler.HandleMessageAsync(botClient, message);
            }
            else if (update.CallbackQuery is { } callbackQuery)
            {
                await _callbackHandler.HandleCallbackQueryAsync(botClient, callbackQuery);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }
}
