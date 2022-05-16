using FarmCraft.Community.Data.Converters;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FarmCraft.Community.Data.Entities.System
{
    public class FarmCraftLog
    {
        [Key]
        public string LogId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        [JsonConverter(typeof(LogLevelConverter))]
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
        public string? Source { get; set; }
        public string? Data { get; set; }
    }
}
