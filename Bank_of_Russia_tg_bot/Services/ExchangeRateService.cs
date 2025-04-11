using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using TelegramCurrencyBot;

namespace Bank_of_Russia_tg_bot.Services
{
    public class ExchangeRateService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ExchangeRateService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendExchangeRateAsync(ITelegramBotClient botClient, long chatId, string currency, DateTime date)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://www.cbr.ru/scripts/XML_daily.asp?date_req={date:dd/MM/yyyy}");

                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream, Encoding.GetEncoding("windows-1251"));
                var xml = XDocument.Parse(await reader.ReadToEndAsync());

                var currencyData = xml.Descendants("Valute")
                    .FirstOrDefault(x => x.Element("CharCode")?.Value == currency);

                if (currencyData != null)
                {
                    var message = $"Валюта: {currencyData.Element("Name")?.Value}\n" +
                                 $"Дата: {date:dd.MM.yyyy}\n" +
                                 $"Курс: {currencyData.Element("VunitRate")?.Value}";

                    await botClient.SendMessage(chatId, message);
                }
                else
                {
                    await botClient.SendMessage(chatId, "Данные по валюте не найдены.");
                }
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(chatId, $"Ошибка: {ex.Message}");
            }
        }
    }
}
