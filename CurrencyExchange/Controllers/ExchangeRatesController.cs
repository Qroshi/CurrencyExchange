using CurrencyExchange.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CurrencyExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly IPasswordGenerator _passwordGenerator;

        private readonly IDatabaseConnector _databaseConnector;

        private readonly IHttpClientFactory _clientFactory;

        public ExchangeRatesController(IPasswordGenerator passwordGenerator, IDatabaseConnector databaseConnector, IHttpClientFactory clientFactory)
        {
            _passwordGenerator = passwordGenerator;
            _databaseConnector = databaseConnector;
            _clientFactory = clientFactory;
            _databaseConnector.createTables();
        }

        [Route("")]
        public IActionResult Get(string currencyCodes, DateTime startDate, DateTime endDate, string apiKey)
        {
            _databaseConnector.insertIntoLogs(currencyCodes, startDate, endDate, apiKey);

            var keyCheck = _databaseConnector.checkKey(apiKey);
            if (!keyCheck)
                return Ok("Wrong key. Please generate key.");
            else
            {
                if(startDate.Date <= DateTime.Now && endDate.Date <= DateTime.Now.Date)
                {

                    using (var client = _clientFactory.CreateClient())
                    {
                        var currencyCodesDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(currencyCodes);

                        foreach (var currencyCode in currencyCodesDict)
                        {
                            var cachedRates = _databaseConnector.checkExchangeRates(currencyCode.Key, currencyCode.Value, startDate, endDate);

                            var selectResult = new List<ExchangeRate>();

                            selectResult.AddRange(cachedRates.Item1);

                            var listofDateDb = cachedRates.Item2;

                            var requestProcesser = new RequestProcessing();

                            var dates = requestProcesser.checkDates(startDate, endDate, listofDateDb);

                            if (dates.Any())
                            {
                                var body = requestProcesser.prepareMsgBody(dates, currencyCode);

                                var request = new HttpRequestMessage(HttpMethod.Get, body);

                                var response = client.Send(request);

                                var requestResult = new List<ExchangeRate>();

                                if (response.IsSuccessStatusCode)
                                {
                                    var elemList = requestProcesser.getXMLElements(response);

                                    requestResult = requestProcesser.getListOfRates(elemList, dates, currencyCode);
                                }

                                _databaseConnector.insertIntoExchangeRates(requestResult);

                                requestResult = requestResult.Where(item => item.date.Date >= startDate.Date && item.date.Date <= endDate.Date).ToList();

                                selectResult.AddRange(requestResult);
                            }
                            return Ok(JsonConvert.SerializeObject(selectResult)); ;
                        }
                    }
                }

                return NotFound("At least one date from the future. Please enter correct date.");
            }
        }

        [Route("key")]
        public IActionResult GetKey()
        {
            string key = _passwordGenerator.generateKey();

            _databaseConnector.insertIntoKeys(key);

            return Ok(JsonConvert.SerializeObject(key));
        }
    }
}
