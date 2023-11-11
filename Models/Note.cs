using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TerraNotes.Models
{
    [Table("tn_notes")]
    public class Note
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("status")]
        public required string Status { get; set; }

        [Column("user_created")]
        public Guid UserCreatedId { get; set; }

        [ForeignKey("UserCreatedId")]
        public required User UserCreated { get; set; }

        [Column("date_created")]
        public DateTime DateCreated { get; set; }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("content")]
        public string? Content { get; set; }
    }
}