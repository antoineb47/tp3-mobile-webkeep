using System;
using SQLite;

namespace WebKeepApp.Models {

    [Table("Website")]
    public class Website {

        [PrimaryKey]
        public Guid Guid { get; } = Guid.NewGuid();

        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? Username { get; set; }
        public string? DateCreateAt { get; set; }
        public string? Notes { get; set; }

        public int UserId { get; set; }
    }
}