using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramCurrencyBot
{
    public static class KeyRateService
    {
        public static async Task SendKeyRate(ITelegramBotClient botClient, long chatId, DateTime requestedDate)
        {
            string url = "https://www.cbr.ru/DailyInfoWebServ/DailyInfo.asmx";
            string day = requestedDate.ToString("yyyy-MM-dd");

            var headers = new Dictionary<string, string>
        {
            { "Content-Type", "text/xml" }
        };

            string body = $@"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
                                    xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
                                    xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                            <soap:Body>
                                <KeyRateXML xmlns=""http://web.cbr.ru/"">
                                    <fromDate>{day}</fromDate>
                                    <ToDate>{day}</ToDate>
                                </KeyRateXML>
                            </soap:Body>
                        </soap:Envelope>";

            using var client = new HttpClient();
            try
            {
                var requestContent = new StringContent(body, Encoding.UTF8, "text/xml");
                var response = await client.PostAsync(url, requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    string rate = responseContent.Split("<Rate>")[1].Split("</Rate>")[0];
                    await botClient.SendTextMessageAsync(chatId, $"Ключевая ставка на {requestedDate:dd.MM.yyyy}: {rate}%",
                                                         replyMarkup: Keyboards.MainMenu);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Не удалось получить ключевую ставку. Попробуйте позже.",
                                                         replyMarkup: Keyboards.MainMenu);
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(chatId, $"Произошла ошибка: {ex.Message}",
                                                     replyMarkup: Keyboards.MainMenu);
            }
        }
    }

}
