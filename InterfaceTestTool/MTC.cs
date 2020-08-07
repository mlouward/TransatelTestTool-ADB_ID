namespace InterfaceTestTool
{
    public class MTC : ITest
    {
        public int Nb { get; set; }
        public int Duration { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public int Delay { get; set; }

        public MTC(int nb, int duration, int from, int to, int delay)
        {
            Nb = nb;
            Duration = duration;
            From = from;
            To = to;
            Delay = delay;
        }

        public string WriteCsv()
        {
            return $"mtc;{Nb};{Duration};{From};{To};{Delay}";
        }

        public override string ToString()
        {
            return $"MTC: {Nb} tests, Duration: {Duration}sec., Phone A: {From}, Phone B: {To}, Delay: {Delay}sec.";
        }
    }
}