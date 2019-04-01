using System;
using System.Globalization;
using Nancy;
using MySql.Data.MySqlClient;

namespace LiftPassPricing
{
    public class Prices : NancyModule
    {

        public Prices(IStoreLiftPricer liftPricerRepository)
        {
            base.Put("/prices", _ =>
            {
                int liftPassCost = Int32.Parse(this.Request.Query["cost"]);
                string liftPassType = this.Request.Query["type"];
                liftPricerRepository.Add(liftPassType, liftPassCost);
                return "Done";
            });

            base.Get("/prices", _ =>
            {
                int? age = this.Request.Query["age"] != null ? Int32.Parse(this.Request.Query["age"]) : null;
                var type = this.Request.Query["type"];
                var tryParseDate = DateTime.TryParseExact(this.Request.Query["date"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);
                var liftPricer = liftPricerRepository.Get(type, tryParseDate ? new DateTime?(date) : null);
                return $"{{ \"cost\": {liftPricer.GetPrice(age)}}}";
            });

            After += ctx =>
            {
                ctx.Response.ContentType = "application/json";
            };

        }
    }
}
