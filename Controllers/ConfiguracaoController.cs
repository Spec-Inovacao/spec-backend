using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourNamespace.Data;
using YourNamespace.Models;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class ConfiguracaoController : ControllerBase
    {
        private readonly YourDbContext _context;

        public ConfiguracaoController(YourDbContext context)
        {
            _context = context;
        }

        // GET: /api/admin/configuracao
        [HttpGet]
        public async Task<IActionResult> GetConfiguracao()
        {
            var config = await _context.pconfigexp.FirstOrDefaultAsync();
            if (config == null)
                return NotFound();

            return Ok(config);
        }

        // PUT: /api/admin/configuracao
        [HttpPut]
        public async Task<IActionResult> UpdateConfiguracao([FromBody] Configuracao config)
        {
            _context.pconfigexp.Update(config);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}