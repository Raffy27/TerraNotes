using System;

namespace TerraNotes.Models
{
    public class OminousTechApiAccess
    {
        public string Key { get; set; }
        public string Status { get; set; }
        public OminousTechUser UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public OminousTechUser UserUpdated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}