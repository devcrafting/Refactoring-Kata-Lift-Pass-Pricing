using System;

public interface ILoadLiftPricer
{
    LiftPricer Get(object type, DateTime? date);
}