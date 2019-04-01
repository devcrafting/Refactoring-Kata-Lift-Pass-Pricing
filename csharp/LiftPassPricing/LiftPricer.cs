using System;

public class LiftPricer
{
    private readonly string type;
    private DateTime? date;
    private bool isHolidays;
    private double basePrice;

    public LiftPricer(string type, DateTime? date, bool isHolidays, double basePrice)
    {
        this.type = type;
        this.date = date;
        this.isHolidays = isHolidays;
        this.basePrice = basePrice;
    }

    public string GetPrice(int? age)
    {
        if (age != null && age < 6)
        {
            return "{ \"cost\": 0}";
        }
        else
        {
            var reduction = 0;

            if (!"night".Equals(type))
            {
                if (date.HasValue && !isHolidays && (int)date.Value.DayOfWeek == 1)
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
}