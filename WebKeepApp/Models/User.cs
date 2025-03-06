using System.Collections.Generic;
using SQLite;

namespace WebKeepApp.Models {

    [Table("User")]
    public class User {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string? Username { get; set; }
        public string? Password { get; set; }
        
        [Ignore]
        public List<Website> Websites { get; set; } = [];
    }
}