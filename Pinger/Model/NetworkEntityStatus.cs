namespace Pinger.Model
{
    public class NetworkEntityStatus
    {
        public string Address { get; set; }
        public string Name { get; set; }
        public string Mac { get; set; }
        public string Status { get; set; }
        public long RoundtripTime { get; set; }
    }
}