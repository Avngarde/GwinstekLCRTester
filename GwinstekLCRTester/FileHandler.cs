using System;
using System.IO;
using Newtonsoft.Json;

namespace GwinstekLCRTester
{
    class FileHandler
    {
        private string settingsPath = Directory.GetCurrentDirectory() + "/settings.json";

        private TextWriter writer = null;

        public readonly Settings currentSettings;


        public FileHandler()
        {

            if (!File.Exists(settingsPath))
            {
                writer = File.CreateText(settingsPath);
                currentSettings = createDefaultSettings();
                writer.Write(JsonConvert.SerializeObject(createDefaultSettings()));
                writer.Flush();
                writer.Dispose();
                writer.Close();
                writer = null;
            }
            else
            {
                currentSettings = readSettings();
            }

            if (!Directory.Exists(currentSettings.CSVPath))
                Directory.CreateDirectory(currentSettings.CSVPath);
        }

        public Settings createDefaultSettings()
        {
            Settings defaultSettings = new Settings()
            {
                Freq1 = "120",
                Freq2 = "100k",
                Freq3 = "200k",
                Freq4 = "300k",

                Cycles = "1",
                AVG = "1",
                CSVPath = Directory.GetCurrentDirectory()+"\\csv",
                DChecked = false,
                SerialTestChecked = false,
                MultiplierUnit = "Podstawowa jednostka",
                MeasurmentType = "Cs-Rs",

                TransmissionSpeed = 115200,
                StopBit = System.IO.Ports.StopBits.One,
                HandShake = System.IO.Ports.Handshake.None,
                Parity = System.IO.Ports.Parity.None,
                DataBits = 8
            };

            return defaultSettings;
        }

        public Settings readSettings()
        {
            if (!File.Exists(settingsPath))
            {
                createDefaultSettings();
            }
            string rawJson = File.ReadAllText(settingsPath);
            Settings currentSettings = JsonConvert.DeserializeObject<Settings>(rawJson);

            return currentSettings;
        }

        public void writeNewSettings(Settings settings)
        {
            writer = File.CreateText(settingsPath);
            writer.Write(JsonConvert.SerializeObject(settings));
            writer.Flush();
            writer.Dispose();
            writer.Close();
            writer = null;
        }

        public void writeCSV(decimal[] paramArray, string multiplier, string freq, string msType, int cyclesIterator, string avg = "1", int deviceIterator = 1)
        {

            string path = currentSettings.CSVPath + "/pomiary_" + DateTime.Now.ToString("dd-M-yyyy--HH-mm-ss") + ".csv";
            if (writer == null)
            {
                // tworzenie kolumn do pliku csv
                writer = File.AppendText(path);
                string csvColumns = "Numer urządzenia;Numer cyklu; AVG;";
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
                writer.WriteLine
                (
                    "{0};{1};{2};{3};{4};{5};{6};{7}",
                    deviceIterator,                                                                 // numer kondensatora
                    cyclesIterator,                                                                 // numer cyklu
                    (avg != "1") ? avg : "NIE",                                                     // mierzenie z parametrem avg
                    paramArray[0].ToString().Replace(",", "."),                                     // główny parametr pomiaru
                    paramArray[1].ToString().Replace(",", "."),                                     // drugi główny parametr pomiaru                       
                    (paramArray[2] != -1) ? paramArray[2].ToString().Replace(",", ".") : "NIE",     // dodatkowy parametr D
                    freq,                                                                           // częstotliwość pomiaru
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")                                     // czas pomiaru
                );
            }
            else
            {
                writer.WriteLine
                (
                    "{0};{1};{2};{3};{4};{5};{6}",
                    deviceIterator,                                                                 //numer kondensatora
                    cyclesIterator,                                                                 // numer cyklu
                    (avg != "1") ? avg : "NIE",                                                     // mierzenie z parametrem avg
                    paramArray[0] * 0.001m,                                                         // główny i jedyny dla DCR parametr pomiaru
                    (paramArray[2] == -1) ? "NIE" : paramArray[2].ToString().Replace(",", "."),     // dodatkowy parametr D
                    freq,                                                                           // częstotliwość pomiaru
                    DateTime.Now.ToString("dd-M-yyyy HH:mm:ss")                                     // czas pomiaru
                );
            }
        }

        public void closeWriter()
        {
            writer.Flush();
            writer.Close();
            writer.Dispose();
            writer = null;
        }
    }
}