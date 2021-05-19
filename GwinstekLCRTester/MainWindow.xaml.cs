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

            SendButton.Content = finished switch
            {
                true => "Wykonaj testy",
                false => "Wykonywanie..."
            };
        }

        private void returnToIdle(RSCommunication rsConnector, bool fileHandlerExist, string errorMessage = "")
        {
            if (errorMessage != "")
                System.Windows.MessageBox.Show(errorMessage);

            rsConnector.changeAVGInDevice("1");
            if (fileHandlerExist) fileHandler.closeWriter();
            System.Threading.Thread.Sleep(200);
            rsConnector.unlockKeypadInDevice();
            rsConnector.closePort();
            rsConnector.Dispose();
            SendButton.Content = "Wykonaj test";
        }

        private void Test_Data(object sender, RoutedEventArgs e)
        {
            ChangeSendButtonText(false); // Change button text during executing tests
            ExecuteTests1();
            ChangeSendButtonText(true); // Change button after tests are finished
        }



        private void ExecuteTests1()
        {
            // pobieranie parametrów połączenia z urządzeniem
            var baudRate = TransSpeed.Text == "" ? 115200 : uint.Parse(TransSpeed.Text);
            var dataBits = DataBit.Text == "" ? 8 : uint.Parse(DataBit.Text);
            var parity = Enum.Parse(typeof(Parity), ParityList.Text);
            var stopBits = Enum.Parse(typeof(StopBits), StopBitsList.Text);
            var handshake = Enum.Parse(typeof(Handshake), Handshakes.Text);

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

            RSCommunication rsConnector;
            try
            {
                rsConnector = new RSCommunication(
                    portName: ComPorts.Text,
                    baudRate: baudRate,
                    parityNumber: (Parity)parity,
                    dataBits: dataBits,
                    stopBits: (StopBits)stopBits,
                    handshakeType: (Handshake)handshake
                 );
            }
            catch (ArgumentException)
            {
                System.Windows.MessageBox.Show("Nie można się połączyć z danym portem");
                return;
            }
            catch (FileNotFoundException)
            {
                System.Windows.MessageBox.Show("Nie można się połączyć z danym portem, jesteś pewien, że nie został w trakcie rozłączony?");
                return;
            }

            
            // zmienne pomocnicze
            int waitMs;
            bool continueMeas = true;
            int deviceCounter = 1;





            System.Threading.Thread.Sleep(2000);

            if (AVGTextLabel.Visibility == Visibility.Visible)
            {
                if (int.Parse(AVGValue.Text) >= 1 && int.Parse(AVGValue.Text) <= 256)
                {
                    rsConnector.changeAVGInDevice(AVGValue.Text);
                    // obliczane dokładnie na podstawie testów oczekiwania na maszynie Gwinstek LCR 6300
                    // 1,5s dodane żeby upewnić się co do przewidywalności wyników
                    waitMs = (int)((int.Parse(AVGValue.Text) / 2.9090909 + 1.5) * 1000);
                }
                else
                {
                    returnToIdle(rsConnector, false, "Podano niepoprawną wartość AVG, poprawne wartości to liczby całkowite z zakresu od 1 do 256");
                    return;
                }
            }
            else
            {
                rsConnector.changeAVGInDevice("1");
                waitMs = 700;
            }

            System.Threading.Thread.Sleep(2000);


            System.Windows.MessageBox.Show("Rozpoczynanie mierzenia dla parametrów: Częstotliwości: " + Freq1.Text + " " + Freq2.Text + " " + Freq3.Text + " " + Freq4.Text);

            // główna pętla
            while (continueMeas)
            {

                // dwa sposoby wyjścia z pętli
                // 1 : test wielu kondenstatorów : wybór z okienka (if)
                // 2 : test pojedyńczego kondensatora : automatyczne wyjście (else)
                if (!(bool)SerialTest.IsChecked)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("Jeśli chcesz rozpocząć mierzenie kondensatora numer: " + deviceCounter + " kilknij OK, jeśli chcesz zakończyć mierzenie wciśnij Cancel", "Czy kontynuować?", MessageBoxButton.OKCancel);
                    if (result != MessageBoxResult.OK)
                        break;
                }
                else 
                {
                    System.Windows.MessageBox.Show("Proszę podłączyć kondensator do testu seryjnego");
                    continueMeas = false; 
                }


                // pobieranie i zapisywanie danych do csv
                try {

                    if (int.Parse(Cycles.Text) < 0)
                    {
                        returnToIdle(rsConnector, false, "Podana wartość dla cykli jest za mała, należy podać liczbę całkowitą większą od 0");
                        return;
                    }

                    for (int cycle = 0; cycle < int.Parse(Cycles.Text); cycle++)
                    {
                        foreach (string freq in frequencies)
                        {
                            
                            if (freq != "" && freq != "0" && !string.IsNullOrEmpty(freq))
                            {

                                uint freqNumber = RSCommunication.convertHz(freq);
                                if ((freqNumber < 10 && freqNumber != 0) || freqNumber > 300000)
                                {
                                    returnToIdle(rsConnector, false, "Podano niepoprawną waartość częstotliwości, Hz musi byc w zakresie od 10 do 300kHz");
                                    return;
                                }

                                System.Threading.Thread.Sleep(3000);
                                rsConnector.changeHzInDevice(freq);


                                decimal[] responseParams = new decimal[3];
                                try
                                {
                                    // pobieranie danych z urządzenia
                                    responseParams = rsConnector.getMeasurementParams(
                                    ModeList.Text,                                                                              // tryb pomiaru
                                    unitList.Text,                                                                              // mnożnik SI
                                    waitFetchMs: waitMs,                                                                       // odstęp czasowy
                                    addD: (DParameter.Visibility == Visibility.Hidden) ? false : DParameter.IsChecked == true   // parametr D
                                    );
                                }
                                catch (TimeoutException)
                                {
                                    returnToIdle(rsConnector, false, "Przekroczono czas oczekiwania na odpowiedź, czy jesteś pewny, że parametry połączenia się zgadzają?");
                                    return;
                                }
                                
                                 // zapis do pliku csv
                                fileHandler.writeCSV(responseParams, unitList.Text, freq, ModeList.Text, cycle + 1, AVGValue.Text, deviceCounter);
                            }
                        }
                    }
                    deviceCounter += 1;
                    returnToIdle(rsConnector, true);
                }
                catch (FormatException)
                {
                    returnToIdle(rsConnector, false, "Podano nienumeryczną lub niepoprawną wartość dla częstotliwości bądź cyklów!");
                    return;
                }
            }

            System.Windows.MessageBox.Show("Zakończono wszystkie testy");
        }

        private void Set_File_Path(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog browser = new FolderBrowserDialog();
            var result = browser.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                FilePath.Text = browser.SelectedPath;
            }
        }

        private void Change_Mode(object sender, SelectionChangedEventArgs e)
        {
            string selectedMode = ModeList.SelectedItem.ToString();

            if (selectedMode.Contains("-D"))
                DParameter.Visibility = Visibility.Hidden;
            else
                DParameter.Visibility = Visibility.Visible;


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
            AVGTextLabel.Visibility = Visibility.Visible;
            AVGValue.Visibility = Visibility.Visible;
            AVGValue.Text = fileHandler.currentSettings.AVG;
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