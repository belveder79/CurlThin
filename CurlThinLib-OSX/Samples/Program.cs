using System;
using CurlThin.Samples.Multi;

namespace Samples
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            HyperSample sample = new HyperSample();
            sample.Run();
        }
    }
}
