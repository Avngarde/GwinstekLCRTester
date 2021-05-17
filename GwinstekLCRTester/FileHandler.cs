using System.IO;
using Newtonsoft.Json;

namespace GwinstekLCRTester
{
    class FileHandler
    {
        private static string settingsPath = Directory.GetCurrentDirectory() + "/settings.json";

        private static TextWriter writer;

        public readonly Settings currentSettings; 



        public FileHandler()
        {

            if (!File.Exists(settingsPath))
            {
                currentSettings = createDefaultSettings();

                writer = File.CreateText(settingsPath);
                writer.Write(JsonConvert.SerializeObject(createDefaultSettings()));
                writer.Flush();
            }
            else
            {
                currentSettings = readSettings();
            }
        }

       public static Settings createDefaultSettings()
        {
            Settings defaultSettings = new Settings()
            {
                Freq1 = "120",
                Freq2 = "100k",
                Freq3 = "200k",
                Freq4 = "300k",

                Cycles = "1",
                AVG = "1",
                CSVPath = Directory.GetCurrentDirectory(),
                DChecked = false,
                SerialTestChecked = false,
                MultiplierUnit = "Podstawowa jednostka",
                MeasurmentType = "Cs-Rs",

                TransmissionSpeed = 115200,
                StopBit = System.IO.Ports.StopBits.None,
                HandShake = System.IO.Ports.Handshake.None,
                Parity = System.IO.Ports.Parity.None,
                DataBits = 8
            };

            return defaultSettings;
        }

        public static Settings readSettings()
        {
            if (!File.Exists(settingsPath))
            {
                createDefaultSettings();
            }
            string rawJson = File.ReadAllText(settingsPath);
            Settings currentSettings = JsonConvert.DeserializeObject<Settings>(rawJson);

            return currentSettings;
        } 

        public static void WriteNewSettings(Settings settings)
        {

            if (!File.Exists(settingsPath))
            {
                writer = File.CreateText(settingsPath);
            }

            writer.Write(JsonConvert.SerializeObject(settings));
            writer.Flush();

        }
    }
}