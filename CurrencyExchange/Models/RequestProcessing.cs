using System.Xml;

namespace CurrencyExchange.Models
{
    public class RequestProcessing
    {
        public RequestProcessing() { }
        public string prepareMsgBody(List<DateTime> dates, KeyValuePair<string, string> currencyCode)
        {
            var start = dates.First().ToString("yyyy-MM-dd");

            var end = dates.Last().ToString("yyyy-MM-dd");

            var detail = "dataonly";

            var format = "structurespecificdata";

            var body = $"https://sdw-wsrest.ecb.europa.eu/service/data/EXR/D.{currencyCode.Key}.{currencyCode.Value}.SP00.A" +
                $"?startPeriod={start}&endPeriod={end}&detail={detail}&format={format}";

            return body;
        }
        public List<ExchangeRate> getListOfRates(XmlNodeList elemList, List<DateTime> dates, KeyValuePair<string, string> currencyCode)
        {
            var requestResult = new List<ExchangeRate>();

            foreach (XmlNode elem in elemList)
                requestResult.Add(new ExchangeRate(currencyCode.Key, currencyCode.Value, DateTime.Parse(elem.Attributes[0].Value), XmlConvert.ToDouble(elem.Attributes[1].Value)));

            foreach (DateTime date in dates)
                if (!(requestResult.Where(item => item.date.Date == date.Date)).Any())
                    requestResult.Add(new ExchangeRate(currencyCode.Key, currencyCode.Value, date));

            requestResult = requestResult.OrderBy(item => item.date).ToList();

            for (int i = 0; i < requestResult.Count; i++)
                if (requestResult[i].rate == 0)
                    requestResult[i].rate = requestResult[i - 1].rate;

            return requestResult;
        }
        public XmlNodeList getXMLElements(HttpResponseMessage response)
        {
            XmlDocument doc = new XmlDocument();

            doc.LoadXml(response.Content.ReadAsStringAsync().Result);

            XmlNodeList elemList = doc.GetElementsByTagName("Obs");

            return elemList;
        }

        public List<DateTime> checkDates(DateTime startDate, DateTime endDate, List<DateTime> DbDates)
        {
            var listOfDates = new List<DateTime>();

            for (var dt = startDate.AddDays(-7); dt <= endDate; dt = dt.AddDays(1))
            {
                listOfDates.Add(dt);
            }

            return listOfDates.Except(DbDates).ToList();
        }
    }
}
