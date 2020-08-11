namespace InterfaceTestTool
{
    public class MOC : ITest
    {
        public int Nb { get; set; }
        public int Duration { get; set; }
        public int From { get; set; }
        public string To { get; set; }
        public int Delay { get; set; }

        public MOC(int nb, int duration, int from, string to, int delay)
        {
            Nb = nb;
            Duration = duration; 
            From = from;
            To = to;
            Delay = delay;
        }

        public string WriteCsv()
        {
            return $"moc;{Nb};{Duration};{From};{To};{Delay}";
        }

        public sealed override string ToString()
        {
            return $"MOC: {Nb} tests, Duration: {Duration}sec., Phone A: {From}, Phone B: {To}, Delay: {Delay}sec.";
        }
    }
}