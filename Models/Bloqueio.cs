namespace YourNamespace.Models
{
    public class Bloqueio
    {
        public int id { get; set; }
        public int? adminid { get; set; } // ID do administrador que criou o bloqueio
        public string motivo { get; set; } // Motivo do bloqueio
        public DateTime dtinicio { get; set; } // Data e hora de início do bloqueio
        public DateTime dtfim { get; set; } // Data e hora de fim do bloqueio
        public bool repdiautil { get; set; } // Indica se o bloqueio se repete em dias úteis
        public DateTime reccreatedon { get; set; } // Data de criação do registro
    }
}