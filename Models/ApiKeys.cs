using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TerraNotes.Models
{
    [Table("api_keys")]
    public class APIKey
    {
        [Column("key")]
        [Key]
        public Guid Key { get; set; }
        [Column("status")]
        public required string Status { get; set; }

        // Custom foreign key
        [Column("user_created")]
        [ForeignKey("user_created")]
        public required User UserCreated { get; set; }
        [Column("date_created")]
        public DateTime DateCreated { get; set; }
        [Column("user_updated")]
        [ForeignKey("user_updated")]
        public User? UserUpdated { get; set; }
        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }
        [Column("uses")]
        public int Uses { get; set; }
        [Column("max_uses")]
        public int MaxUses { get; set; }
    }
}