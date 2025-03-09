using System;
using System.Text.Json.Serialization;

namespace BackupServer.Models
{
    public class BackupRequest
    {
        public int UserId { get; set; }

        [JsonPropertyName("id")]
        public Guid WebsiteId { get; set; }

        public string? Name { get; set; }

        public string? Url { get; set; }

        public string? Note { get; set; }

        public DateTime DateCreatedAt { get; set; }
    }
}
