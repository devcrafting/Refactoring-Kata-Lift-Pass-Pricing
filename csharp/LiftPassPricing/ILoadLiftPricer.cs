using System;

public interface ILoadLiftPricer
{
    IPriceLift Get(object type, DateTime? date);
}