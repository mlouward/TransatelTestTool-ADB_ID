namespace InterfaceTestTool
{
    internal class APN
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Apn { get; set; }
        public int Numeric { get; set; }
        public bool IsDefault { get; set; }

        public APN(int id, string name, string apn, int numeric, bool isDefault)
        {
            Id = id;
            Name = name;
            Apn = apn;
            Numeric = numeric;
            IsDefault = isDefault;
        }

        public override string ToString()
        {
            string def = IsDefault ? " (default) " : "";
            return $"{Name}: MCC MNC: {Numeric.ToString().Substring(0, 3)} {Numeric.ToString().Substring(3)}, APN: {Apn}{def}";
        }
    }
}