namespace YourNamespace.Models
{
    public class Usuario
    {
        public int id { get; set; }
        public string nome { get; set; }
        public string email { get; set; }
        public string telefone { get; set; }
        public bool admin { get; set; }
        public DateTime reccreatedon { get; set; }
        public string? senhahash { get; set; }
    }
}