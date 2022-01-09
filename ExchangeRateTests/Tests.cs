using CurrencyExchange.Controllers;
using CurrencyExchange.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace ExchangeRateTests
{
    [TestClass]
    public class Tests
    {
        private static readonly HttpClient _client = new HttpClient();

        [TestMethod]
        public void GetRate_ShouldReturnOneObject()
        {
            var requestProcesser = new RequestProcessing();

            var date = System.DateTime.Parse("2021-01-11");

            var exchangeRate = new ExchangeRate("USD", "EUR", date, 1.2163);

            var currPair = new KeyValuePair<string, string>("USD", "EUR");

            var dates = new List<DateTime>();
            dates.Add(date);

            var requestResult = new List<ExchangeRate>();

                var body = requestProcesser.prepareMsgBody(dates, currPair);

                var request = new HttpRequestMessage(HttpMethod.Get, body);

                var response = _client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    var elemList = requestProcesser.getXMLElements(response);

                    requestResult = requestProcesser.getListOfRates(elemList, dates, currPair);
                }


            Assert.AreEqual(exchangeRate.originCurrency, requestResult.First().originCurrency);
            Assert.AreEqual(exchangeRate.targetCurrency, requestResult.First().targetCurrency);
            Assert.AreEqual(exchangeRate.date, requestResult.First().date);
            Assert.AreEqual(exchangeRate.rate, requestResult.First().rate);
        }
    }
}