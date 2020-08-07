namespace InterfaceTestTool
{
    public class SMS : ITest
    {
        public int Nb { get; set; }
        public string Text { get; set; }
        public int From { get; set; }
        public string To { get; set; }
        public int Delay { get; set; }

        public SMS(int nb, string text, int from, string to, int delay)
        {
            Nb = nb;
            Text = text;
            From = from;
            To = to;
            Delay = delay;
        }

        public string WriteCsv()
        {
            return $"SMS;{Nb};{Text};{From};{To};{Delay}";
        }

        public override string ToString()
        {
            return $"SMS: {Nb} tests, Text: {Text}, Phone A: {From}, Phone B: {To}, Delay: {Delay}sec.";
        }
    }
}