using System.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.IO.Ports;
using System.Runtime;
using System;

namespace GwinstekLCRTester
{
    public class RSCommunication
    {


        private static SerialPort _serialPort;
        private static TextWriter writer;

        public static readonly string[] measurementTypes = { ///0 is 0xE9

            "Cs-Rs", "Cs-D", "Cp-Rp",
            "Cp-D", "Lp-Q", "Ls-Rs",
            "Ls-Q", "Rs-Q", "Rp-Q",
            "R-X", "DCR", "Z-0r",
            "Z-thr", "Z-0d", "Z-thd",
            "Z-D", "Z-Q"

        };



        public string PortName { get; }
        public uint BaudRate { get; }
        public Parity ParityNumber { get; }
        public uint DataBits { get; }
        public StopBits StopBits { get; }
        public Handshake HandshakeType { get; }

        public RSCommunication(string portName, uint baudRate, Parity parityNumber, uint dataBits, StopBits stopBits, Handshake handshakeType, int readTimeout = 3000, int writeTimeout = 3000)
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

            _serialPort.RtsEnable = true;
            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.Open();
            if (!_serialPort.IsOpen) throw new Exception("Nie udało otworzyć się portu o takich parametrach");
            _serialPort.WriteLine("SYST:CODE OFF");
        }



        // paramArray[0] i [1] są głównymi pomiarami zależnymi od trybu, [2] jest opcjonalnym D
        public void writeToCSV(decimal[] paramArray, string multiplierFarad, string freq, string msType, string pathOutput)
        {
            string path = pathOutput.Replace(@"\", @"\\").Replace("\r\n", "") + "\\pomiary_" + DateTime.Now.ToString("dd-M-yyyy--HH-mm-ss") + ".csv";

            if (writer == null)
            {
                
                writer = File.AppendText(path);
                // ustawianie odpowiednich kolumn w zależności od trybu pomiar
                //przetestuj dla trybu DCR
                string csvColumns = msType switch
                {
                    "Cs-Rs" => string.Format("\"Cs ({0})\", \"Rs (Ω)\"", multiplierFarad),
                    "Cs-D" => string.Format("\"Cs ({0})\", \"D\"", multiplierFarad),
                    "Cp-Rp" => string.Format("\"Cp ({0})\", \"Rp (Ω)\"", multiplierFarad),
                    "Cp-D" => string.Format("\"Cp ({0})\", \"D\"", multiplierFarad),
                    "Lp-Rp" => "\"Lp (H)\", \"Rp (Ω)\"",
                    "Lp-Q" => "\"Lp (H)\", \"Q\"",
                    "Ls-Rs" => "\"Ls (H)\", \"Rs (Ω)\"",
                    "Ls-Q" => "\"Ls (H)\", \"Q\"",
                    "Rs-Q" => "\"Rs (Ω)\", \"Q\"",
                    "Rp-Q" => "\"Rp (Ω)\", \"Q\"",
                    "R-X" => "\"R (Ω)\", \"X (Ω)\"",
                    "DCR" => "\"DCR (Ω)\"",
                    "Z-0r" => "\"Z (Ω)\", \"0 (r)\"",
                    "Z-thr" => "\"Z (Ω)\", \"0 (r)\"",
                    "Z-0d" => "\"Z (Ω)\", \"0 (º)\"",
                    "Z-thd" => "\"Z (Ω)\", \"0 (º)\"",
                    "Z-D" => "\"Z (Ω)\", \"D\"",
                    "Z-Q" => "\"Z (Ω)\", \"Q\"",
                    _ => throw new NotImplementedException()
                };

                csvColumns += ",\"D\", \"częstotliwość (Hz)\", \"czas pomiaru\"";
                writer.WriteLine(csvColumns);
            }
            else
            {
                // 0 i 1 paramArray, 2 to D, 3 to freq, 4 to data
                writer.WriteLine("\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\"", paramArray[0], paramArray[1], (paramArray[2] == -1) ? "-" : paramArray[2], freq, DateTime.Now.ToString("dd-M-yyyy--HH-mm-ss"));
            }

        }

        public void closeCSV()
        {
            writer.Close();
        }

        public void closePort()
        {
            _serialPort.Close();
        }



        public decimal[] testFullParams(string msType, string multiplier = "μ", bool addD = false)
        {
            string[] responseStringArray = new string[3];
            decimal[] responseDecimalArray = new decimal[3];


            setMeasurementInDevice(msType);
            _serialPort.WriteLine("FETCH?");

            responseStringArray[0] = _serialPort.ReadLine().Split(",")[0].Replace(".", ",");
            responseStringArray[1] = _serialPort.ReadLine().Split(",")[1].Replace(".", ",");


            if (addD)
            {

                setMeasurementInDevice("Cs-D");
                _serialPort.WriteLine("FETCH?");
                responseStringArray[2] = _serialPort.ReadLine().Split(",")[1].Replace(".", ",");
            }


            // responseDecimalArray czasami ostatni element ma równy null
            for (int i = 0; i < responseStringArray.Length; i++)
            {
                try
                {
                    responseDecimalArray[i] = decimal.Parse(responseStringArray[i], NumberStyles.Float);
                }
                catch (System.ArgumentNullException)
                {
                    responseDecimalArray[i] = -1;
                }

            }



            // jeżeli wybierzemy tryb z pojemnością, to Farady są zawsze na pierwszym miejscu
            if (msType.Contains("Cs"))
            {
                switch (multiplier)
                {
                    case "p":
                        responseDecimalArray[0] *= 1000000000000m;
                        break;
                    case "n":
                        responseDecimalArray[0] *= 1000000000m;
                        break;
                    case "μ":
                        responseDecimalArray[0] *= 1000000m;
                        break;
                    case "m":
                        responseDecimalArray[0] *= 1000m;
                        break;
                }
            }

            return responseDecimalArray;
        }


        /* dla liczb, które po przemnożeniu przez 1000 nadal mają liczby po przecinku funkcja je zaokrągla*/
        public void changeHzInDevice(string HzString)
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
            if (Hz < 10 && Hz != 0) throw new Exception("Podano za małą wartość dla Hz! (min 10Hz)");
            if (Hz > 300000) throw new Exception("Podano za dużą wartość dla Hz! (maks 30kHz)");

            string command = "FREQ " + Hz;
            _serialPort.WriteLine(command);
        }



        public void setMeasurementInDevice(string command)
        {
            if (!measurementTypes.Contains(command)) throw new Exception(string.Format("Podano błędną wartość dla funkcji mierzenia! ({0})", command));
            _serialPort.DiscardInBuffer();
            _serialPort.Dispose();
            _serialPort.Close();
            _serialPort.Open();
            System.Threading.Thread.Sleep(500);
            command = command.Insert(0, "FUNC ").Replace("z-0r", "z-thr").Replace("z-0d", "z-thd");
            _serialPort.WriteLine(command);
            _serialPort.DiscardInBuffer();
            _serialPort.Dispose();
            _serialPort.Close();
            _serialPort.Open();
            System.Threading.Thread.Sleep(500);

        }


        // powinno być używane tylko i wyłącznie po zakończeniu wszystkich operacji!
        public void unlockKeypadInDevice()
        {
            _serialPort.WriteLine("SYST:KEYLOCK OFF");
        }


    }
}