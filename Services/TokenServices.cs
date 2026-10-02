using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using YourNamespace.Models;

namespace YourNamespace.Services
{
    public class TokenService
    {
        public const string Emissor = "agenda-api";
        private static readonly TimeSpan Validade = TimeSpan.FromHours(8);

        public SymmetricSecurityKey Chave { get; }

        public TokenService(string segredo)
        {
            Chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(segredo));
        }

        public (string Token, DateTime ExpiraEm) Gerar(Usuario usuario)
        {
            var expiraEm = DateTime.UtcNow.Add(Validade);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = Emissor,
                Audience = Emissor,
                Expires = expiraEm,
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, usuario.id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, usuario.email),
                    new Claim("name", usuario.nome),
                    new Claim("role", usuario.admin ? "admin" : "cliente")
                }),
                SigningCredentials = new SigningCredentials(Chave, SecurityAlgorithms.HmacSha256)
            };

            return (new JsonWebTokenHandler().CreateToken(descriptor), expiraEm);
        }
    }
}