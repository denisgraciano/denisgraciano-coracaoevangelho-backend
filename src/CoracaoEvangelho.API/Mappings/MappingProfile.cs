using AutoMapper;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Models;

namespace CoracaoEvangelho.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Livro → LivroResponseDto
        CreateMap<Livro, LivroResponseDto>();

        // Capitulo → CapituloResponseDto (versículos sem isFavorito por padrão)
        CreateMap<Capitulo, CapituloResponseDto>()
            .ForMember(dest => dest.Versiculos,
                opt => opt.MapFrom(src => src.Versiculos));

        // Versiculo → VersiculoResponseDto
        // IsFavorito é calculado no Service conforme o usuário logado
        CreateMap<Versiculo, VersiculoResponseDto>()
            .ForMember(dest => dest.IsFavorito, opt => opt.Ignore());

        // Devocional → DevocionalResponseDto
        CreateMap<Devocional, DevocionalResponseDto>()
            .ForMember(dest => dest.Versiculo,
                opt => opt.MapFrom(src => src.Versiculo));

        // Favorito → FavoritoResponseDto
        CreateMap<Favorito, FavoritoResponseDto>()
            .ForMember(dest => dest.Versiculo,
                opt => opt.MapFrom(src => src.Versiculo));

        // Usuario → UsuarioResponseDto (SenhaHash NUNCA é mapeado)
        CreateMap<Usuario, UsuarioResponseDto>();

        // Usuario → ConfiguracaoResponseDto
        CreateMap<Usuario, ConfiguracaoResponseDto>()
            .ForMember(dest => dest.Tema, opt => opt.MapFrom(src => src.Tema))
            .ForMember(dest => dest.TamanhoFonte, opt => opt.MapFrom(src => src.TamanhoFonte));

        // Sync
        CreateMap<Livro, SyncLivroDto>()
            .ForMember(dest => dest.Deletado, opt => opt.MapFrom(src => src.Deletado));
    }
}
