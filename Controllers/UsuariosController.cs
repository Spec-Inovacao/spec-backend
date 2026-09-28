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
    public class UsuariosController : ControllerBase
    {
        private readonly YourDbContext _context;

        public UsuariosController(YourDbContext context)
        {
            _context = context;
        }

        // POST: /api/clientes
        [HttpPost]
        public async Task<IActionResult> FindOrCreateCliente([FromBody] InsertUsuarioDTO dto)
        {
            var usuario = await _context.pusuarios
                .FirstOrDefaultAsync(u => u.email == dto.Email);

            if (usuario == null)
            {
                usuario = UsuarioMapper.ToModel(dto);
                _context.pusuarios.Add(usuario);
                await _context.SaveChangesAsync();
            }

            return Ok(UsuarioMapper.ToListDTO(usuario));
        }

        // GET: /api/clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListUsuarioDTO>>> ListClientes()
        {
            var usuarios = await _context.pusuarios.ToListAsync();
            var result = usuarios.Select(UsuarioMapper.ToListDTO).ToList();
            return Ok(result);
        }

        // GET: /api/clientes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ListUsuarioDTO>> GetCliente(int id)
        {
            var usuario = await _context.pusuarios.FindAsync(id);
            if (usuario == null)
                return NotFound();

            return Ok(UsuarioMapper.ToListDTO(usuario));
        }
    }
}