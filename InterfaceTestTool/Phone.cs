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
        public string PhoneName { get; set; }

        public Phone(string phoneNumber, string iMSI)
        {
            PhoneNumber = phoneNumber;
            IMSI = iMSI;
            IsRooted = false;
        }

        public Phone(int index, string phoneNumber, string iMSI) : this(phoneNumber, iMSI)
        {
            Index = index;
        }

        public Phone(int index, string phoneNumber, string iMSI, string phoneName) : this(index, phoneNumber, iMSI)
        {
            PhoneName = phoneName;
        }

        public override string ToString()
        {
            // We don't display information if unavailable. (i.e. device unplugged.)
            string r = IsRooted ? " (rooted)" : "";
            string version = string.IsNullOrEmpty(Version) ? "" : $", Android {Version}";
            string model = string.IsNullOrEmpty(Model) ? "" : $", Model: {Model}";
            string name = string.IsNullOrEmpty(PhoneName) ? "" : $"{PhoneName}, ";
            return $"{Index}: {name}n°: {PhoneNumber}, IMSI: {IMSI}{model}{version}{r}";
        }

        public string ErrorString()
        {
            string m = string.IsNullOrEmpty(Model) ? "" : $" ({Model}, Android {Version})";
            return $"n°: {PhoneNumber}, IMSI: {IMSI}{m}";
        }

        internal string WriteCsv()
        {
            return $"{Index};{PhoneNumber};{IMSI};{PhoneName}";
        }

        public int CompareTo(object obj)
        {
            Phone other = (Phone)obj;
            return Index.CompareTo(other.Index);
        }
    }
}