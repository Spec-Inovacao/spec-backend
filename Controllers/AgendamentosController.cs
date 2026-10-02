using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourNamespace.Data;
using YourNamespace.DTOs;
using YourNamespace.Mappers;
using YourNamespace.Models;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgendamentosController : ControllerBase
    {
        private readonly YourDbContext _context;

        public AgendamentosController(YourDbContext context)
        {
            _context = context;
        }

        // Npgsql só aceita DateTime UTC em colunas timestamptz.
        // Sem 'Z'/offset no JSON o Kind chega Unspecified: tratamos como UTC.
        private static DateTime ParaUtc(DateTime data) => data.Kind switch
        {
            DateTimeKind.Utc => data,
            DateTimeKind.Local => data.ToUniversalTime(),
            _ => DateTime.SpecifyKind(data, DateTimeKind.Utc)
        };

        // POST: /api/agendamentos
        [HttpPost]
        public async Task<IActionResult> InsertAgendamento([FromBody] InsertAgendamentoDTO dto)
        {
            var servico = await _context.pservicos.FindAsync(dto.ServicoId);
            if (servico == null || !servico.ativo)
                return BadRequest("Serviço inválido ou inativo.");

            var cliente = await _context.pusuarios.FindAsync(dto.ClienteId);
            if (cliente == null)
                return BadRequest("Cliente não encontrado.");

            var dtInicio = ParaUtc(dto.DtInicio);
            var dtFim = dtInicio.AddMinutes(servico.tempomin);

            // Validações de conflito
            var conflito = await _context.pagendamentos.AnyAsync(a =>
                a.dtinicio < dtFim && a.dtfim > dtInicio && a.status == "agendado");
            if (conflito)
                return Conflict("Já existe um agendamento neste horário.");

            var agendamento = new Agendamento
            {
                clienteid = dto.ClienteId,
                servicoid = dto.ServicoId,
                dtinicio = dtInicio,
                dtfim = dtFim,
                status = "agendado",
                reccreatedon = DateTime.UtcNow
            };

            _context.pagendamentos.Add(agendamento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAgendamento), new { id = agendamento.id }, agendamento);
        }

        // GET: /api/agendamentos?clienteId=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListAgendamentoDTO>>> ListAgendamentos([FromQuery] int? clienteId)
        {
            if (!clienteId.HasValue)
                return BadRequest("O parâmetro 'clienteId' é obrigatório.");

            var agendamentos = await _context.pagendamentos
                .Where(a => a.clienteid == clienteId.Value)
                .ToListAsync();

            var result = new List<ListAgendamentoDTO>();
            foreach (var agendamento in agendamentos)
            {
                var cliente = await _context.pusuarios.FindAsync(agendamento.clienteid);
                var servico = await _context.pservicos.FindAsync(agendamento.servicoid);

                var podeCancelar = (agendamento.dtinicio - DateTime.UtcNow).TotalHours >= 2;

                result.Add(AgendamentoMapper.ToListDTO(agendamento, cliente?.nome, servico?.nome, podeCancelar));
            }

            return Ok(result);
        }

        // GET: /api/agendamentos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Agendamento>> GetAgendamento(int id)
        {
            var agendamento = await _context.pagendamentos.FindAsync(id);
            if (agendamento == null)
                return NotFound();

            return Ok(agendamento);
        }

        // PATCH: /api/agendamentos/{id}/cancelar
        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> CancelAgendamento(int id)
        {
            var agendamento = await _context.pagendamentos.FindAsync(id);
            if (agendamento == null)
                return NotFound();

            if ((agendamento.dtinicio - DateTime.UtcNow).TotalHours < 2)
                return BadRequest("Cancelamento bloqueado.");

            agendamento.status = "cancelado";
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}