using System;

namespace Us.Cbp.Extensions;

public static class RandomExtensions
{
    public static bool NextBool(this Random random, double probabilityOfTrue = 0.5)
    {
        return random.NextDouble() < probabilityOfTrue;
    }
}