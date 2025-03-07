using System;
using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace WebKeepApp.Models {

    [Table("User")]
    public class User {

        // Internal parameterless constructor for ORM usage only.
        public User() {
            Websites = [];
        }

        // Public constructor that enforces required fields.
        public User(string username, string password) {

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username must be provided", nameof(username));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password must be provided", nameof(password));

            Username = username;
            Password = password;
            Websites = [];
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        [Unique]
        public string? Username { get; set; }

        [NotNull]
        public string? Password { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)] 
        public List<Website> Websites { get; set; }
    }
}
