using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourNamespace.Data;
using YourNamespace.DTOs;
using YourNamespace.Mappers;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicosController : ControllerBase
    {
        private readonly YourDbContext _context;

        public ServicosController(YourDbContext context)
        {
            _context = context;
        }

        // GET: /api/servicos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServicoDTO>>> ListServicos()
        {
            var servicos = await _context.pservicos
                .Where(s => s.ativo)
                .Select(s => ServicoMapper.ToDTO(s))
                .ToListAsync();

            return Ok(servicos);
        }

        // GET: /api/servicos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ServicoDTO>> GetServico(int id)
        {
            var servico = await _context.pservicos
                .Where(s => s.id == id)
                .FirstOrDefaultAsync();

            if (servico == null)
            {
                return NotFound();
            }

            return Ok(ServicoMapper.ToDTO(servico));
        }
    }
}