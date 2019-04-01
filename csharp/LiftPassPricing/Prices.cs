using System;
using System.Globalization;
using Nancy;
using MySql.Data.MySqlClient;

namespace LiftPassPricing
{
    public class Prices : NancyModule
    {

        public readonly MySqlConnection connection;

        public Prices(LiftServices liftServices)
        {
            base.Put("/prices", _ =>
            {
                int liftPassCost = Int32.Parse(this.Request.Query["cost"]);
                string liftPassType = this.Request.Query["type"];
                return liftServices.AddPrice(liftPassType, liftPassCost);
            });

            base.Get("/prices", _ =>
            {
                int? age = this.Request.Query["age"] != null ? Int32.Parse(this.Request.Query["age"]) : null;
                var type = this.Request.Query["type"];
                var date = this.Request.Query["date"];
                return liftServices.GetPrice(age, type, date);
            });

            After += ctx =>
            {
                ctx.Response.ContentType = "application/json";
            };

        }
    }
}
