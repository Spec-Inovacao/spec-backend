using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourNamespace.Data;
using YourNamespace.Models;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisponibilidadeController : ControllerBase
    {
        private readonly YourDbContext _context;

        private static readonly TimeZoneInfo FusoHorario =
            TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

        public DisponibilidadeController(YourDbContext context)
        {
            _context = context;
        }

        private (DateTime inicioUtc, DateTime fimUtc) ObterIntervaloUtc(DateTime data, TimeSpan horaInicio, TimeSpan horaFim)
        {
            var inicioLocal = DateTime.SpecifyKind(data.Date.Add(horaInicio), DateTimeKind.Unspecified);
            var fimLocal = DateTime.SpecifyKind(data.Date.Add(horaFim), DateTimeKind.Unspecified);

            var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(inicioLocal, FusoHorario);
            var fimUtc = TimeZoneInfo.ConvertTimeToUtc(fimLocal, FusoHorario);

            return (inicioUtc, fimUtc);
        }

        // Interpreta os dias da semana atendidos (int[]: 0=Dom ... 6=Sáb)
        private static HashSet<DayOfWeek> ParseDiasAtendimento(int[] diasAtendimento)
        {
            var dias = new HashSet<DayOfWeek>();

            if (diasAtendimento == null)
                return dias;

            foreach (var num in diasAtendimento)
            {
                if (num >= 0 && num <= 6)
                    dias.Add((DayOfWeek)num);
            }

            return dias;
        }

        // GET: /api/disponibilidade?servicoId=&data=
        [HttpGet]
        public async Task<IActionResult> GetDisponibilidade([FromQuery] int servicoId, [FromQuery] DateTime data)
        {
            var config = await _context.pconfigexp.FirstOrDefaultAsync();

            if (config == null)
                return BadRequest("Configuração não encontrada.");

            var servico = await _context.pservicos.FindAsync(servicoId);

            if (servico == null || !servico.ativo)
                return BadRequest("Serviço inválido ou inativo.");

            // Verifica se o dia da semana é atendido
            var diasSemanaAtendidos = ParseDiasAtendimento(config.diasatendimento);

            if (!diasSemanaAtendidos.Contains(data.Date.DayOfWeek))
                return Ok(new List<string>());

            // Converte o horário de funcionamento para UTC
            var (inicioUtc, fimUtc) = ObterIntervaloUtc(data, config.horainicio, config.horafim);

            var agendamentos = await _context.pagendamentos
                .Where(a =>
                    a.dtinicio < fimUtc &&
                    a.dtfim > inicioUtc &&
                    a.status == "agendado")
                .ToListAsync();

            var bloqueios = await _context.pbloqagenda
                .Where(b =>
                    b.dtinicio < fimUtc &&
                    b.dtfim > inicioUtc)
                .ToListAsync();

            // AJUSTE: troque /* NOME REAL */ pela propriedade de duração (em minutos) do model Servico
            var duracao = TimeSpan.FromMinutes(servico.tempomin);
            var passo = duracao; // AJUSTE: use um passo fixo se quiser outra granularidade

            var agoraUtc = DateTime.UtcNow;
            var slots = new List<string>();

            for (var slotInicioUtc = inicioUtc; slotInicioUtc + duracao <= fimUtc; slotInicioUtc += passo)
            {
                var slotFimUtc = slotInicioUtc + duracao;

                // Descarta horários que já passaram (relevante quando data == hoje)
                if (slotInicioUtc <= agoraUtc)
                    continue;

                bool colideAgendamento = agendamentos.Any(a =>
                    a.dtinicio < slotFimUtc && a.dtfim > slotInicioUtc);

                bool colideBloqueio = bloqueios.Any(b =>
                    b.dtinicio < slotFimUtc && b.dtfim > slotInicioUtc);

                if (colideAgendamento || colideBloqueio)
                    continue;

                var slotLocal = TimeZoneInfo.ConvertTimeFromUtc(slotInicioUtc, FusoHorario);
                slots.Add(slotLocal.ToString("HH:mm"));
            }

            return Ok(slots);
        }

        // GET: /api/disponibilidade/dias?mes=
        [HttpGet("dias")]
        public async Task<IActionResult> GetDiasAtendimento([FromQuery] int mes)
        {
            if (mes < 1 || mes > 12)
                return BadRequest("Mês inválido.");

            var config = await _context.pconfigexp.FirstOrDefaultAsync();

            if (config == null)
                return BadRequest("Configuração não encontrada.");

            var diasSemanaAtendidos = ParseDiasAtendimento(config.diasatendimento);

            var hojeLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, FusoHorario).Date;
            var ano = hojeLocal.Year; // AJUSTE: assume ano atual (parâmetro só traz o mês)
            var diasNoMes = DateTime.DaysInMonth(ano, mes);

            // Intervalo UTC do mês inteiro para buscar todos os bloqueios de uma vez
            var inicioMesLocal = DateTime.SpecifyKind(new DateTime(ano, mes, 1), DateTimeKind.Unspecified);
            var fimMesLocal = DateTime.SpecifyKind(inicioMesLocal.AddMonths(1), DateTimeKind.Unspecified);
            var inicioMesUtc = TimeZoneInfo.ConvertTimeToUtc(inicioMesLocal, FusoHorario);
            var fimMesUtc = TimeZoneInfo.ConvertTimeToUtc(fimMesLocal, FusoHorario);

            var bloqueios = await _context.pbloqagenda
                .Where(b => b.dtinicio < fimMesUtc && b.dtfim > inicioMesUtc)
                .ToListAsync();

            var culturaPt = new CultureInfo("pt-BR");
            var diasDisponiveis = new List<string>();

            for (int dia = 1; dia <= diasNoMes; dia++)
            {
                var dataLocal = new DateTime(ano, mes, dia);

                var abrev = culturaPt.DateTimeFormat
                    .GetAbbreviatedDayName(dataLocal.DayOfWeek)
                    .TrimEnd('.');
                abrev = char.ToUpper(abrev[0]) + abrev.Substring(1);

                string status;

                if (dataLocal < hojeLocal)
                {
                    status = "fechado"; // dia já passou
                }
                else if (!diasSemanaAtendidos.Contains(dataLocal.DayOfWeek))
                {
                    status = "fechado"; // dia da semana não atendido
                }
                else
                {
                    // Fecha o dia apenas se um bloqueio cobrir todo o expediente
                    var (diaInicioUtc, diaFimUtc) = ObterIntervaloUtc(dataLocal, config.horainicio, config.horafim);

                    bool expedienteTodoBloqueado = bloqueios.Any(b =>
                        b.dtinicio <= diaInicioUtc && b.dtfim >= diaFimUtc);

                    status = expedienteTodoBloqueado ? "fechado" : "aberto";
                }

                diasDisponiveis.Add($"{abrev} {dia} {status}");
            }

            return Ok(diasDisponiveis);
        }
    }
}