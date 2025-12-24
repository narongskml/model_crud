namespace PortModelApi.Models
{
    public class HttpClientOptions
    {
        public bool UseProxy { get; set; }
        public string? ProxyUrl { get; set; }
        public bool BypassOnLocal { get; set; } = true;
    }
}
