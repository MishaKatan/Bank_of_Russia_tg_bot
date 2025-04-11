using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bank_of_Russia_tg_bot.Keyboards
{
    public class KeyboardBuilder
    {
        public InlineKeyboardMarkup GetMainMenu()
        {
            return new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Ставка ЦБ РФ", "cb_rate"),
                InlineKeyboardButton.WithCallbackData("Курс валюты", "exchange_rate")
            }
        });
        }

        public InlineKeyboardMarkup GetCurrencyMenu()
        {
            return new InlineKeyboardMarkup(new[]
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
        }

        public InlineKeyboardMarkup GetDateMenu()
        {
            return new InlineKeyboardMarkup(new[]
            {
            InlineKeyboardButton.WithCallbackData("Сегодня", "today"),
            InlineKeyboardButton.WithCallbackData("Ввести дату", "enter_date")
        });
        }
    }
}
