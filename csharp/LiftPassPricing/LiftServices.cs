using System;
using System.Data;
using System.Globalization;
using MySql.Data.MySqlClient;

public class LiftServices
{
    private readonly MySqlConnection connection;
    private readonly ILoadLiftPricer liftPricerLoader;

    public LiftServices(MySqlConnection connection, ILoadLiftPricer liftPricerLoader)
    {
        this.connection = connection;
        this.liftPricerLoader = liftPricerLoader;
    }

    public string GetPrice(int? age, object type, DateTime? date)
    {
        var liftPricer = liftPricerLoader.Get(type, date);
        return $"{{ \"cost\": {liftPricer.GetPrice(age)}}}";
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