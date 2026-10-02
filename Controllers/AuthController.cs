using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using YourNamespace.Data;
using YourNamespace.DTOs;
using YourNamespace.Mappers;
using YourNamespace.Models;
using YourNamespace.Services;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly YourDbContext _context;
        private readonly IPasswordHasher<Usuario> _hasher;
        private readonly TokenService _tokenService;

        public AuthController(YourDbContext context, IPasswordHasher<Usuario> hasher, TokenService tokenService)
        {
            _context = context;
            _hasher = hasher;
            _tokenService = tokenService;
        }

        // POST: /api/auth/cadastro
        [HttpPost("cadastro")]
        public async Task<IActionResult> Cadastrar([FromBody] CadastroUsuarioDTO dto)
        {
            var email = NormalizarEmail(dto.Email);

            var usuario = await _context.pusuarios
                .FirstOrDefaultAsync(u => u.email.ToLower() == email);

            if (usuario != null && usuario.senhahash != null)
                return Conflict(new { message = "E-mail já cadastrado." });

            if (usuario == null)
            {
                usuario = new Usuario
                {
                    email = email,
                    reccreatedon = DateTime.UtcNow
                };
                _context.pusuarios.Add(usuario);
            }

            // Usuário novo ou criado antes sem senha (fluxo antigo do front): define os dados e a senha
            usuario.nome = dto.Nome.Trim();
            usuario.telefone = dto.Telefone.Trim();
            usuario.senhahash = _hasher.HashPassword(usuario, dto.Senha);

            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, MontarResposta(usuario));
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var email = NormalizarEmail(dto.Email);

            var usuario = await _context.pusuarios
                .FirstOrDefaultAsync(u => u.email.ToLower() == email);

            // Mesma mensagem para e-mail inexistente e senha errada (não revela quais e-mails existem)
            if (usuario?.senhahash == null)
                return Unauthorized(new { message = "E-mail ou senha inválidos." });

            var resultado = _hasher.VerifyHashedPassword(usuario, usuario.senhahash, dto.Senha);
            if (resultado == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "E-mail ou senha inválidos." });

            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.senhahash = _hasher.HashPassword(usuario, dto.Senha);
                await _context.SaveChangesAsync();
            }

            return Ok(MontarResposta(usuario));
        }

        // GET: /api/auth/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!int.TryParse(sub, out var id))
                return Unauthorized();

            var usuario = await _context.pusuarios.FindAsync(id);
            if (usuario == null)
                return Unauthorized();

            return Ok(UsuarioMapper.ToListDTO(usuario));
        }

        private AuthResponseDTO MontarResposta(Usuario usuario)
        {
            var (token, expiraEm) = _tokenService.Gerar(usuario);
            return new AuthResponseDTO
            {
                Token = token,
                ExpiraEm = expiraEm,
                Admin = usuario.admin,
                Usuario = UsuarioMapper.ToListDTO(usuario)
            };
        }

        private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
    }
}