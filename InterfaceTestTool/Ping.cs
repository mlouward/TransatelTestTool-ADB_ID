using System;

namespace InterfaceTestTool
{
    [Serializable]
    internal class Ping : ITest
    {
        public int From { get; set; }
        public int Delay { get; set; }
        public string Address { get; set; }
        public int Nb { get; set; }
        public int Size { get; set; }

        public Ping(int from, int delay, string address, int nb, int size)
        {
            From = from;
            Delay = delay;
            Address = address;
            Nb = nb;
            Size = size;
        }

        public string WriteCsv()
        {
            return $"ping;{From};{Delay};{Address};{Nb};{Size}";
        }

        public override string ToString()
        {
            return $"Ping: {Nb} packets, {Size} bytes, Phone A: {From}, IP: {Address}, Delay: {Delay}sec.";
        }
    }
}