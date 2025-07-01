using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CFDDNS
{
    public class GlobalSettings
    {
        public string Email { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public int UpdateIntervalMinutes { get; set; } = 10;
    }

    public class DomainConfig
    {
        public bool Enabled { get; set; } = true;
        public string Domain { get; set; } = "example.com";
        public string Type { get; set; } = "A"; // A or AAAA
        public string ZoneId { get; set; } = "";
        public string RecordId { get; set; } = "";

        // Per-domain credentials (optional, override global settings)
        public string? Email { get; set; }
        public string? ApiKey { get; set; }

        [JsonIgnore]
        public string Status { get; set; } = "待处理";

        [JsonIgnore]
        public string? CurrentRecordIp { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.MinValue;

        [JsonIgnore]
        public string? LastKnownIp { get; set; }
    }

    public class AppConfig
    {
        public GlobalSettings Global { get; set; } = new();
        public List<DomainConfig> Domains { get; set; } = new();
    }
} 