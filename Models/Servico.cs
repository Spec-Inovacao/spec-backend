namespace YourNamespace.Models
{
    public class Servico
    {
        public int id { get; set; }
        public string nome { get; set; }
        public string descricao { get; set; }
        public int tempomin { get; set; }
        public decimal preco { get; set; }
        public bool ativo { get; set; }
    }
}