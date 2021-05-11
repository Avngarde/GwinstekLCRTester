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
                "F",
                "nF",
                "mF",
                "μF"
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

            } catch (IndexOutOfRangeException) { 
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
            var portName = ComPorts.Text;
            var baudRate = TransSpeed.Text == "" ? 115200 : uint.Parse(TransSpeed.Text);
            var dataBits = DataBit.Text == "" ? 8 : uint.Parse(DataBit.Text);
            var parity = (Parity)Enum.Parse(typeof(Parity), ParityList.Text);
            var stopBits = (StopBits)Enum.Parse(typeof(StopBits), StopBitsList.Text);
            var handshake = (Handshake)Enum.Parse(typeof(Handshake), Handshakes.Text);

            string[] frequencies = new string[]
            {
                Freq1.Text,
                Freq2.Text,
                Freq3.Text,
                Freq4.Text,
            };

            RSCommunication rsConnector = new RSCommunication(
                portName: portName,
                baudRate: baudRate,
                parityNumber: parity,
                dataBits: dataBits,
                stopBits: stopBits,
                handshakeType: handshake
             );

            rsConnector.changeMon1InDevice(DParameter.IsChecked == true);

            foreach (string freq in frequencies)
            {
                if (freq != "" || freq != "0")
                {
                    System.Threading.Thread.Sleep(1000);
                    rsConnector.changeHzInDevice(freq);
                    decimal resultD = -1;
                    decimal[] result = rsConnector.getBasicParametricData(unitList.Text);
                    if (DParameter.IsChecked == true)
                    {
                        resultD = rsConnector.getparameterDInDevice();
                    }
                    
                    rsConnector.writeToCSV(result, unitList.Text, freq, resultD);
                }
            }

            rsConnector.closeCSV();
            System.Threading.Thread.Sleep(200);
            rsConnector.unlockKeypadInDevice();
            rsConnector.closePort();
            System.Windows.MessageBox.Show("Wykonano wszystkie testy");
            SendButton.Content = "Wykonaj test";
        }

        private void Test_Data(object sender, RoutedEventArgs e)
        {
            ChangeSendButtonText(false); // Change button text during executing tests
<<<<<<< HEAD
            System.Threading.Thread.Sleep(300);
            if (SendButton.Content == "Wykonywanie...")
            {
                ExecuteTests();
            }
=======
            ExecuteTests();
>>>>>>> 55b035d1ffa9d819b1aad6ff9e315f080241fe82
            ChangeSendButtonText(true); // Change button after tests are finished
        }

        private void Set_File_Path(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog browser = new System.Windows.Forms.FolderBrowserDialog();
            var result = browser.ShowDialog();
            if(result.ToString() != String.Empty)
            {
                FilePath.Text = browser.SelectedPath;
            }
        }
    }
}
