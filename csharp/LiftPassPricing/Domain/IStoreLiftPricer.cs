using System;

public interface IStoreLiftPricer
{
    IPriceLift Get(object type, DateTime? date);

    void Add(string liftPassType, int liftPassCost);
}