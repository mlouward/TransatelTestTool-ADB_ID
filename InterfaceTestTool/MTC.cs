namespace InterfaceTestTool
{
    public class MTC : ITest
    {
        public int Nb { get; set; }
        public int Duration { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public int Delay { get; set; }
        public string Prefix { get; set; }

        public MTC(int nb, int duration, int from, int to, int delay, string prefix)
        {
            Nb = nb;
            Duration = duration;
            From = from;
            To = to;
            Delay = delay;
            Prefix = prefix;
        }

        public string WriteCsv()
        {
            return $"mtc;{Nb};{Duration};{From};{To};{Delay};{Prefix}";
        }

        public sealed override string ToString()
        {
            var p = MainWindow.prefixToType.TryGetValue(Prefix, out string prf) ? prf : Prefix;
            return $"MTC: {Nb} tests, Duration: {Duration}sec., Phone A: {From}, Phone B: {To} ({p}), Delay: {Delay}sec.";
        }
    }
}