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

        public static readonly string[] measurementTypes = {

            "Cs-Rs", "Cs-D",
            "Cp-Rp", "Cp-D",
            "Lp-Rp", "Lp-Q",
            "Ls-Rs", "Ls-Q",
            "Rs-Q", "Rp-Q",
            "R-X", "DCR",
            "Z-0r","Z-0d",
            "Z-D", "Z-Q"
        };

        public RSCommunication(string portName, uint baudRate, Parity parityNumber, uint dataBits, StopBits stopBits, Handshake handshakeType, int readTimeout = 30000, int writeTimeout = 30000)
        {
            _serialPort = new SerialPort();

            _serialPort.PortName = portName;
            _serialPort.DataBits = (int)dataBits;
            _serialPort.BaudRate = (int)baudRate;
            _serialPort.ReadTimeout = readTimeout;
            _serialPort.WriteTimeout = writeTimeout;
            _serialPort.Handshake = handshakeType;
            _serialPort.Parity = parityNumber;
            _serialPort.StopBits = stopBits;
            _serialPort.RtsEnable = true;

            if (_serialPort.IsOpen)
                _serialPort.Close();

            _serialPort.Open();

            if (!_serialPort.IsOpen)
                throw new Exception("Nie udało otworzyć się portu o takich parametrach");

            _serialPort.WriteLine("SYST:CODE OFF");
            System.Threading.Thread.Sleep(300);
            _serialPort.WriteLine("DISP:PAGE meas");
            System.Threading.Thread.Sleep(300);
        }





        public decimal[] getMeasurementParams(string msType, string multiplier, bool addD = false, int waitFetchMs = 700)
        {

            string d_Parameter = null;
            decimal[] responseDecimalArray = new decimal[3];


            // dodatkowe opcjonalne mierzenie parametru D
            if (addD)
            {
                setMeasurementInDevice("Cs-D");
                System.Threading.Thread.Sleep(waitFetchMs);
                _serialPort.WriteLine("FETCH?");
                d_Parameter = _serialPort.ReadLine().Split(",")[1].Replace(".", ",");
            }

            // mierzenie głównych parametrów
            setMeasurementInDevice(msType);
            System.Threading.Thread.Sleep(waitFetchMs);
            _serialPort.WriteLine("FETCH?");

            try
            {
                responseDecimalArray[0] = decimal.Parse(_serialPort.ReadLine().Split(",")[0].Replace(".", ","), NumberStyles.Float);
                responseDecimalArray[1] = decimal.Parse(_serialPort.ReadLine().Split(",")[1].Replace(".", ","), NumberStyles.Float);
                responseDecimalArray[2] = (addD) ? decimal.Parse(d_Parameter, NumberStyles.Float) : -1;
            }

            // FormatException to błąd pomiaru, kiedy tryb jest ustawiony na DCR jest to SZCZEGÓLNY przypadek a nie błąd
            catch (FormatException)
            {
                if (msType == "DCR")
                    responseDecimalArray[1] = -1;
                else
                    //TODO : dodaj potem logikę ponownego sprawdzenia
                    throw new Exception("wystąpił błąd pomiaru, ponawianie mierzenia...");
            }


            // dodawanie mnożników, jeśli jednostka pomiaru pozwala na to
            // dodatkowe zabezpieczenie oprócz GUI
            if (msType.Contains("Cs") || msType.Contains("Cp") || msType.Contains("Ls") || msType.Contains("Lp"))
            {
                responseDecimalArray[0] = applyMulitplier(responseDecimalArray[0], multiplier);
            }

            // element [0] to główny parametr pomiaru, nigdy nie powinien być nullem lub -1
            // element [1] to drugi główny parametr pomiaru, w przypadku trybu DCR jest równy -1
            // element [2] to opcjonalny parametr D, gdy użytkownik go nie podał jest równy -1
            return responseDecimalArray;
        }


        public decimal applyMulitplier(decimal paramValue, string multiplier)
        {
            return multiplier switch
            {
                "p" => paramValue *= 1000000000000m,
                "n" => paramValue *= 1000000000m,
                "μ" => paramValue *= 1000000m,
                "m" => paramValue *= 1000m,
                _ => paramValue
            };

        }



        // funkcja zmieniająca częstotliwość w urządzeniu
        // UWAGA : liczby, które po przemnożeniu przez 1000 nadal są zmiennoprzecinkowe są zaokrąglane
        public void changeHzInDevice(string HzString)
        {
            uint Hz;

            try
            {
                if (HzString.Contains("k") || HzString.Contains("K"))
                    Hz = Convert.ToUInt32(decimal.Round(Convert.ToDecimal(HzString.Remove(HzString.Length - 1).Replace(".", ",")) * 1000));
                else
                    Hz = Convert.ToUInt32(HzString);
            }
            catch (FormatException)
            {
                throw new Exception("Podano nienumeryczną wartość dla Hz!");
            }


            /*uint Hz;
            bool isNumeric = uint.TryParse(HzString, out Hz);*/

            //if (!isNumeric) throw new Exception("Podano nienumeryczną lub ujemną wartość dla uśredniania Hz");
            if (Hz < 10) throw new Exception("Podano za małą wartość dla Hz! (min 10Hz)");
            if (Hz > 300000) throw new Exception("Podano za dużą wartość dla Hz! (maks 30kHz)");

            string command = "FREQ " + Hz;
            _serialPort.WriteLine(command);
        }



        public void setMeasurementInDevice(string command)
        {

            if (!measurementTypes.Contains(command))
                throw new Exception(string.Format("Podano błędną wartość dla funkcji mierzenia! ({0})", command));

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


        // powinno być używane tylko i wyłącznie po zakończeniu wszystkich operacji!
        public void unlockKeypadInDevice()
        {
            _serialPort.WriteLine("SYST:KEYLOCK OFF");
        }

        public void changeAVGInDevice(string avg)
        {
            uint avgN;
            bool isNumeric = uint.TryParse(avg, out avgN);

            if (!isNumeric) throw new Exception("Podano nienumeryczną wartość dla uśredniania (prawidłowe wartości to liczby całkowite od 1 do 256)");
            if (avgN > 256 || avgN < 1) throw new Exception("Podano złą wartość dla avg (prawidłowe wartości to liczby całkowite od 1 do 256)");

            _serialPort.WriteLine("aper " + avg);
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