using System;

internal class NightLiftPricer : IPriceLift
{
    private DateTime? date;
    private double basePrice;

    public NightLiftPricer(DateTime? date, double basePrice)
    {
        this.date = date;
        this.basePrice = basePrice;
    }

    public string GetPrice(int? age)
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