using System;
using MySql.Data.MySqlClient;

public class LiftPricerRepository : ILoadLiftPricer
{
    private readonly MySqlConnection connection;

    public LiftPricerRepository(MySqlConnection connection)
    {
        this.connection = connection;
    }

    public IPriceLift Get(object type, DateTime? date)
    {
        if (type.ToString() == "night")
            return new NightLiftPricer(date, GetBasePrice(type));
        return new LiftPricer(date, IsHolidays(date), GetBasePrice(type));
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
}