using System;

namespace InterfaceTestTool
{
    [Serializable]
    public class SMS : ITest
    {
        public int Nb { get; set; }
        public string Text { get; set; }
        public int From { get; set; }
        public string To { get; set; }
        public int Delay { get; set; }
        public string Prefix { get; set; }

        public SMS(int nb, string text, int from, string to, int delay, string prefix)
        {
            Nb = nb;
            Text = text;
            From = from;
            To = to;
            Delay = delay;
            Prefix = prefix;
        }

        public string WriteCsv()
        {
            return $"SMS;{Nb};{Text};{From};{To};{Delay};{Prefix}";
        }

        public sealed override string ToString()
        {
            var p = MainWindow.prefixToType.TryGetValue(Prefix, out string prf) ? prf : Prefix;
            return $"SMS: {Nb} tests, Text: {Text}, Phone A: {From}, Phone B: {To} ({p}), Delay: {Delay}sec.";
        }
    }
}