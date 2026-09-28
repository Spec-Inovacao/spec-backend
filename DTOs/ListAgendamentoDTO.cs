namespace YourNamespace.DTOs
{
    public class ListAgendamentoDTO
    {
        public int Id { get; set; }
        public string NomeCliente { get; set; }
        public string NomeServico { get; set; }
        public DateTime DtInicio { get; set; }
        public DateTime DtFim { get; set; }
        public string Status { get; set; }
        public bool PodeCancelar { get; set; }
    }
}