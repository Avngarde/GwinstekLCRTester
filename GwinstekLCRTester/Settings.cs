using System.IO.Ports;

namespace GwinstekLCRTester
{
    class Settings
    {
        // Test data
        public string Freq1 { get; set; }
        public string Freq2 { get; set; }
        public string Freq3 { get; set; }
        public string Freq4 { get; set; }

        public string Cycles { get; set; }
        public string AVG { get; set; }

        public string CSVPath { get; set; }
        public bool DChecked { get; set; }
        public bool SerialTestChecked { get; set; }
        public string MultiplierUnit { get; set; }

        // Device connection data
        public string MeasurmentType { get; set; }
        public uint TransmissionSpeed { get; set; }
        public StopBits StopBit { get; set; }
        public Handshake HandShake { get; set; }
        public Parity Parity { get; set; }
        public uint DataBits { get; set; }
    }
}
