using Bank_of_Russia_tg_bot.Keyboards;
using Bank_of_Russia_tg_bot.Services;
using Bank_of_Russia_tg_bot.StateManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramCurrencyBot;

namespace Bank_of_Russia_tg_bot.Handlers
{
    public class MessageHandler
    {
        private readonly IUserStateManager _stateManager;
        private readonly ExchangeRateService _exchangeService;
        private readonly KeyRateService _keyRateService;
        private readonly KeyboardBuilder _keyboardBuilder;
        public MessageHandler(
            IUserStateManager stateManager,
            ExchangeRateService exchangeService,
            KeyRateService keyRateService,
            KeyboardBuilder keyboardBuilder)
        {
            _stateManager = stateManager;
            _exchangeService = exchangeService;
            _keyRateService = keyRateService;
            _keyboardBuilder = keyboardBuilder;
        }

        public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "/start")
            {
                await ShowMainMenu(botClient, message.Chat.Id);
                return;
            }

            if (DateTime.TryParse(message.Text, out var requestedDate))
            {
                await ProcessDateInput(botClient, message.Chat.Id, requestedDate);
                return;
            }

            await botClient.SendMessage(
                message.Chat.Id,
                "Извините, я не понимаю этот запрос.",
                replyMarkup: _keyboardBuilder.GetMainMenu());
        }

        private async Task ShowMainMenu(ITelegramBotClient botClient, long chatId)
        {
            await botClient.SendMessage(
                chatId,
                "Привет! Что вы хотите узнать?",
                replyMarkup: _keyboardBuilder.GetMainMenu());
        }

        private async Task ProcessDateInput(ITelegramBotClient botClient, long chatId, DateTime date)
        {
            if (_stateManager.TryGetUserState(chatId, out var state))
            {
                switch (state.CurrentCommand)
                {
                    case UserCommand.KeyRate:
                        await _keyRateService.SendKeyRateAsync(botClient, chatId, date);
                        break;
                    case UserCommand.ExchangeRate when !string.IsNullOrEmpty(state.SelectedCurrency):
                        await _exchangeService.SendExchangeRateAsync(botClient, chatId, state.SelectedCurrency, date);
                        break;
                    default:
                        await botClient.SendMessage(chatId, "Сначала выберите валюту.");
                        break;
                }
                _stateManager.ClearState(chatId);
            }
            else
            {
                await botClient.SendMessage(chatId, "Сначала выберите команду.");
            }
        }
    }
}
