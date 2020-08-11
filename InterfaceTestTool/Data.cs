namespace InterfaceTestTool
{
    public class Data : ITest
    {
        public int Nb { get; set; }
        public string URL { get; set; }
        public int From { get; set; }
        public int Delay { get; set; }

        public Data(int nb, string url, int from, int delay)
        {
            Nb = nb;
            URL = url;
            From = from;
            Delay = delay;
        }

        public string WriteCsv()
        {
            return $"data;{Nb};{URL};{From};{Delay}";
        }

        public sealed override string ToString()
        {
            return $"Data: {Nb} tests, URL: {URL}, Phone A: {From}, Delay: {Delay}sec.";
        }
    }
}