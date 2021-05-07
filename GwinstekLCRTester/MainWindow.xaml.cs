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

namespace GwinstekLCRTester
{
    public partial class MainWindow : Window
    {
        static SerialPort serialPort;

        public MainWindow()
        {
            InitializeComponent();

            // Set data for connection input
            string[] ports = SerialPort.GetPortNames();

            Parity[] parities = new Parity[]
            {
                Parity.None,
                Parity.Odd,
                Parity.Even,
                Parity.Space,
                Parity.Mark
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

                ComPorts.ItemsSource = ports;
                ComPorts.SelectedItem = ports[0];
            } catch (IndexOutOfRangeException e) { 
                MessageBox.Show("Nie znaleziono portów COM");
                // Close(); // Exits program if no COM ports found
            };
        }

        private void Test_Data(object sender, RoutedEventArgs e)
        {
            var portName = "COM1";
            var transmissionSpeed = int.Parse(TransSpeed.Text);
            var dataBits = int.Parse(DataBit.Text);
            var parity = ParityList.Text;
            var stopBits = StopBitsList.Text;
            var handshake = Handshakes.Text;

            string[] frequencies = new string[]
            {
                Freq1.Text,
                Freq2.Text,
                Freq3.Text,
                Freq4.Text,
            };

            foreach(string freq in frequencies)
            {
                RSCommunication.changeHzInDevice(freq);
            }
        }
    }
}
