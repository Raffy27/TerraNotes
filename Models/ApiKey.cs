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

        // Have the id of the user as a foreign key, use it separately
        [Column("user_created")]
        public Guid UserCreatedId { get; set; }

        [ForeignKey("UserCreatedId")]
        public required User UserCreated { get; set; }

        [Column("date_created")]
        public DateTime DateCreated { get; set; }

        [Column("user_updated")]
        public Guid? UserUpdatedId { get; set; }

        [ForeignKey("UserUpdatedId")]
        public User? UserUpdated { get; set; }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }

        [Column("uses")]
        public int Uses { get; set; }
        
        [Column("max_uses")]
        public int MaxUses { get; set; }
    }
}