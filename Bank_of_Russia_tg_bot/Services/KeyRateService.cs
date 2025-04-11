using Bank_of_Russia_tg_bot.Keyboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Telegram.Bot;
using TelegramCurrencyBot;

namespace Bank_of_Russia_tg_bot.Services
{
    public class KeyRateService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public KeyRateService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendKeyRateAsync(ITelegramBotClient botClient, long chatId, DateTime date)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CbrSoap");
                var response = await GetKeyRateAsync(client, date);

                if (response?.KeyRateXMLResult?.Any() == true)
                {
                    var rate = response.KeyRateXMLResult.First().Rate;
                    await botClient.SendTextMessageAsync(
                        chatId,
                        $"Ключевая ставка на {date:dd.MM.yyyy}: {rate}%",
                        replyMarkup: new KeyboardBuilder().GetMainMenu());
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Данные не найдены");
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(chatId, $"Ошибка: {ex.Message}");
            }
        }

        private static async Task<KeyRateXMLResponse> GetKeyRateAsync(HttpClient client, DateTime date)
        {
            var request = new KeyRateXMLRequest(
                new KeyRateXMLRequestBody(date, date));

            var response = await client.PostAsync(
                "DailyInfoWebServ/DailyInfo.asmx",
                new StringContent(request.ToXml(), Encoding.UTF8, "text/xml"));

            return await DeserializeResponse<KeyRateXMLResponse>(await response.Content.ReadAsStreamAsync());
        }

        private static async Task<T> DeserializeResponse<T>(Stream stream)
        {
            var serializer = new DataContractSerializer(typeof(T));
            using var reader = XmlReader.Create(stream);
            return (T)serializer.ReadObject(reader);
        }
    }

    // SOAP DTOs
    [DataContract]
    public class KeyRateXMLRequest
    {
        [DataMember] public KeyRateXMLRequestBody Body { get; set; }

        public KeyRateXMLRequest(KeyRateXMLRequestBody body)
        {
            Body = body;
        }

        public string ToXml()
        {
            var serializer = new DataContractSerializer(GetType());
            using var writer = new StringWriter();
            using var xmlWriter = XmlWriter.Create(writer);
            serializer.WriteObject(xmlWriter, this);
            return writer.ToString();
        }
    }

    [DataContract(Namespace = "http://web.cbr.ru/")]
    public class KeyRateXMLRequestBody
    {
        [DataMember] public DateTime fromDate { get; set; }
        [DataMember] public DateTime ToDate { get; set; }

        public KeyRateXMLRequestBody(DateTime fromDate, DateTime toDate)
        {
            this.fromDate = fromDate;
            ToDate = toDate;
        }
    }

    [DataContract]
    public class KeyRateXMLResponse
    {
        [DataMember] public List<KeyRateInfo> KeyRateXMLResult { get; set; }
    }

    [DataContract]
    public class KeyRateInfo
    {
        [DataMember] public DateTime D0 { get; set; }
        [DataMember] public double Rate { get; set; }
    }
}
