using System.Text.Json.Serialization;

namespace IPrazo.Crowler.Models;

public class Proxy
{
    public string IpAddress { get; set; } = "";
    public string Port { get; set; } = "";
    public string Country { get; set; } = "";
    public string Protocol { get; set; } = "";

    [JsonIgnore]
    public int PageNumber { get; set; }

    public override string ToString()
    {
        return $"{IpAddress}:{Port} | {Protocol} | {Country}";
    }
}