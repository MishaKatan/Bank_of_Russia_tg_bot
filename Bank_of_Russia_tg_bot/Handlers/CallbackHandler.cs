using Bank_of_Russia_tg_bot.Keyboards;
using Bank_of_Russia_tg_bot.StateManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace Bank_of_Russia_tg_bot.Handlers
{
    public class CallbackHandler
    {
        private readonly IUserStateManager _stateManager;
        private readonly KeyboardBuilder _keyboardBuilder;

        public CallbackHandler(
            IUserStateManager stateManager,
            KeyboardBuilder keyboardBuilder)
        {
            _stateManager = stateManager;
            _keyboardBuilder = keyboardBuilder;
        }

        public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery query)
        {
            try
            {
                await botClient.AnswerCallbackQueryAsync(query.Id);

                switch (query.Data)
                {
                    case "cb_rate":
                        HandleKeyRateCommand(query.Message.Chat.Id);
                        await SendDateMenu(botClient, query.Message.Chat.Id);
                        break;

                    case "exchange_rate":
                        await SendCurrencyMenu(botClient, query.Message.Chat.Id);
                        break;

                    case "today":
                        await ProcessTodayRequest(botClient, query.Message.Chat.Id);
                        break;

                    case "enter_date":
                        await RequestDateInput(botClient, query.Message.Chat.Id);
                        break;

                    default:
                        if (IsCurrencyCode(query.Data))
                        {
                            HandleCurrencySelection(query.Message.Chat.Id, query.Data);
                            await SendDateMenu(botClient, query.Message.Chat.Id);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Callback error: {ex}");
                await botClient.SendMessage(
                    query.Message.Chat.Id,
                    "Произошла ошибка при обработке запроса");
            }
        }

        private void HandleKeyRateCommand(long chatId)
        {
            _stateManager.SetCommandState(chatId, UserCommand.KeyRate);
        }

        private void HandleCurrencySelection(long chatId, string currency)
        {
            _stateManager.SetCommandState(chatId, UserCommand.ExchangeRate, currency);
        }

        private async Task SendCurrencyMenu(ITelegramBotClient botClient, long chatId)
        {
            await botClient.SendMessage(
                chatId,
                "Выберите валюту:",
                replyMarkup: _keyboardBuilder.GetCurrencyMenu());
        }

        private async Task SendDateMenu(ITelegramBotClient botClient, long chatId)
        {
            await botClient.SendMessage(
                chatId,
                "Введите дату или выберите опцию:",
                replyMarkup: _keyboardBuilder.GetDateMenu());
        }

        private async Task RequestDateInput(ITelegramBotClient botClient, long chatId)
        {
            await botClient.SendMessage(
                chatId,
                "Введите дату в формате ДД.ММ.ГГГГ");
        }

        private async Task ProcessTodayRequest(ITelegramBotClient botClient, long chatId)
        {
            if (_stateManager.TryGetUserState(chatId, out var state))
            {
                var today = DateTime.Today;
                switch (state.CurrentCommand)
                {
                    case UserCommand.KeyRate:
                        await botClient.SendMessage(
                            chatId,
                            $"Запрашиваю ключевую ставку на {today:dd.MM.yyyy}...");
                        break;

                    case UserCommand.ExchangeRate when !string.IsNullOrEmpty(state.SelectedCurrency):
                        await botClient.SendMessage(
                            chatId,
                            $"Запрашиваю курс {state.SelectedCurrency} на {today:dd.MM.yyyy}...");
                        break;
                }
                _stateManager.ClearState(chatId);
            }
        }

        private static bool IsCurrencyCode(string data)
            => data.Length == 3 && data.All(char.IsUpper);
    }
}
