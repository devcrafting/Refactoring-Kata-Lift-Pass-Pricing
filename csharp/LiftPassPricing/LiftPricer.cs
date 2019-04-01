using System;

public class LiftPricer : IPriceLift
{
    private DateTime? date;
    private bool isHolidays;
    private int basePrice;

    public LiftPricer(DateTime? date, bool isHolidays, int basePrice)
    {
        this.date = date;
        this.isHolidays = isHolidays;
        this.basePrice = basePrice;
    }

    public int GetPrice(int? age)
    {
        if (age != null && age < 6)
        {
            return 0;
        }
        var reduction = 0;

        if (date.HasValue && !isHolidays && (int)date.Value.DayOfWeek == 1)
        {
            reduction = 35;
        }

        if (age != null && age < 15)
        {
            return (int)Math.Ceiling(basePrice * .7);
        }

        if (age == null || age <= 64)
        {
            var cost1 = basePrice * (1 - reduction / 100.0);
            return (int)Math.Ceiling(cost1);
        }

        var cost = basePrice * .75 * (1 - reduction / 100.0);
        return (int)Math.Ceiling(cost);
    }
}