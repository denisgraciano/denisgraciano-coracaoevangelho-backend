// ============================================================
// Validators/Validators.cs
// FluentValidation para todos os RequestDtos
// ============================================================

using CoracaoEvangelho.API.DTOs.Request;
using FluentValidation;

namespace CoracaoEvangelho.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
            .MaximumLength(150);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(200);

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres.")
            .Matches("[A-Z]").WithMessage("Senha deve conter ao menos uma letra maiúscula.")
            .Matches("[0-9]").WithMessage("Senha deve conter ao menos um número.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Senha).NotEmpty();
    }
}

public class MatriculaRequestValidator : AbstractValidator<MatriculaRequestDto>
{
    public MatriculaRequestValidator()
    {
        RuleFor(x => x.NomeCompleto)
            .NotEmpty().WithMessage("Nome completo é obrigatório.")
            .MinimumLength(3);

        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress().WithMessage("E-mail inválido.");
    }
}

public class PedidoVibracaoRequestValidator : AbstractValidator<PedidoVibracaoRequestDto>
{
    public PedidoVibracaoRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MinimumLength(3).MaximumLength(150);
        RuleFor(x => x.Email)
    .EmailAddress().WithMessage("E-mail inválido.")
    .When(x => !string.IsNullOrWhiteSpace(x.Email)); // só valida se vier preenchido

        RuleFor(x => x.Pedido)
            .NotEmpty().WithMessage("Por favor, descreva seu pedido.")
            .MinimumLength(10).WithMessage("Pedido muito curto.")
            .MaximumLength(5000);
    }
}

public class MarcarAulaConcluidaRequestValidator : AbstractValidator<MarcarAulaConcluidaRequestDto>
{
    public MarcarAulaConcluidaRequestValidator()
    {
        RuleFor(x => x.CursoId).NotEmpty();
        RuleFor(x => x.AulaId).NotEmpty();
    }
}

public class AtualizarPerfilRequestValidator : AbstractValidator<AtualizarPerfilRequestDto>
{
    public AtualizarPerfilRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().MinimumLength(3).MaximumLength(150);

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500)
            .Must(url => url == null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("URL do avatar inválida.")
            .When(x => x.AvatarUrl != null);
    }
}

public class AlterarSenhaRequestValidator : AbstractValidator<AlterarSenhaRequestDto>
{
    public AlterarSenhaRequestValidator()
    {
        RuleFor(x => x.SenhaAtual).NotEmpty();
        RuleFor(x => x.NovaSenha)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Nova senha deve conter ao menos uma maiúscula.")
            .Matches("[0-9]").WithMessage("Nova senha deve conter ao menos um número.")
            .NotEqual(x => x.SenhaAtual).WithMessage("Nova senha deve ser diferente da atual.");
    }
}
