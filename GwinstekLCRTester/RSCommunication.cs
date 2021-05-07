using System;
using System.IO.Ports;
using System.Text;
using System.Globalization;
using System.IO;


namespace GwinstekLCRTester
{
    public class RSCommunication
    {
        private static SerialPort _serialPort;
        private static TextWriter writter;


        public static void connectToDevice(string portName, uint baudRate, uint parityNumber, uint dataBits, uint stopBits, uint readTimeout = 500, uint writeTimeout = 500, uint handshakeType = 0)
        {
            _serialPort = new SerialPort();

            _serialPort.PortName = portName;
            _serialPort.DataBits = (int)dataBits;
            _serialPort.BaudRate = (int)baudRate;
            _serialPort.ReadTimeout = (int)readTimeout;
            _serialPort.WriteTimeout = (int)writeTimeout;

            try
            {
                _serialPort.Handshake = (Handshake)handshakeType;
                _serialPort.Parity = (Parity)parityNumber;
                _serialPort.StopBits = (StopBits)stopBits;
            }
            catch (System.ArgumentOutOfRangeException e) { Console.WriteLine(e.Message); }


            _serialPort.Open();
            if (!_serialPort.IsOpen) throw new Exception("Nie udało otworzyć się portu o takich parametrach");
        }


        public static void writeToCSV(decimal[] paramArray)
        {
            string path = "C:\\test\\data.csv";
            if (!File.Exists(path))
            {
                File.Create(path).Close();
                if (writter == null) writter = File.AppendText(path);
                writter.WriteLine("\"pF\",\"MΩ\",\"Czas pomiaru\"");
            }

            if (writter == null) writter = File.AppendText(path);
            writter.WriteLine("\"{0}\",\"{1}\",\"{2}\"", paramArray[0], paramArray[1], DateTime.Now);
            writter.Close();
        }

        public static void changeHzInDevice(string HzString)
        {
            int Hz = -1;
            try
            {
                Hz = (HzString.EndsWith("k") || HzString.EndsWith("K")) ? Convert.ToInt32(Convert.ToDecimal(HzString.Replace(".", ",").Remove(HzString.Length - 1)) * 1000) : Convert.ToInt32(HzString);
            }
            catch (FormatException)
            {
                throw new Exception("Podano wartość dla Hz w złej formie!");
            }


            if (Hz < 0) throw new Exception("Podano liczbę ujemną dla Hz!");
            if (Hz == 0) throw new Exception("Liczba Hz nie może być równa 0 !");
            if (Hz < 10) throw new Exception("Podano za małą wartość dla Hz! (min 10Hz)");
            if (Hz > 30000) throw new Exception("Podano za dużą wartość dla Hz! (maks 30kHz)");

            string command = "FREQ " + Hz;
            _serialPort.WriteLine(command);
        }


        public static decimal[] getBasicParametricData(string Querycommand) // fetch? < = Basic test command
        {

            _serialPort.WriteLine(Querycommand.ToLower());
            byte[] rawBuffer = new byte[1024];
            StringBuilder outputMessage = new StringBuilder();
            decimal[] returnParametricData = new decimal[2];

            _serialPort.Read(rawBuffer, 0, 1024);
            for (int i = 0; i < rawBuffer.Length; i++)
            {
                if (rawBuffer[i] == 13) break;
                outputMessage.Append(Convert.ToChar(rawBuffer[i]));
            }

            string[] stringParams = outputMessage.ToString().Split(",");
            stringParams[0] = stringParams[0].Replace(".", ",");
            stringParams[1] = stringParams[1].Replace(".", ",");
            returnParametricData[0] = Decimal.Parse(stringParams[0], NumberStyles.Float);
            returnParametricData[1] = Decimal.Parse(stringParams[1], NumberStyles.Float);

            return returnParametricData;
        }
    }
}