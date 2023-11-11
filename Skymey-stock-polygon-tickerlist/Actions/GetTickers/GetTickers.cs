using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Nancy.Json;
using RestSharp;
using Skymey_main_lib.Models.Prices.Polygon;
using Skymey_main_lib.Models.Tickers.Polygon;
using Skymey_stock_polygon_tickerlist.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Skymey_stock_polygon_tickerlist.Actions.GetTickers
{
    public class GetTickers 
    {
        private RestClient _client;
        private RestRequest _request;
        private MongoClient _mongoClient;
        private ApplicationContext _db;
        private string _apiKey;
        public GetTickers()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();

            _apiKey = config.GetSection("ApiKeys:Polygon").Value;
            _mongoClient = new MongoClient("mongodb://127.0.0.1:27017");
            _db = ApplicationContext.Create(_mongoClient.GetDatabase("skymey"));
        }
        public string GetTickersFromPolygon(string Ticker)
        {
            _client = new RestClient("https://api.polygon.io/v3/reference/tickers?active=true&limit=1000&ticker.gt="+ Ticker + "&apiKey=" + _apiKey);
            _request = new RestRequest("https://api.polygon.io/v3/reference/tickers?active=true&limit=1000&ticker.gt="+ Ticker + "&apiKey=" + _apiKey, Method.Get);
            string resp = "";
            try
            {
                #region DIA
                _request.AddHeader("Content-Type", "application/json");
                var r = _client.Execute(_request).Content;
                TickerListQuery tp = new JavaScriptSerializer().Deserialize<TickerListQuery>(r);
                if (tp.count > 0)
                {
                    #endregion

                    foreach (var ticker in tp.results)
                    {

                        resp = ticker.ticker;
                        Console.WriteLine(ticker.ticker);
                        var ticker_find = (from i in _db.TickerList where i.ticker == ticker.ticker select i).FirstOrDefault();

                        if (ticker_find == null)
                        {
                            ticker._id = ObjectId.GenerateNewId();
                            ticker.Update = DateTime.UtcNow;
                            _db.TickerList.Add(ticker);
                        }
                        else
                        {
                            ticker_find.active = ticker.active;
                            ticker_find.Update = DateTime.UtcNow;
                            _db.TickerList.Update(ticker_find);
                        }

                    }
                    _db.SaveChanges();
                }
                else
                {
                    resp = "";
                }

            }
            catch (Exception ex)
            {
            }
            return resp;

        }
        public void Dispose()
        {
        }
        ~GetTickers()
        {

        }
    }
}
