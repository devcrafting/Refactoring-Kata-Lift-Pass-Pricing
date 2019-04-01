﻿using System;
using System.Globalization;
using Nancy;
using MySql.Data.MySqlClient;

namespace LiftPassPricing
{
    public class Prices : NancyModule
    {

        public readonly MySqlConnection connection;

        public Prices()
        {
            this.connection = new MySqlConnection
            {
                ConnectionString = @"Database=lift_pass;Data Source=localhost;User Id=root;Password=mysql"
            };
            connection.Open();

            base.Put("/prices", _ =>
            {
                int liftPassCost = Int32.Parse(this.Request.Query["cost"]);
                string liftPassType = this.Request.Query["type"];
                return AddPrice(liftPassType, liftPassCost);
            });

            base.Get("/prices", _ =>
            {
                int? age = this.Request.Query["age"] != null ? Int32.Parse(this.Request.Query["age"]) : null;
                var type = this.Request.Query["type"];
                var date = this.Request.Query["date"];
                return GetPrice(age, type, date);
            });

            After += ctx =>
            {
                ctx.Response.ContentType = "application/json";
            };

        }

        private string GetPrice(int? age, object type, dynamic date)
        {
            using (var costCmd = new MySqlCommand( //
                "SELECT cost FROM base_price " + //
                "WHERE type = @type", connection))
            {
                costCmd.Parameters.AddWithValue("@type", type);
                costCmd.Prepare();
                double result = (int)costCmd.ExecuteScalar();

                int reduction;
                var isHoliday = false;

                if (age != null && age < 6)
                {
                    return "{ \"cost\": 0}";
                }
                else
                {
                    reduction = 0;

                    if (!"night".Equals(type))
                    {
                        using (var holidayCmd = new MySqlCommand( //
                            "SELECT * FROM holidays", connection))
                        {
                            holidayCmd.Prepare();
                            using (var holidays = holidayCmd.ExecuteReader())
                            {

                                while (holidays.Read())
                                {
                                    var holiday = holidays.GetDateTime("holiday");
                                    if (date != null)
                                    {
                                        DateTime d = System.DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                        if (d.Year == holiday.Year &&
                                            d.Month == holiday.Month &&
                                            d.Date == holiday.Date)
                                        {
                                            isHoliday = true;
                                        }
                                    }
                                }

                            }
                        }

                        if (date != null)
                        {
                            DateTime d = System.DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            if (!isHoliday && (int)d.DayOfWeek == 1)
                            {
                                reduction = 35;
                            }
                        }

                        // TODO apply reduction for others
                        if (age != null && age < 15)
                        {
                            return "{ \"cost\": " + (int)Math.Ceiling(result * .7) + "}";
                        }
                        else
                        {
                            if (age == null)
                            {
                                double cost = result * (1 - reduction / 100.0);
                                return "{ \"cost\": " + (int)Math.Ceiling(cost) + "}";
                            }
                            else
                            {
                                if (age > 64)
                                {
                                    double cost = result * .75 * (1 - reduction / 100.0);
                                    return "{ \"cost\": " + (int)Math.Ceiling(cost) + "}";
                                }
                                else
                                {
                                    double cost = result * (1 - reduction / 100.0);
                                    return "{ \"cost\": " + (int)Math.Ceiling(cost) + "}";
                                }
                            }
                        }
                    }
                    else
                    {
                        if (age != null && age >= 6)
                        {
                            if (age > 64)
                            {
                                return "{ \"cost\": " + (int)Math.Ceiling(result * .4) + "}";
                            }
                            else
                            {
                                return "{ \"cost\": " + result + "}";
                            }
                        }
                        else
                        {
                            return "{ \"cost\": 0}";
                        }
                    }
                }
            }
        }

        private string AddPrice(string liftPassType, int liftPassCost)
        {
            using (var command = new MySqlCommand( //
                   "INSERT INTO base_price (type, cost) VALUES (@type, @cost) " + //
                   "ON DUPLICATE KEY UPDATE cost = @cost;", connection))
            {
                command.Parameters.AddWithValue("@type", liftPassType);
                command.Parameters.AddWithValue("@cost", liftPassCost);
                command.Prepare();
                command.ExecuteNonQuery();
            }

            return "";
        }
    }
}
