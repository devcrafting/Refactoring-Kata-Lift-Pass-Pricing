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

    public string GetPrice(int? age, object type, DateTime? date)
    {
        double basePrice = GetBasePrice(type);

        if (age != null && age < 6)
        {
            return "{ \"cost\": 0}";
        }
        else
        {
            var reduction = 0;

            if (!"night".Equals(type))
            {
                var isHoliday = IsHolidays(date);

                if (date.HasValue && !isHoliday && (int)date.Value.DayOfWeek == 1)
                {
                    reduction = 35;
                }

                // TODO apply reduction for others
                if (age != null && age < 15)
                {
                    return "{ \"cost\": " + (int)Math.Ceiling(basePrice * .7) + "}";
                }
                else
                {
                    if (age == null || age <= 64)
                    {
                        double cost = basePrice * (1 - reduction / 100.0);
                        return "{ \"cost\": " + (int)Math.Ceiling(cost) + "}";
                    }
                    else
                    {
                        double cost = basePrice * .75 * (1 - reduction / 100.0);
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
                        return "{ \"cost\": " + (int)Math.Ceiling(basePrice * .4) + "}";
                    }
                    else
                    {
                        return "{ \"cost\": " + basePrice + "}";
                    }
                }
                else
                {
                    return "{ \"cost\": 0}";
                }
            }
        }
    }

    private bool IsHolidays(DateTime? date)
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
                    if (date.HasValue && date.Value.Equals(holiday))
                    {
                        return true;
                    }
                }

            }
        }

        return false;
    }

    private double GetBasePrice(object type)
    {
        double basePrice;
        using (var costCmd = new MySqlCommand( //
    "SELECT cost FROM base_price " + //
    "WHERE type = @type", connection))
        {
            costCmd.Parameters.AddWithValue("@type", type);
            costCmd.Prepare();
            basePrice = (int)costCmd.ExecuteScalar();
        }

        return basePrice;
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