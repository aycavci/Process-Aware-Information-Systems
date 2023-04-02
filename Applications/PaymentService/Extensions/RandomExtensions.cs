using System;

namespace PaymentService.Extensions;

public static class RandomExtensions
{
    public static bool NextBool(this Random random, double probabilityOfTrue = 0.5)
    {
        Console.WriteLine("here");
        Console.WriteLine(random.NextDouble() < probabilityOfTrue);
        return random.NextDouble() < probabilityOfTrue;
    }
}