using System;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace GwinstekLCRTester
{
    public partial class MainWindow : Window
    {
        private FileHandler fileHandler = new FileHandler();

        public MainWindow()
        {
            InitializeComponent();

            // Set data for WPF form
            FilePath.Text = fileHandler.currentSettings.CSVPath;

            string[] ports = SerialPort.GetPortNames();

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
                ParityList.SelectedItem = fileHandler.currentSettings.Parity;

                StopBitsList.ItemsSource = stopBits;
                StopBitsList.SelectedItem = fileHandler.currentSettings.StopBit;

                Handshakes.ItemsSource = handshakes;
                Handshakes.SelectedItem = fileHandler.currentSettings.HandShake;

                unitList.ItemsSource = units;
                unitList.SelectedItem = fileHandler.currentSettings.MultiplierUnit;

                ModeList.ItemsSource = RSCommunication.measurementTypes;
                ModeList.SelectedItem = fileHandler.currentSettings.MeasurmentType;

                TransSpeed.ItemsSource = baudRates;
                TransSpeed.SelectedItem = fileHandler.currentSettings.TransmissionSpeed;
                
                DataBit.ItemsSource = bits;
                DataBit.SelectedItem = fileHandler.currentSettings.DataBits;

                Freq1.Text = fileHandler.currentSettings.Freq1;
                Freq2.Text = fileHandler.currentSettings.Freq2;
                Freq3.Text = fileHandler.currentSettings.Freq3;
                Freq4.Text = fileHandler.currentSettings.Freq4;

                ComPorts.ItemsSource = ports;
                ComPorts.SelectedItem = ports[0];
            }
            catch (IndexOutOfRangeException)
            {
                System.Windows.MessageBox.Show("Nie znaleziono aktywnych portów COM");
                //Close();
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


            if (AVGValue.Text != "AVG:")
            {
                System.Windows.MessageBox.Show("Rozpoczynanie mierzenia dla parametrów: Częstotliwości: " + Freq1.Text + " " + Freq2.Text + " " + Freq3.Text + " " + Freq4.Text);
                int iter = 0;
                while (true)
                {
                    if (SerialTest.IsChecked != true)
                    {
                        System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Jeśli chcesz rozpocząć mierzenie urządzenia numer: " + (iter + 1) + " kilknij OK, jeśli chcesz zakończyć mierzenie wciśnij Cancel", "Czy kontynuować?", MessageBoxButton.OKCancel);
                        if (result != MessageBoxResult.OK)
                        {
                            break;
                        }
                    }
                    foreach (string freq in frequencies)
                    {
                        if (freq != "" && freq != "0" && !string.IsNullOrEmpty(freq))
                        {
                            System.Threading.Thread.Sleep(3000);
                            rsConnector.changeHzInDevice(freq);
                            decimal[] responseParams = rsConnector.getMeasurementParams(ModeList.Text, unitList.Text, addD: (DParameter.Visibility == Visibility.Hidden) ? false : DParameter.IsChecked == true);
                            //rsConnector.writeToCSV(responseParams, unitList.Text, freq, ModeList.Text, FilePath.Text, (iter + 1));
                            
                        }
                    }
                    iter++;
                }
            }
            else
            {
                System.Threading.Thread.Sleep(2000);
                rsConnector.changeAVGInDevice(Cycles.Text);
                System.Threading.Thread.Sleep(2000);
                int waitMs = (int)((int.Parse(Cycles.Text) / 2.9090909 + 1) * 1000);
                foreach (string freq in frequencies)
                {
                    if (freq != "" && freq != "0" && !string.IsNullOrEmpty(freq))
                    {
                        System.Threading.Thread.Sleep(500);
                        rsConnector.changeHzInDevice(freq);
                        decimal[] responseParams = rsConnector.getMeasurementParams(ModeList.Text, unitList.Text, addD: (DParameter.Visibility == Visibility.Hidden) ? false : DParameter.IsChecked == true, waitMs);
                        fileHandler.writeCSV(responseParams, unitList.Text, freq, ModeList.Text, avg : AVGValue.Text);
                    }
                }
                rsConnector.changeAVGInDevice("1");
            }

            fileHandler.closeWriter();
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
               // settings.CSVPath = browser.SelectedPath;
                //FileHandler.WriteNewSettings(settings);
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

        private void SerialTest_Checked(object sender, RoutedEventArgs e)
        {
            AVGValue.Text = "AVG:";
        }

        private void SerialTest_Unchecked(object sender, RoutedEventArgs e)
        {
            AVGTextLabel.Visibility = Visibility.Hidden;
            AVGValue.Visibility = Visibility.Hidden;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings newSettings = new Settings();

            //Connection values
            newSettings.TransmissionSpeed = (uint)TransSpeed.SelectedItem;
            newSettings.DataBits = (uint)DataBit.SelectedItem;
            newSettings.Parity = (Parity)ParityList.SelectedItem;
            newSettings.StopBit = (StopBits)StopBitsList.SelectedItem;
            newSettings.HandShake = (Handshake)Handshakes.SelectedItem;

            //Test values
            newSettings.Freq1 = Freq1.Text;
            newSettings.Freq2 = Freq2.Text;
            newSettings.Freq3 = Freq3.Text;
            newSettings.Freq4 = Freq4.Text;

            newSettings.MultiplierUnit = unitList.Text;
            newSettings.MeasurmentType = ModeList.Text;
            newSettings.DChecked = DParameter.IsChecked.Value;

            newSettings.AVG = AVGValue.Text;
            newSettings.Cycles = Cycles.Text;
            newSettings.SerialTestChecked = SerialTest.IsChecked.Value;
            newSettings.CSVPath = FilePath.Text;

            fileHandler.writeNewSettings(newSettings);
        }
    }
}