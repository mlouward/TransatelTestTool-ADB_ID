namespace InterfaceTestTool
{
    internal interface ITest
    {
        /// <summary>
        /// The index of the phone to send the test from.
        /// </summary>
        int From { get; set; }
        /// <summary>
        /// The delay after the test.
        /// </summary>
        int Delay { get; set; }

        /// <summary>
        /// Used to add the test to the CSV file.
        /// </summary>
        /// <returns></returns>
        string WriteCsv();

        string ToString();
    }
}