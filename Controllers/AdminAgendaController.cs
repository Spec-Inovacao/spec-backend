using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourNamespace.Data;
using YourNamespace.Models;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class AdminAgendaController : ControllerBase
    {
        private readonly YourDbContext _context;

        public AdminAgendaController(YourDbContext context)
        {
            _context = context;
        }

        private (DateTime inicioUtc, DateTime fimUtc) ObterIntervaloUtc(DateTime data)
        {
            var fusoHorario = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

            var inicioLocal = DateTime.SpecifyKind(
                data.Date,
                DateTimeKind.Unspecified
            );

            var fimLocal = inicioLocal.AddDays(1);

            var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(inicioLocal, fusoHorario);
            var fimUtc = TimeZoneInfo.ConvertTimeToUtc(fimLocal, fusoHorario);

            return (inicioUtc, fimUtc);
        }

        // GET: /api/admin/agenda?data=
        [HttpGet]
        public async Task<IActionResult> GetAgendaDia([FromQuery] DateTime data)
        {
            var (inicioUtc, fimUtc) = ObterIntervaloUtc(data);

            var agendamentos = await _context.pagendamentos
                .Where(a => a.dtinicio >= inicioUtc && a.dtinicio < fimUtc)
                .OrderBy(a => a.dtinicio)
                .ToListAsync();

            return Ok(agendamentos);
        }

        // GET: /api/admin/agenda/resumo?data=
        [HttpGet("resumo")]
        public async Task<IActionResult> GetResumoDia([FromQuery] DateTime data)
        {
            var (inicioUtc, fimUtc) = ObterIntervaloUtc(data);

            var agendamentos = await _context.pagendamentos
                .Where(a => a.dtinicio >= inicioUtc && a.dtinicio < fimUtc)
                .Include(a => a.Servico)
                .ToListAsync();

            var totalAgendamentos = agendamentos.Count;
            var totalTempo = agendamentos.Sum(a => (a.dtfim - a.dtinicio).TotalMinutes);
            var totalPreco = agendamentos.Sum(a => a.Servico.preco);

            return Ok(new
            {
                TotalAgendamentos = totalAgendamentos,
                TotalTempo = totalTempo,
                TotalPreco = totalPreco
            });
        }
    }
}