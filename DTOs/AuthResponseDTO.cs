namespace YourNamespace.DTOs
{
    public class AuthResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEm { get; set; }
        public bool Admin { get; set; }
        public ListUsuarioDTO Usuario { get; set; } = new();
    }
}