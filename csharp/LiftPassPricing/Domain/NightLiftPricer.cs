using System;

internal class NightLiftPricer : IPriceLift
{
    private DateTime? date;
    private int basePrice;

    public NightLiftPricer(DateTime? date, int basePrice)
    {
        this.date = date;
        this.basePrice = basePrice;
    }

    public int GetPrice(int? age)
    {
        if (age == null || age < 6)
        {
            return 0;
        }

        if (age > 64)
        {
            return (int)Math.Ceiling(basePrice * .4);
        }

        return basePrice;
    }
}