using System.Text;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GwinstekLCRTester
{
    public class RSCommunication
    {


        private SerialPort _serialPort;
        private TextWriter writter;

        public readonly string[] measurementTypes = { ///0 is 0xE9

            "Cs-Rs", "Cs-D", "Cp-Rp",
            "Cp-D", "Lp-Q", "Ls-Rs",
            "Ls-Q", "Rs-Q", "Rp-Q",
            "R-X", "DCR", "Z-0r",
            "Z-thr", "Z-0d", "Z-thd",
            "Z-D", "Z-Q"

        };
       

        public static void connectToDevice(string portName, uint baudRate, Parity parityNumber, uint dataBits, StopBits stopBits, uint readTimeout = 500, uint writeTimeout = 500, Handshake handshakeType = Handshake.None)
        {
            _serialPort = new SerialPort();

            _serialPort.PortName = portName;
            _serialPort.DataBits = (int)dataBits;
            _serialPort.BaudRate = (int)baudRate;
            _serialPort.ReadTimeout = (int)readTimeout;
            _serialPort.WriteTimeout = (int)writeTimeout;

            try
            {
                _serialPort.Handshake = handshakeType;
                _serialPort.Parity = parityNumber;
                _serialPort.StopBits = stopBits;
            }
            catch (System.ArgumentOutOfRangeException e) { Console.WriteLine(e.Message); }


            _serialPort.Open();
            if (!_serialPort.IsOpen) throw new Exception("Nie udało otworzyć się portu o takich parametrach");
        }


        void writeToCSV(decimal[] paramArray)
        {
            string path = "C:\\test\\data.csv";
            if (!File.Exists(path))
            {
                File.Create(path).Close();
                if (writter == null) writter = File.AppendText(path);
                writter.WriteLine("\"parametr 1\",\"parametr 2\",\"czas pomiaru\"");
            }

            if (writter == null) writter = File.AppendText(path);
            writter.WriteLine("\"{0}\",\"{1}\",\"{2}\"", paramArray[0], paramArray[1], DateTime.Now);
            writter.Close();
        }



        decimal[] getBasicParametricData(string Querycommand)
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
            stringParams[0] = stringParams[0].Replace(".",",");
            stringParams[1] = stringParams[1].Replace(".", ",");
            returnParametricData[0] = Decimal.Parse(stringParams[0], NumberStyles.Float);
            returnParametricData[1] = Decimal.Parse(stringParams[1], NumberStyles.Float);

            return returnParametricData;
        } 


        /* dla liczb, które po przemnożeniu przez 1000 nadal mają liczby po przecinku funkcja je zaokrągla*/
        public void changeHzInDevice(string HzString)
        {
            int Hz = -1;
            try
            {
                Hz = (HzString.EndsWith("k") || HzString.EndsWith("K")) ? Convert.ToInt32(Convert.ToDecimal(HzString.Replace(".", ",").Remove(HzString.Length - 1)) * 1000) : Convert.ToInt32(HzString);
            } 
            catch(FormatException)
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


        public void setMeasurementInDevice(string command)
        {
            if (!measurementTypes.Contains(command)) throw new Exception(String.Format("Podano błędną wartość dla funkcji mierzenia! ({0})", command));

            command = command.Insert(0, "func ").ToLower().Replace("z-0r", "z-thr").Replace("z-0d","z-thd");
            _serialPort.WriteLine(command);
           
        }


        // powinno być używane tylko i wyłącznie po zakończeniu wszystkich operacji!
        public void unlockKeypadInDevice()
        {
            _serialPort.WriteLine("SYST:KEYLOCK OFF");
        }

        
    }
}
