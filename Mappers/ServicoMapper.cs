using YourNamespace.DTOs;
using YourNamespace.Models;

namespace YourNamespace.Mappers
{
    public static class ServicoMapper
    {
        public static ServicoDTO ToDTO(Servico servico)
        {
            return new ServicoDTO
            {
                id = servico.id,
                nome = servico.nome,
                tempoMin = servico.tempomin,
                preco = servico.preco
            };
        }
    }
}