using System.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.IO.Ports;
using System.Runtime;
using System;

namespace GwinstekLCRTester
{
    public class RSCommunication : IDisposable
    {


        private static SerialPort _serialPort;
        private static TextWriter writer;

        public static readonly string[] measurementTypes = { ///0 is 0xE9

            "Cs-Rs", "Cs-D", "Cp-Rp",
            "Cp-D","Lp-Rp", "Lp-Q", "Ls-Rs",
            "Ls-Q", "Rs-Q", "Rp-Q",
            "R-X", "DCR", "Z-0r","Z-0d",
            "Z-D", "Z-Q"

        };



        public string PortName { get; }
        public uint BaudRate { get; }
        public Parity ParityNumber { get; }
        public uint DataBits { get; }
        public StopBits StopBits { get; }
        public Handshake HandshakeType { get; }

        public RSCommunication(string portName, uint baudRate, Parity parityNumber, uint dataBits, StopBits stopBits, Handshake handshakeType, int readTimeout = 30000, int writeTimeout = 30000)
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
            _serialPort.WriteLine("DISP:PAGE meas");
        }



        // paramArray[0] i [1] są głównymi pomiarami zależnymi od trybu, [2] jest opcjonalnym D
        public void writeToCSV(decimal[] paramArray, string multiplier, string freq, string msType, string pathOutput, int cyclesIterator = 0)
        {
            string path = pathOutput.Replace(@"\", @"\\").Replace("\r\n", "") + "\\pomiary_" + DateTime.Now.ToString("dd-M-yyyy--HH-mm-ss") + ".csv";

            if (writer == null)
            {

                writer = File.AppendText(path);
                // ustawianie odpowiednich kolumn w zależności od trybu pomiar
                string csvColumns = "Numer cyklu;";
                csvColumns += msType switch
                {
                    "Cs-Rs" => string.Format("Cs ({0}F);Rs (Ω)", (multiplier == "Podstawowa jednostka") ? "" : multiplier),
                    "Cs-D" => string.Format("Cs ({0}F);D", (multiplier == "Podstawowa jednostka") ? "" : multiplier),
                    "Cp-Rp" => string.Format("Cp ({0}F);Rp (Ω)", (multiplier == "Podstawowa jednostka") ? "" : multiplier),
                    "Cp-D" => string.Format("Cp ({0}F);D", (multiplier == "Podstawowa jednostka") ? "" : multiplier),
                    "Lp-Rp" => string.Format("Lp ({0}H);Rp (Ω)", (multiplier == "Podstawowa jednostka") ? "" : multiplier),
                    "Lp-Q" => string.Format("Lp ({0}H);Q", (multiplier == "Podstawowa jednostka") ? "" : multiplier),
                    "Ls-Rs" => string.Format("Ls ({0}H);Rp (Ω)", (multiplier == "Podstawowa jednostka") ? "" : multiplier),
                    "Ls-Q" => string.Format("Ls ({0}H);Q", (multiplier == "Podstawowa jednostka") ? "" : multiplier),
                    "Rs-Q" => "Rs (Ω);Q",
                    "Rp-Q" => "Rp (Ω);Q",
                    "R-X" => "R (Ω);X (Ω)",
                    "DCR" => "DCR (kΩ)",
                    "Z-0r" => "Z (Ω);0 (r)",
                    "Z-0d" => "Z (Ω);0 (º)",
                    "Z-D" => "Z (Ω);D",
                    "Z-Q" => "Z (Ω);Q",
                    _ => throw new NotImplementedException()
                };

                csvColumns += ";dodatkowo wybrany parametr D;częstotliwość (Hz);czas pomiaru";
                writer.WriteLine(csvColumns);
            }

            if (msType != "DCR")
            {
                // 0 to numer cyklu 1 i 2 paramArray, 2 to D, 3 to freq, 4 to data
                writer.WriteLine("{0};{1};{2};{3};{4};{5}", cyclesIterator, paramArray[0].ToString().Replace(",", "."), paramArray[1].ToString().Replace(",", "."), (paramArray[2] == -1) ? "__" : paramArray[2].ToString().Replace(",", "."), freq, DateTime.Now.ToString("dd-M-yyyy HH:mm:ss"));
            }
            else
            {
                writer.WriteLine("{0};{1};{2};{3};{4}", cyclesIterator, paramArray[0] * 0.001m, (paramArray[2] == -1) ? "__" : paramArray[2].ToString().Replace(",", "."), freq, DateTime.Now.ToString("dd-M-yyyy HH:mm:ss"));
            }



        }


        public decimal[] testFullParams(string msType, string multiplier = "μ", bool addD = false, int waitFetchMs = 0)
        {

            string[] responseStringArray = new string[3];
            decimal[] responseDecimalArray = new decimal[3];

            setMeasurementInDevice(msType);
            System.Threading.Thread.Sleep(waitFetchMs);
            _serialPort.WriteLine("FETCH?");

            responseStringArray[0] = _serialPort.ReadLine().Split(",")[0].Replace(".", ",");
            responseStringArray[1] = _serialPort.ReadLine().Split(",")[1].Replace(".", ",");

            if (addD)
            {
                setMeasurementInDevice("Cs-D");
                System.Threading.Thread.Sleep(waitFetchMs);
                _serialPort.WriteLine("FETCH?");
                responseStringArray[2] = _serialPort.ReadLine().Split(",")[1].Replace(".", ",");
            }

            // responseDecimalArray czasami ostatni element ma równy -1 to znaczy, że nie podano dodatkowego parametru D






            // FormatException to błąd pomiaru, kiedy tryb jest ustawiony na DCR jest to SZCZEGÓLNY przypadek a nie błąd
            for (int i = 0; i < responseStringArray.Length; i++)
            {
                try
                {
                    responseDecimalArray[i] = decimal.Parse(responseStringArray[i], NumberStyles.Float);
                }
                catch (ArgumentNullException)
                {
                    responseDecimalArray[i] = -1;
                }
                catch (FormatException)
                {
                    if (msType == "DCR")
                    {
                        responseDecimalArray[i] = -1;
                    }
                    else
                    {
                        throw new Exception("Błąd pomiaru, ponawianie testu z tymi samymi ustawieniami");
                    }
                }

            }

            // jeżeli mierzymy farady lub henry to możemy dodawać do nich mnożniki
            // dodatkowe zabezpieczenie oprócz GUI
            if (msType.Contains("Cs") || msType.Contains("Cp") || msType.Contains("Ls") || msType.Contains("Lp"))
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

            if (Hz < 10 && Hz != 0) throw new Exception("Podano za małą wartość dla Hz! (min 10Hz)");
            if (Hz < 0) throw new Exception("Podano liczbę ujemną dla Hz!");
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
            System.Threading.Thread.Sleep(100);
            command = command.Insert(0, "FUNC ").Replace("z-0r", "z-thr").Replace("z-0d", "z-thd");
            _serialPort.WriteLine(command);
            _serialPort.DiscardInBuffer();
            _serialPort.Dispose();
            _serialPort.Close();
            _serialPort.Open();
            System.Threading.Thread.Sleep(3000);
        }



        public bool checkDeviceConnected()
        {
            _serialPort.WriteLine("FETCH?");

            // interesują nas tylko ohmy stąd tylko [1] indeks
            decimal ohmResistance = decimal.Parse(_serialPort.ReadLine().Split(",")[1].Replace(".", ","), NumberStyles.Float);

            // tutaj jakiś warunek sprawdzający czy ohmy są wystrczająco duże 
            // jeżeli nie to niech wyświetli się komunikat czy na pewno podłączono urządzenie? (bardzo duża/mała rezystancja)
            return true;
        }





        // powinno być używane tylko i wyłącznie po zakończeniu wszystkich operacji!
        public void unlockKeypadInDevice()
        {
            _serialPort.WriteLine("SYST:KEYLOCK OFF");
        }

        public void changeAVGInDevice(string avg)
        {
            bool isNumeric = uint.TryParse(avg, out _);
            if (!isNumeric) throw new Exception("Podano nienumeryczną wartość dla uśredniania (prawidłowe wartości to liczby całkowite od 1 do 256)");

            uint avgN = uint.Parse(avg);
            if (avgN > 256 || avgN < 1) throw new Exception("Podano złą wartość dla avg (prawidłowe wartości to liczby całkowite od 1 do 256)");

            _serialPort.WriteLine("aper " + avg);
        }


        public void closeCSV()
        {
            writer.Close();
            writer = null;
        }

        public void closePort()
        {
            _serialPort.Close();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool v)
        {
            GC.SuppressFinalize(this);
        }
    }
}