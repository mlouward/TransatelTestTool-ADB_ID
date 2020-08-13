using System;
using System.Collections.Generic;

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
        public string AdbId { get; set; }
        public int[] SubIds { get; set; } = new int[2];

        public Phone(int index, string phoneNumber, string iMSI)
        {
            Index = index;
            PhoneNumber = phoneNumber;
            IMSI = iMSI;
            IsRooted = false;
        }

        public Phone(string phoneNumber, string iMSI)
        {
            PhoneNumber = phoneNumber;
            IMSI = iMSI;
            IsRooted = false;
        }

        public override string ToString()
        {
            // We don't display information if unavailable. (i.e. device unplugged.)
            string r = IsRooted ? " (rooted)" : "";
            string v = string.IsNullOrEmpty(Version) ? "" : $", Android {Version}";
            string m = string.IsNullOrEmpty(Model) ? "" : $", Model: {Model}";
            return $"{Index}: n°: {PhoneNumber}, IMSI: {IMSI}{m}{v}{r}";
        }

        public string ErrorString()
        {
            string m = string.IsNullOrEmpty(Model) ? "" : $" ({Model}, Android {Version})";
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