namespace YourNamespace.Models
{
    public class Configuracao
    {
        public int id { get; set; }
        public TimeSpan horainicio { get; set; }
        public TimeSpan horafim { get; set; }
        public int cancelamentominhora { get; set; }
        public int[] diasatendimento { get; set; }
    }
}