using YourNamespace.DTOs;
using YourNamespace.Models;

namespace YourNamespace.Mappers
{
    public static class AgendamentoMapper
    {
        public static ListAgendamentoDTO ToListDTO(Agendamento agendamento, string nomeCliente, string nomeServico, bool podeCancelar)
        {
            return new ListAgendamentoDTO
            {
                Id = agendamento.id,
                NomeCliente = nomeCliente,
                NomeServico = nomeServico,
                DtInicio = agendamento.dtinicio,
                DtFim = agendamento.dtfim,
                Status = agendamento.status,
                PodeCancelar = podeCancelar
            };
        }
    }
}