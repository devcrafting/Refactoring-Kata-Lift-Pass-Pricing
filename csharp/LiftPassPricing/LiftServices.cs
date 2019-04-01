using System;
using System.Data;
using System.Globalization;
using MySql.Data.MySqlClient;

public class LiftServices
{
    private readonly MySqlConnection connection;

    public LiftServices(MySqlConnection connection)
    {
        this.connection = connection;
    }

    public string GetPrice(int? age, object type, dynamic date)
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
                        if (age == null || age <= 64)
                        {
                            double cost = result * (1 - reduction / 100.0);
                            return "{ \"cost\": " + (int)Math.Ceiling(cost) + "}";
                        }
                        else
                        {
                            double cost = result * .75 * (1 - reduction / 100.0);
                            return "{ \"cost\": " + (int)Math.Ceiling(cost) + "}";
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

    public string AddPrice(string liftPassType, int liftPassCost)
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