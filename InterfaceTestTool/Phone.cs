using System;

namespace InterfaceTestTool
{
    public class Phone : IComparable
    {
        public int Index { get; set; }
        public string PhoneNumber { get; set; }
        public string IMSI { get; set; }
        public bool IsRooted { get; set; }
        public string Version { get; set; }
        public string Model { get; set; }

        public Phone(int index, string phoneNumber, string iMSI)
        {
            Index = index;
            PhoneNumber = phoneNumber;
            IMSI = iMSI;
        }

        public Phone(string phoneNumber, string iMSI)
        {
            PhoneNumber = phoneNumber;
            IMSI = iMSI;
        }

        public override string ToString()
        {
            // We don't display information if unavailable. (i.e. device unplugged.)
            string r = IsRooted ? " (rooted)" : "";
            string v = Version == "" ? "" : $", Android {Version}";
            string m = Model == "" ? "" : $", Model: {Model}";
            return $"{Index}: n°: {PhoneNumber}, IMSI: {IMSI}{m}{v}{r}";
        }
        public string ErrorString()
        {
            string m = Model == "" ? "" : $" ({Model}, Android {Version})";
            return $"n°: {PhoneNumber}, IMSI: {IMSI}{m}";
        }
        internal string WriteCsv()
        {
            return $"{Index};{PhoneNumber};{IMSI}";
        }

        public int CompareTo(object obj)
        {
            Phone other = (Phone)obj;
            return Index.CompareTo(other.Index);
        }
    }
}