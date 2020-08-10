namespace InterfaceTestTool
{
    internal class ChangeApn : ITest
    {
        public int From { get; set; }
        public APN Apn { get; set; }
        public int Delay { get; set; }

        public ChangeApn(int from, APN apn, int delay)
        {
            From = from;
            Apn = apn;
            Delay = delay;
        }

        public string WriteCsv()
        {
            return $"changeapn;{From};{Apn.Id};{Apn.Name};{Apn.Apn};{Delay}";
        }
        public override string ToString()
        {
            return $"Set {Apn.Apn} ({Apn.Id}) as default APN for {From}, Delay: {Delay}";
        }
    }
}