namespace CurrencyExchange.Models
{
    public class ExchangeRate
    {
        public string originCurrency { get; set; }
        public string targetCurrency { get; set; }
        public DateTime date { get; set; }
        public double rate { get; set; }

        public ExchangeRate(string originCurrency, string targetCurrency, DateTime date, double rate)
        {
            this.originCurrency = originCurrency;
            this.targetCurrency = targetCurrency;
            this.date = date;
            this.rate = rate;
        }
        public ExchangeRate(string originCurrency, string targetCurrency, DateTime date)
        {
            this.originCurrency = originCurrency;
            this.targetCurrency = targetCurrency;
            this.date = date;
        }
    }
}
