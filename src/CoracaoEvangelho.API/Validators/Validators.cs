using CoracaoEvangelho.API.DTOs.Request;
using FluentValidation;

namespace CoracaoEvangelho.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MinimumLength(3).WithMessage("Nome deve ter ao menos 3 caracteres.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(200).WithMessage("E-mail deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter ao menos 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("Senha deve conter ao menos uma letra maiúscula.")
            .Matches(@"[0-9]").WithMessage("Senha deve conter ao menos um número.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("E-mail inválido.");
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Senha é obrigatória.");
    }
}

public class AdicionarFavoritoRequestValidator : AbstractValidator<AdicionarFavoritoRequestDto>
{
    public AdicionarFavoritoRequestValidator()
    {
        RuleFor(x => x.VersiculoId)
            .NotEmpty().WithMessage("VersiculoId é obrigatório.");
    }
}

public class SetTemaRequestValidator : AbstractValidator<SetTemaRequestDto>
{
    public SetTemaRequestValidator()
    {
        RuleFor(x => x.Tema)
            .NotEmpty()
            .Must(t => t == "claro" || t == "escuro")
            .WithMessage("Tema deve ser 'claro' ou 'escuro'.");
    }
}

public class SetFonteRequestValidator : AbstractValidator<SetFonteRequestDto>
{
    public SetFonteRequestValidator()
    {
        RuleFor(x => x.TamanhoFonte)
            .InclusiveBetween(12, 28)
            .WithMessage("Tamanho de fonte deve estar entre 12 e 28.");
    }
}
