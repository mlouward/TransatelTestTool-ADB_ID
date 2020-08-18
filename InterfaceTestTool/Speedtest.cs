using System;

namespace InterfaceTestTool
{
    [Serializable]
    public class Speedtest : ITest
    {
        public int From { get; set; }

        // Size must be one of 10, 20, 50 or 100 (MB).
        public int Size { get; set; }

        public int Delay { get; set; }

        public Speedtest(int from, int size, int delay)
        {
            From = from;
            Size = size;
            Delay = delay;
        }

        public string WriteCsv()
        {
            return $"speedtest;{From};{Size};{Delay}";
        }

        public sealed override string ToString()
        {
            return $"Speedtest: Phone A: {From}, Size: {Size}MB, Delay: {Delay}sec.";
        }
    }
}