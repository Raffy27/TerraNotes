namespace TerraNotes.Models
{
    public class TypedFile
    {
        public string Path { get; set; }
        public string Type { get; set; }

        public TypedFile(string path, string type)
        {
            Path = path;
            Type = type;
        }
    }
}