using YourNamespace.DTOs;
using YourNamespace.Models;

namespace YourNamespace.Mappers
{
    public static class UsuarioMapper
    {
        public static ListUsuarioDTO ToListDTO(Usuario usuario)
        {
            return new ListUsuarioDTO
            {
                Id = usuario.id,
                Nome = usuario.nome,
                Email = usuario.email,
                Telefone = usuario.telefone
            };
        }

        public static Usuario ToModel(InsertUsuarioDTO dto)
        {
            return new Usuario
            {
                nome = dto.Nome,
                email = dto.Email,
                telefone = dto.Telefone
            };
        }
    }
}