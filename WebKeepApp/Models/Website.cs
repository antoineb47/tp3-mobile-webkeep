using System;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace WebKeepApp.Models
{
    [Table("Website")]
    public class Website
    {
        // Parameterless constructor needed by SQLite-net.
        public Website() { }

        // Constructor for enforcing the correct format of the object.
        public Website(int userId, string name, string url, string? notes = null)
        {
            if (userId < 0)
                throw new ArgumentException("UserId must be greater than -1.", nameof(userId));

            UserId = userId;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            DateCreatedAt = DateTime.UtcNow;
            Notes = notes;
            Id = Guid.NewGuid().ToString();
        }

        [PrimaryKey]
        [Column("Guid")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [NotNull]
        public string? Name { get; set; }

        [NotNull]
        public string? Url { get; set; }

        [NotNull]
        public DateTime DateCreatedAt { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }

        [NotNull]
        [Indexed]
        [ForeignKey(typeof(User))]
        public int UserId { get; set; }

        [ManyToOne]
        public User? User { get; set; } //Navigation property
    }
}
