using System;
using Nalix.Common.Networking.Protocols;

class Program
{
    static void Main()
    {
        Console.WriteLine("--- ProtocolReason ---");
        foreach (var name in Enum.GetNames(typeof(ProtocolReason)))
        {
            Console.WriteLine(name);
        }
        Console.WriteLine("\n--- ProtocolAdvice ---");
        foreach (var name in Enum.GetNames(typeof(ProtocolAdvice)))
        {
            Console.WriteLine(name);
        }
    }
}