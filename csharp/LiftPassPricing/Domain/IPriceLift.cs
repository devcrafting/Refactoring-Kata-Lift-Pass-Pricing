using System.Collections.Generic;

public interface IPriceLift
{
    int GetPrice(int? age);
    int GetPrice(IEnumerable<int> ages);
}