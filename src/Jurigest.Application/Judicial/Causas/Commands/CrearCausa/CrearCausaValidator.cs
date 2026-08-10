using FluentValidation;

namespace Jurigest.Application.Judicial.Causas.Commands.CrearCausa;

public sealed class CrearCausaValidator
    : AbstractValidator<CrearCausaCommand>
{
    public CrearCausaValidator()
    {
        RuleFor(x => x.Rit)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.Tribunal)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .MaximumLength(1000);
    }
}