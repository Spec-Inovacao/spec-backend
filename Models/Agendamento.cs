namespace YourNamespace.Models
{
    public class Agendamento
    {
        public int id { get; set; }
        public int? clienteid { get; set; }
        public int servicoid { get; set; }
        public DateTime dtinicio { get; set; }
        public DateTime dtfim { get; set; }
        public string status { get; set; } = "agendado";
        public DateTime reccreatedon { get; set; }
    }
}