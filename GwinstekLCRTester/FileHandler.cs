using System.IO;
using Newtonsoft.Json;

namespace GwinstekLCRTester
{
    class FileHandler
    {
        private static string SettingFilePath = Directory.GetCurrentDirectory() + "/settings.json";
        private static TextWriter writer;

        public static Settings CreateDefaultSettings()
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

                TransmissionSpeed = 115200,
                StopBit = System.IO.Ports.StopBits.None,
                HandShake = System.IO.Ports.Handshake.None,
                Parity = System.IO.Ports.Parity.None,
                DataBits = 8
            };

            WriteNewSettings(defaultSettings);
            return defaultSettings;
        }

        public static Settings ReadSettings()
        {
            if (!File.Exists(SettingFilePath))
            {
                CreateDefaultSettings();
            }
            string rawJson = File.ReadAllText(SettingFilePath);
            Settings currentSettings = JsonConvert.DeserializeObject<Settings>(rawJson);

            return currentSettings;
        } 

        public static void WriteNewSettings(Settings settings)
        {
            string serializedSettings = JsonConvert.SerializeObject(settings);
            writer = File.CreateText(SettingFilePath);
            writer.Write(settings);
        }
    }
}