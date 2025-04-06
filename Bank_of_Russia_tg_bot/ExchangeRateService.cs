using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;

namespace TelegramCurrencyBot
{
    public static class ExchangeRateService
    {
        public static async Task SendExchangeRate(ITelegramBotClient botClient, long chatId, string currency, DateTime requestedDate)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string date = requestedDate.ToString("dd/MM/yyyy");
            string url = $"https://www.cbr.ru/scripts/XML_daily.asp?date_req={date}";

            using var client = new HttpClient();

            try
            {
                var response = await client.GetAsync(url);
                using var responseStream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(responseStream, Encoding.GetEncoding("windows-1251"));
                string responseString = await reader.ReadToEndAsync();

                var xml = XDocument.Parse(responseString);

                var currencyElement = xml.Descendants("Valute")
                    .FirstOrDefault(x => (string)x.Element("CharCode") == currency);

                if (currencyElement != null)
                {
                    string name = currencyElement.Element("Name")?.Value ?? "Неизвестно";
                    string value = currencyElement.Element("VunitRate")?.Value ?? "Неизвестно";
                    await botClient.SendTextMessageAsync(chatId, $"Валюта: {name}\nДата: {requestedDate:dd.MM.yyyy}\nКурс: {value}");
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Не удалось найти данные для указанной валюты.", replyMarkup: Keyboards.MainMenu);
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(chatId, $"Произошла ошибка: {ex.Message}", replyMarkup: Keyboards.MainMenu);
            }
        }
    }

}
