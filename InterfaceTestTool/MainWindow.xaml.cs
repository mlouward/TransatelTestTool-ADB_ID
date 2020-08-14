using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InterfaceTestTool
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // List of the the phones written in simInfos.csv
        private static List<Phone> validPhones = new List<Phone>();

        // List of the tests written in testsToPerform.csv
        private static List<ITest> tests = new List<ITest>();

        // List of the indexes of the phones in validPhones that are currently plugged in
        private static List<int> validIndexes = new List<int>();

        // List of the indexes of rooted phones in validPhones
        private static List<int> rootedIndexes = new List<int>();

        // List of the APNs for a specific phone and MCC/MNC
        private static List<APN> apnList = new List<APN>();

        // List of tests used for Copy/Paste shortcut in the TestsList ListView
        private static List<ITest> testCopy = new List<ITest>();

        // Used to not query the APNs if it has already been done for a phone.
        private static bool apnListChanged = false;

        // Used to correspond Type of number with the phone number prefix
        public static Dictionary<int, string> indexToPrefix = new Dictionary<int, string>() {
            {0, "+" },
            {1, "00" },
            {2, "0" },
        };

        // Used to correspond phone number code to Type of number
        public static Dictionary<string, string> prefixToType = new Dictionary<string, string>() {
            {"+" , "International (+)" },
            {"00", "International (00)" },
            {"0" , "National Format" },
        };

        public MainWindow()
        {
            InitializeComponent();
            // Working directory is main folder.
            Directory.SetCurrentDirectory(@"../../../");
            // Get phones in simInfos.csv
            validPhones = GetAllPhones(@"simInfos.csv");
            // Gets phone's model, Android version and root status if phone is plugged in,
            // and updates validPhones and validIndexes, as well as setting ItemsSource for
            // the corresponding ListView/ComboBox
            UpdatePhoneList();
            rootedIndexes = validPhones.Where(p => p.IsRooted).Select(p => p.Index).ToList();
            // Only rooted phones are available for apn changes.
            PhonesListAPN.ItemsSource = validPhones.Where(p => p.IsRooted);
            // Read test file and add tests to listview and tests List
            GetTestsFromFile(@"testsToPerform.csv");
            TestsList.ItemsSource = tests;
            // Query APNs for the first valid phone
            if (rootedIndexes.Count > 0) RefreshApnList(rootedIndexes[0], "208", "22");
        }

        /// <summary>
        /// Reads the file with SIM infos and instantiates a list of Phones according to it.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static List<Phone> GetAllPhones(string path)
        {
            List<Phone> res = new List<Phone>();
            using (StreamReader sr = new StreamReader(path))
            {
                sr.ReadLine(); // Ignore header of simInfos.csv
                while (!sr.EndOfStream)
                {
                    string[] l = sr.ReadLine().Split(';');
                    if (l.Length > 2)
                        res.Add(new Phone(int.Parse(l[0]), l[1], l[2]));
                }
            }
            return res;
        }

        /// <summary>
        /// Uses validPhones to update the phone list, and gets the model/android version/root status
        /// of the phones plugged in.
        /// </summary>
        private void UpdatePhoneList()
        {
            PhonesList.ItemsSource = null;
            // Sort by index
            validPhones.Sort();
            // Refresh validIndexes.
            validIndexes = validPhones.Select(p => p.Index).ToList();

            string s = ""; // Indexes of all phones in simInfos.csv
            foreach (var item in validIndexes) s += item.ToString() + " ";
            try
            {
                ProcessStartInfo start = new ProcessStartInfo("python.exe", $"checkRoot.py {s.Trim()}")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process p = new Process { StartInfo = start };
                Mouse.OverrideCursor = Cursors.Wait;
                p.Start();
                p.WaitForExit();

                using (StreamReader sr = new StreamReader("rootList.txt"))
                {
                    for (int i = 0; i < validPhones.Count; i++)
                    {
                        var l = sr.ReadLine().Split(';');
                        if (l.Length > 1)
                        {
                            validPhones[i].Model = l[0];
                            validPhones[i].Version = l[1];
                            validPhones[i].IsRooted = bool.Parse(l[2]);
                        }
                    }
                }
                File.Delete("rootList.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Mouse.OverrideCursor = Cursors.Arrow;
            // Valid indexes are only for plugged in phones (Model != "").
            validIndexes = validPhones.Where(p => !string.IsNullOrEmpty(p.Model)).Select(p => p.Index).ToList();

            // Update item sources
            PhonesList.ItemsSource = validPhones;
            From.ItemsSource = null;
            From.ItemsSource = validPhones.Where(p => !string.IsNullOrEmpty(p.Model));
            From.SelectedIndex = 0;
            PhonesListAPN.ItemsSource = null;
            PhonesListAPN.ItemsSource = validPhones.Where(x => x.IsRooted);
            PhonesListAPN.SelectedIndex = 0;
        }

        /// <summary>
        /// Populates the 'tests' list by reading the tests file.
        /// </summary>
        /// <param name="path"></param>
        private void GetTestsFromFile(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                string type;
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (line == null || string.IsNullOrEmpty(line) || line == "\r\n") return;

                    string[] l = line.Split(';');
                    switch (l[0].ToLower())
                    {
                        case "moc":
                            prefixToType.TryGetValue(l[6], out type);
                            tests.Add(new MOC(int.Parse(l[1]), int.Parse(l[2]), int.Parse(l[3]), l[4], int.Parse(l[5]), type));
                            break;

                        case "mtc":
                            prefixToType.TryGetValue(l[6], out type);
                            tests.Add(new MTC(int.Parse(l[1]), int.Parse(l[2]), int.Parse(l[3]), int.Parse(l[4]), int.Parse(l[5]), type));
                            break;

                        case "sms":
                            prefixToType.TryGetValue(l[6], out type);
                            tests.Add(new SMS(int.Parse(l[1]), l[2], int.Parse(l[3]), l[4], int.Parse(l[5]), type));
                            break;

                        case "data":
                            tests.Add(new Data(int.Parse(l[1]), l[2], int.Parse(l[3]), int.Parse(l[4])));
                            break;

                        case "speedtest":
                            tests.Add(new Speedtest(int.Parse(l[1]), int.Parse(l[2]), int.Parse(l[3])));
                            break;

                        case "ping":
                            tests.Add(new Ping(int.Parse(l[1]), int.Parse(l[2]), l[3], int.Parse(l[4]), int.Parse(l[5])));
                            break;

                        case "airplane":
                            tests.Add(new Airplane(int.Parse(l[1]), int.Parse(l[2]), int.Parse(l[3])));
                            break;

                        case "changeapn":
                            tests.Add(new ChangeApn(int.Parse(l[1]), new APN(int.Parse(l[2]), l[3], l[4]), int.Parse(l[5])));
                            break;

                        default:
                            // To add a test, add a 'case' before default.
                            MessageBox.Show($"Unrecognized test '{l[0]}'.");
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Provides a validation for each type of text.
        /// </summary>
        /// <param name="type">The type of the test (MOC, MTC...)</param>
        /// <returns>True if the form is valid for the selected test, False otherwise.</returns>
        private bool ValidateForm(string type)
        {
            Match m;
            Match n;
            switch (type)
            {
                case "MOC":
                    if (!int.TryParse(NbTests.Text.Trim(), out _))
                    {
                        MessageBox.Show("You must enter an integer in the field 'Number of tests'.",
                                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    if (To.Text.Trim().Length <= 2 && !validIndexes.Contains(int.Parse(To.Text.Trim())))
                    {
                        MessageBox.Show($"{To.Text} is not a valid index for Phone B in 'simInfos.csv'",
                                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    if ((From.SelectedItem as Phone).Index.ToString() == To.Text.Trim() || ((Phone)From.SelectedItem).PhoneNumber == To.Text.Trim())
                    {
                        MessageBox.Show("Phone A and Phone B must be different.");
                        return false;
                    }
                    m = Regex.Match(To.Text.Trim(), @"^(?:\+?(\d{1,3}))?([-. (]*(\d{3})[-. )]*)?((\d{3})[-. ]*(\d{2,4})(?:[-.x ]*(\d+))?)$");
                    if (To.Text.Trim().Length > 2 && !m.Success)
                    {
                        var q = MessageBox.Show("Phone B is not a valid phone number or index in the list. Would you like to continue?",
                                        "Wrong number",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);
                        if (q == MessageBoxResult.No) return false;
                    }
                    break;

                case "MTC":
                    if (!int.TryParse(NbTests.Text.Trim(), out _))
                    {
                        MessageBox.Show("You must enter an integer in the field 'Number of tests'.",
                                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    if (!int.TryParse(To.Text.Trim(), out _))
                    {
                        MessageBox.Show("You must enter an integer in the field 'Phone B'.",
                                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    // Length more than 2 is not an index.
                    if (To.Text.Trim().Length > 2)
                    {
                        MessageBox.Show("Phone B must be the index of a phone.");
                        return false;
                    }
                    if (!validIndexes.Contains(int.Parse(To.Text.Trim())))
                    {
                        MessageBox.Show($"{To.Text} is not a valid index for Phone B in 'simInfos.csv'",
                                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    if ((From.SelectedItem as Phone).Index.ToString() == To.Text.Trim())
                    {
                        MessageBox.Show("Phone A and Phone B must be different.");
                        return false;
                    }
                    break;

                case "SMS":
                    if (!int.TryParse(NbTests.Text, out _))
                    {
                        MessageBox.Show("You must enter an integer in the field 'Number of tests'.",
                                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    if (To.Text.Trim().Length <= 2 && !validIndexes.Contains(int.Parse(To.Text.Trim())))
                    {
                        MessageBox.Show($"{To.Text} is not a valid index for Phone B in 'simInfos.csv'",
                                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    if (Message.Text.Length > 150)
                    {
                        var res = MessageBox.Show($"The message length ({Message.Text.Length}) is greater than 150. Your message" +
                            $" might not be sent properly. Would you like to continue?",
                            "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (res == MessageBoxResult.No) Message.Text = "";
                        return (res == MessageBoxResult.Yes);
                    }
                    if (Message.Text.Length == 0)
                    {
                        MessageBox.Show($"The message is empty. Your message can not be sent.");
                        return false;
                    }
                    m = Regex.Match(To.Text.Trim(), @"^(?:\+?(\d{1,3}))?([-. (]*(\d{3})[-. )]*)?((\d{3})[-. ]*(\d{2,4})(?:[-.x ]*(\d+))?)$");
                    if (To.Text.Trim().Length > 2 && !m.Success)
                    {
                        var q = MessageBox.Show("Phone B is not a valid phone number or index in the list. Would you like to continue?",
                                        "Wrong number",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);
                        if (q == MessageBoxResult.No) return false;
                    }
                    break;

                case "Data":
                    if (!int.TryParse(NbTests.Text.Trim(), out _))
                    {
                        MessageBox.Show("You must enter an integer in the field 'Number of tests'.",
                                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    // If left empty, default URL is google.com.
                    if (URL.Text.Trim() == "")
                    {
                        return true;
                    }
                    // m matches url, n matches IP address.
                    m = Regex.Match(URL.Text.Trim(), @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)");
                    n = Regex.Match(URL.Text.Trim(), @"^(?:[0-9]{1,3}\.){1,3}[0-9]{1,3}$");
                    if (!m.Success && !n.Success)
                    {
                        MessageBox.Show(@"URL must be a valid address (e.g. 'https://www.example.com' or '8.8.8.8')");
                        return false;
                    }
                    break;

                case "Ping":
                    if (!int.TryParse(NbTests.Text.Trim(), out _))
                    {
                        MessageBox.Show("You must enter an integer in the field 'Number of tests'.",
                                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    if (!int.TryParse(PacketSize.Text.Trim(), out _))
                    {
                        MessageBox.Show("You must enter an integer in the field 'Packet Size'.",
                                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    m = Regex.Match(URL.Text.Trim(), @"(https?:\/\/)?(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)");
                    n = Regex.Match(URL.Text.Trim(), @"^(?:[0-9]{1,3}\.){1,3}[0-9]{1,3}$");
                    if (!m.Success && !n.Success)
                    {
                        MessageBox.Show(@"URL must be a valid address (e.g. 'https://www.example.com' or '8.8.8.8')");
                        return false;
                    }

                    if (string.IsNullOrEmpty(PacketSize.Text.Trim())) PacketSize.Text = "32";

                    if (int.Parse(PacketSize.Text.Trim()) < 16)
                    {
                        var res = MessageBox.Show("A packet size less than 16 will not display RTT statistics. " +
                            "Are you sure you want to add this test?", "Attention", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (res == MessageBoxResult.Yes) break;
                        return false;
                    }
                    break;

                case "Speedtest":
                    if (Size.SelectedItem == null)
                    {
                        MessageBox.Show("You must choose a file size.");
                        return false;
                    }
                    break;

                case "AddPhone":
                    if (Imsi.Text.Length > 15 || Imsi.Text.Length < 14)
                    {
                        MessageBox.Show($"IMSI length must be 14 or 15 digits (Current: {Imsi.Text.Length}).");
                        return false;
                    }
                    m = Regex.Match(PhoneNumber.Text.Trim(), @"^(?:\+?(\d{1,3}))?([-. (]*(\d{3})[-. )]*)?((\d{3})[-. ]*(\d{2,4})(?:[-.x ]*(\d+))?)$");
                    if (!m.Success)
                    {
                        MessageBox.Show("Phone Number is not in a valid format.");
                        return false;
                    }
                    if (validIndexes.Count == 99)
                    {
                        MessageBox.Show("There are too many phones in the file. Please remove at least one sim card to add a new one.");
                        return false;
                    }
                    break;

                case "Airplane":
                    if (!int.TryParse(Duration.Text.Trim(), out _))
                    {
                        MessageBox.Show("Duration field must be a valid integer.");
                        return false;
                    }
                    if (!(From.SelectedItem as Phone).IsRooted)
                    {
                        MessageBox.Show("Only rooted phones can use airplane mode.");
                        return false;
                    }
                    break;

                case "ChangeApn":
                    if (!(From.SelectedItem as Phone).IsRooted)
                    {
                        MessageBox.Show("Only rooted phones can change default APN.");
                        return false;
                    }
                    break;

                default:
                    MessageBox.Show("Wrong test");
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Adds the test in parameter to the list of tests n times,
        /// and writes them to the testsToPerform.csv file
        /// </summary>
        /// <param name="t">An ITest object to add. </param>
        /// <param name="n">The number of tests to add. </param>
        private void AddTest(ITest t, int n)
        {
            for (int i = 0; i < n; i++) tests.Add(t);
            TestsList.ItemsSource = null;
            TestsList.ItemsSource = tests;
            WriteTests(tests);
        }

        /// <summary>
        /// Writes the tests in the list in parameter to the file 'testsToPerform.csv'
        /// </summary>
        /// <param name="t"></param>
        private static void WriteTests(List<ITest> t)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(@"testsToPerform.csv"))
                {
                    foreach (ITest item in t)
                    {
                        sw.WriteLine(item.WriteCsv());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Disables the fields not required for the selected test.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (TestType.SelectedIndex)
            {
                case 0:
                    NbTests.IsEnabled = true;
                    Duration.IsEnabled = true;
                    To.IsEnabled = true;
                    Message.IsEnabled = false;
                    PacketSize.IsEnabled = false;
                    //Message.Visibility = Visibility.Hidden;
                    Message.Text = "";
                    URL.IsEnabled = false;
                    URL.Text = "";
                    Size.IsEnabled = false;
                    Prefix.IsEnabled = true;
                    Size.SelectedIndex = -1;
                    APN.IsEnabled = false;
                    break;

                case 1:
                    NbTests.IsEnabled = true;
                    Duration.IsEnabled = true;
                    To.IsEnabled = true;
                    Message.IsEnabled = false;
                    Message.Text = "";
                    Prefix.IsEnabled = true;
                    PacketSize.IsEnabled = false;
                    URL.IsEnabled = false;
                    URL.Text = "";
                    Size.IsEnabled = false;
                    Size.SelectedIndex = -1;
                    APN.IsEnabled = false;
                    break;

                case 2:
                    NbTests.IsEnabled = true;
                    Duration.IsEnabled = false;
                    Duration.Text = "";
                    To.IsEnabled = true;
                    Message.IsEnabled = true;
                    Prefix.IsEnabled = true;
                    URL.IsEnabled = false;
                    PacketSize.IsEnabled = false;
                    URL.Text = "";
                    Size.IsEnabled = false;
                    Size.SelectedIndex = -1;
                    APN.IsEnabled = false;
                    break;

                case 3:
                    NbTests.IsEnabled = true;
                    Duration.IsEnabled = false;
                    Duration.Text = "";
                    To.IsEnabled = false;
                    To.Text = "";
                    Message.IsEnabled = false;
                    Message.Text = "";
                    Prefix.IsEnabled = false;
                    URL.IsEnabled = true;
                    PacketSize.IsEnabled = false;
                    Size.IsEnabled = false;
                    Size.SelectedIndex = -1;
                    APN.IsEnabled = false;
                    break;

                case 4:
                    NbTests.IsEnabled = false;
                    NbTests.Text = "";
                    Duration.IsEnabled = false;
                    Duration.Text = "";
                    To.IsEnabled = false;
                    To.Text = "";
                    Message.IsEnabled = false;
                    Message.Text = "";
                    Prefix.IsEnabled = false;
                    URL.IsEnabled = false;
                    URL.Text = "";
                    PacketSize.IsEnabled = false;
                    Size.IsEnabled = true;
                    APN.IsEnabled = false;
                    break;

                case 5:
                    NbTests.IsEnabled = true;
                    Duration.IsEnabled = false;
                    Duration.Text = "";
                    To.IsEnabled = false;
                    To.Text = "";
                    Message.IsEnabled = false;
                    Message.Text = "";
                    Prefix.IsEnabled = false;
                    URL.IsEnabled = true;
                    PacketSize.IsEnabled = true;
                    PacketSize.Text = "32";
                    Size.IsEnabled = false;
                    Size.SelectedIndex = -1;
                    APN.IsEnabled = false;
                    break;

                case 6:
                    NbTests.IsEnabled = false;
                    Duration.IsEnabled = true;
                    Duration.Text = "";
                    To.IsEnabled = false;
                    To.Text = "";
                    Message.IsEnabled = false;
                    Message.Text = "";
                    Prefix.IsEnabled = false;
                    URL.IsEnabled = false;
                    URL.Text = "";
                    PacketSize.IsEnabled = false;
                    Size.IsEnabled = false;
                    Size.SelectedIndex = -1;
                    APN.IsEnabled = false;
                    break;

                case 7:
                    NbTests.IsEnabled = false;
                    Duration.IsEnabled = false;
                    Duration.Text = "";
                    To.IsEnabled = false;
                    To.Text = "";
                    Message.IsEnabled = false;
                    Message.Text = "";
                    Prefix.IsEnabled = false;
                    URL.IsEnabled = false;
                    URL.Text = "";
                    PacketSize.IsEnabled = false;
                    Size.IsEnabled = false;
                    Size.SelectedIndex = -1;
                    APN.IsEnabled = true;
                    if (apnListChanged)
                    {
                        RefreshApnList((From.SelectedItem as Phone).Index, "208", "22");
                        apnListChanged = false;
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Only allows integers input in the Delay TextBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delay_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            bool res = IsTextAllowed(e.Text);
            if (!res) SystemSounds.Asterisk.Play();
            e.Handled = !res;
        }

        /// <summary>
        /// Regex match for numbers only.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex(@"[^0-9]");
            return !regex.IsMatch(text);
        }

        /// <summary>
        /// Adds the test selected with the corresponding parameters to the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Delay.Text.Trim())) Delay.Text = "0"; // Default delay is 0 sec.
            if (string.IsNullOrEmpty(Repetitions.Text.Trim())) Repetitions.Text = "1"; // Default repetitions is 1.
            bool b = int.TryParse(Repetitions.Text.Trim(), out int n);
            if (!b)
            {
                MessageBox.Show("Repetitions field must be a valid integer.");
                return;
            }
            b = int.TryParse(Delay.Text.Trim(), out int d);
            if (!b)
            {
                MessageBox.Show("Delay field must be a valid integer.");
                return;
            }
            switch (TestType.SelectedIndex)
            {
                case -1:
                    MessageBox.Show("Please, select a test to add to the list from the dropdown menu at the top.");
                    return;

                case 0:
                    if (ValidateForm("MOC"))
                    {
                        indexToPrefix.TryGetValue(Prefix.SelectedIndex, out string type);
                        AddTest(new MOC(int.Parse(NbTests.Text.Trim()), int.Parse(Duration.Text.Trim()), (From.SelectedItem as Phone).Index, To.Text.Trim(), d, type), n);
                        break;
                    }
                    return;

                case 1:
                    if (ValidateForm("MTC"))
                    {
                        indexToPrefix.TryGetValue(Prefix.SelectedIndex, out string type);
                        AddTest(new MTC(int.Parse(NbTests.Text.Trim()), int.Parse(Duration.Text.Trim()), (From.SelectedItem as Phone).Index, int.Parse(To.Text.Trim()), d, type), n);
                        break;
                    }
                    return;

                case 2:
                    if (ValidateForm("SMS"))
                    {
                        indexToPrefix.TryGetValue(Prefix.SelectedIndex, out string type);
                        AddTest(new SMS(int.Parse(NbTests.Text.Trim()), Message.Text.Trim(), (From.SelectedItem as Phone).Index, To.Text.Trim(), d, type), n);
                        break;
                    }
                    return;

                case 3:
                    if (ValidateForm("Data"))
                    {
                        AddTest(new Data(int.Parse(NbTests.Text.Trim()), URL.Text.Trim(), (From.SelectedItem as Phone).Index, d), n);
                        break;
                    }
                    return;

                case 4:
                    if (ValidateForm("Speedtest"))
                    {
                        string s = Size.Text.Trim().Split(' ')[0];
                        AddTest(new Speedtest((From.SelectedItem as Phone).Index, int.Parse(s), d), n);
                        break;
                    }
                    return;

                case 5:
                    if (ValidateForm("Ping"))
                    {
                        AddTest(new Ping((From.SelectedItem as Phone).Index, d, URL.Text.Trim(), int.Parse(NbTests.Text), int.Parse(PacketSize.Text)), n);
                        break;
                    }
                    return;

                case 6:
                    if (ValidateForm("Airplane"))
                    {
                        AddTest(new Airplane((From.SelectedItem as Phone).Index, d, int.Parse(Duration.Text.Trim())), n);
                        break;
                    }
                    return;

                case 7:
                    if (ValidateForm("ChangeApn"))
                    {
                        APN apn = apnList.Where(p => p.Id == (APN.SelectedItem as APN).Id).First();
                        AddTest(new ChangeApn((From.SelectedItem as Phone).Index, apn, d), n);
                    }
                    return;

                default:
                    return;
            }
        }

        /// <summary>
        /// Erases all the content of the tests file as well as clearing the 'tests' list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Warning! This operation will delete all the tests in the list, as well as clear the " +
                "file 'testsToPerform.csv'. Are you sure you want to proceed?",
                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                File.WriteAllText(@"testsToPerform.csv", string.Empty);
                tests.Clear();
                TestsList.ItemsSource = null;
            }
        }

        /// <summary>
        /// Deletes the selected test(s) in the listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (TestsList.SelectedItems != null)
            {
                //var selection = TestsList.SelectedItems as List<ITest>;
                foreach (var item in TestsList.SelectedItems)
                {
                    tests.Remove(item as ITest);
                }
                TestsList.ItemsSource = null;
                TestsList.ItemsSource = tests;
                WriteTests(tests);
            }
        }

        /// <summary>
        /// Shortcut for Del key and Ctrl+C/Ctrl+V in Tests list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete) Delete_Click(sender, e);

            if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                testCopy.Clear();
                foreach (var item in TestsList.SelectedItems)
                {
                    testCopy.Add(item as ITest);
                }
                //testCopy = TestsList.SelectedItems as List<ITest>;
                return;
            }

            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (testCopy != null)
                {
                    foreach (ITest test in testCopy)
                    {
                        AddTest(test, 1);
                    }
                }
            }
        }

        /// <summary>
        /// Executes the Python program to run each test in the testsToPerform.csv file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Execute_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            ProcessStartInfo start = new ProcessStartInfo("python.exe", @"Test_Tool.py")
            {
                //UseShellExecute = false,
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
                //CreateNoWindow = true
            };

            Process p = new Process() { StartInfo = start };
            p.Start();
            p.WaitForExit();
            Mouse.OverrideCursor = Cursors.Arrow;

            //string errors = "--------------ERRORS--------------\n";
            //string res = "";
            //errors += p.StandardError.ReadToEnd();
            //res += p.StandardOutput.ReadToEnd();
            //if (errors != "--------------ERRORS--------------\n") MessageBox.Show(errors);
            //if (res != "") MessageBox.Show(res);

            MessageBox.Show("All the tests have been run.");
        }

        /// <summary>
        /// Saves the current test setup to a csv file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (TestsList.Items.Count == 0)
            {
                MessageBox.Show("Test list is empty.");
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv|Text file (*.txt)|*.txt|All files (*.*)|*.*",
                InitialDirectory = Path.GetFullPath(@"Saved Test Files/"),
                DefaultExt = "csv"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                using (StreamReader stream = new StreamReader(Path.GetFullPath(@"testsToPerform.csv")))
                {
                    string res = stream.ReadToEnd();
                    File.WriteAllText(saveFileDialog.FileName, res);
                }
            }
        }

        /// <summary>
        /// Loads a saved setup from a csv file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "CSV file (*.csv)|*.csv|Text file (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.InitialDirectory = Path.GetFullPath(@"Saved Test Files/");
            dialog.DefaultExt = "csv";
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true) // When user clicks "OK".
            {
                foreach (var filename in dialog.FileNames)
                {
                    GetTestsFromFile(Path.GetFullPath(filename));
                }
                TestsList.ItemsSource = null;
                TestsList.ItemsSource = tests;
                WriteTests(tests);
            }
        }

        /// <summary>
        /// Updates simInfos.csv with the list validPhones.
        /// </summary>
        /// <param name="phone"></param>
        private void WritePhones()
        {
            using (StreamWriter sw = new StreamWriter("simInfos.csv", false))
            {
                // Header
                sw.WriteLine("Index;PhoneNumber;IMSI");
                for (int i = 0; i < validPhones.Count; i++)
                {
                    validPhones[i].Index = i + 1; // Index starting at 1.
                    sw.WriteLine(validPhones[i].WriteCsv());
                }
            }
            validPhones = GetAllPhones("simInfos.csv");
            UpdatePhoneList();
        }

        /// <summary>
        /// Add a phone with its IMSI and Phone number.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddPhone_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm("AddPhone"))
            {
                validPhones.Add(new Phone(PhoneNumber.Text.Trim(), Imsi.Text.Trim()));
                WritePhones();
            }
            return;
        }

        /// <summary>
        /// Remove the selected phone(s) from the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemovePhones_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            if (PhonesList.SelectedItems.Count != 0)
            {
                var selection = PhonesList.SelectedItems;
                foreach (Phone phone in selection)
                {
                    validPhones.Remove(phone);
                }
                WritePhones();
            }
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        /// <summary>
        /// Shortcut for the Del key in Phones list, and allows
        /// to Ctrl+C the content of one or multiple line (phone infos).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PhonesList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete) RemovePhones_Click(sender, e);

            if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                string s = "";
                foreach (var selection in PhonesList.SelectedItems)
                {
                    s += selection.ToString() + "\n";
                }
                Clipboard.SetText(s);
                return;
            }
        }

        /// <summary>
        /// Enable airplane mode if phone is rooted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Airplane_Click(object sender, RoutedEventArgs e)
        {
            // Default duration is 3 seconds
            if (AirplaneDuration.Text.Trim() == "") AirplaneDuration.Text = "3";
            if (PhonesList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please, select one or multiple phones to activate airplane mode for.");
                return;
            }
            string s = "";
            foreach (Phone phone in PhonesList.SelectedItems)
            {
                if (phone.IsRooted)
                {
                    try
                    {
                        Process p = new Process();
                        ProcessStartInfo start = new ProcessStartInfo(
                            "python.exe", $"airplane.py {phone.Index} {AirplaneDuration.Text}");
                        start.CreateNoWindow = true;
                        start.UseShellExecute = false;
                        p.StartInfo = start;
                        p.Start();
                        p.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    s += phone.ErrorString() + "\n";
                }
            }
            if (string.IsNullOrEmpty(s))
            {
                MessageBox.Show(s + "\nThese phones are not rooted, thus Airplane mode couldn't be used.");
            }
        }

        /// <summary>
        /// Refreshes the list of APNs for the selected phone and MCC/MNC
        /// </summary>
        /// <param name="index">Index of the phone to search for APNs</param>
        /// <param name="mcc">MCC to search for</param>
        /// <param name="mnc">MNC to search for</param>
        private void RefreshApnList(int index, string mcc, string mnc)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                apnList.Clear();
                Process p = new Process();
                ProcessStartInfo start = new ProcessStartInfo(
                    "python.exe", $"getApn.py {index} {mcc}{mnc}")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                p.StartInfo = start;
                p.Start();
                p.WaitForExit();

                using (StreamReader sr = new StreamReader("apn.csv"))
                {
                    while (!sr.EndOfStream)
                    {
                        var l = sr.ReadLine().Split(';');
                        apnList.Add(new APN(int.Parse(l[0]), l[1], l[3], int.Parse(l[2]), bool.Parse(l[4])));
                    }
                }
                ApnListView.ItemsSource = null;
                ApnListView.ItemsSource = apnList;
                APN.ItemsSource = null;
                APN.ItemsSource = apnList;
                ApnText.Text = $"List of APNs for {validPhones.Find(x => x.Index == index).PhoneNumber} ({mcc}{mnc}): ";
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show($"{mcc}{mnc} is not a valid code, or has not been found in the phone. Add it manually and try again." +
                    $" (Could not find 'apn.csv')");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        /// <summary>
        /// Queries the phone for APNs of the selected MCC/MNC.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueryApn_Click(object sender, RoutedEventArgs e)
        {
            if (MCC.Text.Trim().Length < 2 || MNC.Text.Trim().Length < 2)
            {
                MessageBox.Show("You must enter valid MCC and MNC.");
                return;
            }
            RefreshApnList((PhonesListAPN.SelectedItem as Phone).Index, MCC.Text.Trim(), MNC.Text.Trim());
        }

        /// <summary>
        /// Add an APN with the selected Name, MCC/MNC and APN.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddApn_Click(object sender, RoutedEventArgs e)
        {
            apnListChanged = true;
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                Process p = new Process();
                ProcessStartInfo start = new ProcessStartInfo(
                    "python.exe", $"addApn.py \"{(PhonesListAPN.SelectedItem as Phone).IMSI}\" \"{NameAdd.Text.Trim()}\" \"{MCCAdd.Text.Trim()}\" \"{MNCAdd.Text.Trim()}\" \"{APNAdd.Text.Trim()}\"");
                start.CreateNoWindow = true;
                start.UseShellExecute = false;
                p.StartInfo = start;
                p.Start();
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Mouse.OverrideCursor = Cursors.Arrow;
            // Refresh the APN list to show the one added.
            RefreshApnList((PhonesListAPN.SelectedItem as Phone).Index, MCCAdd.Text.Trim(), MNCAdd.Text.Trim());
        }

        /// <summary>
        /// Sets the default APN for the selected phone
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetDefault_Click(object sender, RoutedEventArgs e)
        {
            apnListChanged = true;
            if (ApnListView.SelectedItem == null)
            {
                MessageBox.Show("Please, select an APN from the list to set as default.");
                return;
            }
            Mouse.OverrideCursor = Cursors.Wait;
            APN a = (APN)ApnListView.SelectedItem;
            try
            {
                Process p = new Process();
                ProcessStartInfo start = new ProcessStartInfo(
                    "python.exe", $"setApn.py \"{(PhonesListAPN.SelectedItem as Phone).IMSI}\" \"{a.Id}\"");
                start.CreateNoWindow = true;
                start.UseShellExecute = false;
                p.StartInfo = start;
                p.Start();
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            // Refresh the list
            QueryApn_Click(sender, e);
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        /// <summary>
        /// Delete one or multiple APN(s) from the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteApn_Click(object sender, RoutedEventArgs e)
        {
            apnListChanged = true;
            if (ApnListView.SelectedItem == null)
            {
                MessageBox.Show("Please, select an APN from the list to delete.");
                return;
            }
            Mouse.OverrideCursor = Cursors.Wait;
            APN apn = (APN)ApnListView.SelectedItem;
            try
            {
                Phone phone = PhonesListAPN.SelectedItem as Phone;
                Process p = new Process();
                ProcessStartInfo start = new ProcessStartInfo(
                    "python.exe", $"delApn.py {phone.IMSI} {apn.Id}");
                start.CreateNoWindow = true;
                start.UseShellExecute = false;
                p.StartInfo = start;
                p.Start();
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            // Refresh the list using last MCC/MNC.
            var mccmnc = ApnText.Text.Trim().Split(new char[] { '(', ')' })[1];
            RefreshApnList((PhonesListAPN.SelectedItem as Phone).Index, mccmnc.Substring(0, 3), mccmnc.Substring(3));
        }

        /// <summary>
        /// Selects all text in the TextBox if accessed using Tab.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.Tab))
                ((TextBox)sender).SelectAll();
        }

        /// <summary>
        /// Occurs when the application closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            File.Delete("apn.csv");
        }

        /// <summary>
        /// Gets the list of APNs for the selected phone in Phone A field
        /// when the test is "Change APN"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void From_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TestType.SelectedIndex == 7)
            {
                RefreshApnList((From.SelectedItem as Phone).Index, "208", "22");
            }
        }

        private void RefreshPhoneA_Click(object sender, RoutedEventArgs e)
        {
            UpdatePhoneList();
            if (From.SelectedItem != null && (From.SelectedItem as Phone).IsRooted)
            {
                RefreshApnList((From.SelectedItem as Phone).Index, "208", "22");
            }
        }
    }
}