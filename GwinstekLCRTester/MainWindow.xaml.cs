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
                TransSpeed.SelectedItem = baudRates[0];

                DataBit.ItemsSource = bits;
                DataBit.SelectedItem = bits[1];

                ComPorts.ItemsSource = ports;
                ComPorts.SelectedItem = ports[0];

            } catch (IndexOutOfRangeException) { 
                MessageBox.Show("Nie znaleziono portów COM");
                Close();
            };
        }

        private void Test_Data(object sender, RoutedEventArgs e)
        {
            var portName = ComPorts.Text;
            var baudRate = TransSpeed.Text == "" ? 11550 : uint.Parse(DataBit.Text);
            var dataBits = DataBit.Text == "" ? 8 : uint.Parse(DataBit.Text);
            var parity = (Parity)Enum.Parse(typeof(Parity),ParityList.Text);
            var stopBits = (StopBits)Enum.Parse(typeof(StopBits), StopBitsList.Text);
            var handshake = (Handshake)Enum.Parse(typeof(Handshake), Handshakes.Text);

            string[] frequencies = new string[]
            {
                Freq1.Text,
                Freq2.Text,
                Freq3.Text,
                Freq4.Text,
            };


            // Start the testing
            var rsConnector = new RSCommunication(
                portName: portName,
                baudRate: baudRate,
                parityNumber: parity,
                dataBits: dataBits,
                stopBits: stopBits,
                handshakeType: handshake
             );

            SendButton.Content = "Testuję...";
            foreach (string freq in frequencies)
            {
                if (freq != "")
                {
                    System.Threading.Thread.Sleep(3000);
                    rsConnector.changeHzInDevice(freq);
                    decimal[] result = rsConnector.getBasicParametricData(unitList.Text);
                    ResultBox.Text = result[0].ToString() + " " + result[1].ToString();
                    rsConnector.writeToCSV(result, unitList.Text);
                }
            }


            SendButton.Content = "Zbierz dane";
            rsConnector.closeCSV();
            System.Threading.Thread.Sleep(200);
            rsConnector.unlockKeypadInDevice();
            rsConnector.closePort();
        }
    }
}
