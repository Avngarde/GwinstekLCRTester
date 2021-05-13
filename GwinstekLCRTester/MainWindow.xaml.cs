using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;

namespace GwinstekLCRTester
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set data for WPF form
            FilePath.Text = FileHandler.ReadTestFilesPath();

            string[] ports = SerialPort.GetPortNames();

            if (FileHandler.ReadTestFilesPath() == string.Empty)
            {
                string default_path = FileHandler.CreateDefaultPath();
                FilePath.Text = default_path;
            }

            Parity[] parities = new Parity[]
            {
                Parity.None,
                Parity.Odd,
                Parity.Even,
                Parity.Space,
                Parity.Mark
            };

            string[] units = new string[]
            {
                "Podstawowa jednostka",
                "n",
                "m",
                "μ"
            };

            uint[] bits = new uint[]
            {
                7,
                8
            };

            uint[] baudRates = new uint[]
            {
                4800,
                9600,
                14400,
                19200,
                38400,
                56000,
                57600,
                115200
            };

            StopBits[] stopBits = new StopBits[]
            {
                StopBits.One,
                StopBits.None,
                StopBits.OnePointFive,
                StopBits.Two
            };

            Handshake[] handshakes = new Handshake[]
            {
                Handshake.None,
                Handshake.RequestToSend,
                Handshake.RequestToSendXOnXOff,
                Handshake.XOnXOff
            };


            try
            {
                ParityList.ItemsSource = parities;
                ParityList.SelectedItem = parities[0];
                StopBitsList.ItemsSource = stopBits;
                StopBitsList.SelectedItem = stopBits[0];

                Handshakes.ItemsSource = handshakes;
                Handshakes.SelectedItem = handshakes[0];

                unitList.ItemsSource = units;
                unitList.SelectedItem = units[0];

                ModeList.ItemsSource = RSCommunication.measurementTypes;
                ModeList.SelectedItem = RSCommunication.measurementTypes[0];

                TransSpeed.ItemsSource = baudRates;
                TransSpeed.SelectedItem = baudRates[7];

                DataBit.ItemsSource = bits;
                DataBit.SelectedItem = bits[1];

                ComPorts.ItemsSource = ports;
                ComPorts.SelectedItem = ports[0];

            }
            catch (IndexOutOfRangeException)
            {
                System.Windows.MessageBox.Show("Nie znaleziono portów COM");
                Close();
            };
        }

        private void ChangeSendButtonText(bool finished)
        {
            switch (finished)
            {
                case true:
                    SendButton.Content = "Wykonaj testy";
                    break;
                case false:
                    SendButton.Content = "Wykonywanie...";
                    break;

            }
        }

        private void ExecuteTests()
        {
            var portName = SerialPort.GetPortNames();
            var baudRate = TransSpeed.Text == "" ? 115200 : uint.Parse(TransSpeed.Text);
            var dataBits = DataBit.Text == "" ? 8 : uint.Parse(DataBit.Text);
            var parity = (Parity)Enum.Parse(typeof(Parity), ParityList.Text);
            var stopBits = (StopBits)Enum.Parse(typeof(StopBits), StopBitsList.Text);
            var handshake = (Handshake)Enum.Parse(typeof(Handshake), Handshakes.Text);

            string[] frequencies = new string[4];

            if (ModeList.SelectedItem.ToString() != "DCR")
            {
                frequencies[0] = Freq1.Text;
                frequencies[1] = Freq2.Text;
                frequencies[2] = Freq3.Text;
                frequencies[3] = Freq4.Text;
            } 
            else 
            {
                frequencies[0] = Freq1.Text;
            }

            RSCommunication rsConnector = new RSCommunication(
                portName: ComPorts.Text,
                baudRate: baudRate,
                parityNumber: parity,
                dataBits: dataBits,
                stopBits: stopBits,
                handshakeType: handshake
             );



            for (int iter = 0; iter < uint.Parse(Cycles.Text); iter++)
            {
                if (SerialTest.IsChecked != true)
                {
                    System.Windows.MessageBox.Show("Proszę podpiąć następne urządzenie numer: " + (iter + 1));
                }
                foreach (string freq in frequencies)
                {
                    if (freq != "" || freq != "0")
                    {
                        System.Threading.Thread.Sleep(3000);
                        rsConnector.changeHzInDevice(freq);
                        decimal[] responseParams = rsConnector.testFullParams(ModeList.Text, unitList.Text, addD: (DParameter.Visibility == Visibility.Hidden) ? false : DParameter.IsChecked == true);
                        rsConnector.writeToCSV(responseParams, unitList.Text, freq, ModeList.Text, FilePath.Text, (iter + 1));
                    }
                }
            }

            rsConnector.closeCSV();
            System.Threading.Thread.Sleep(200);
            rsConnector.unlockKeypadInDevice();
            rsConnector.closePort();
            rsConnector.Dispose();


            System.Windows.MessageBox.Show("Wykonano wszystkie testy");
            SendButton.Content = "Wykonaj test";
        }

        private void Test_Data(object sender, RoutedEventArgs e)
        {
            ChangeSendButtonText(false); // Change button text during executing tests
            ExecuteTests();
            ChangeSendButtonText(true); // Change button after tests are finished
        }

        private void Set_File_Path(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog browser = new FolderBrowserDialog();
            var result = browser.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                FilePath.Text = browser.SelectedPath;
                FileHandler.WriteNewPathToFile(browser.SelectedPath);
            }
        }

        private void Change_Mode(object sender, SelectionChangedEventArgs e)
        {
            string selectedMode = ModeList.SelectedItem.ToString();

            if (selectedMode.Contains("-D"))
            {
                DParameter.Visibility = Visibility.Hidden;
            }
            else
            {
                DParameter.Visibility = Visibility.Visible;
            }

            if (selectedMode.Contains("Cp-") || selectedMode.Contains("Cs-") || selectedMode.Contains("Lp-") || selectedMode.Contains("Ls-"))
            {
                unitList.Visibility = Visibility.Visible;
                UnitLabel.Visibility = Visibility.Visible;
            }
            else
            {
                unitList.Visibility = Visibility.Hidden;
                UnitLabel.Visibility = Visibility.Hidden;
            }

            if (selectedMode.Contains("DCR"))
            {
                Freq2.Visibility = Visibility.Hidden;
                Freq3.Visibility = Visibility.Hidden;
                Freq4.Visibility = Visibility.Hidden;
                Freq2Label.Visibility = Visibility.Hidden;
                Freq3Label.Visibility = Visibility.Hidden;
                Freq4Label.Visibility = Visibility.Hidden;

                HzLabel2.Visibility = Visibility.Hidden;
                HzLabel3.Visibility = Visibility.Hidden;
                HzLabel4.Visibility = Visibility.Hidden;

                Freq1.IsReadOnly = true;
                Freq1.Text = "120";
            }
            else
            {
                Freq2.Visibility = Visibility.Visible;
                Freq3.Visibility = Visibility.Visible;
                Freq4.Visibility = Visibility.Visible;
                Freq2Label.Visibility = Visibility.Visible;
                Freq3Label.Visibility = Visibility.Visible;
                Freq4Label.Visibility = Visibility.Visible;

                HzLabel2.Visibility = Visibility.Visible;
                HzLabel3.Visibility = Visibility.Visible;
                HzLabel4.Visibility = Visibility.Visible;

                Freq1.IsReadOnly = false;
            }
        }
    }
}