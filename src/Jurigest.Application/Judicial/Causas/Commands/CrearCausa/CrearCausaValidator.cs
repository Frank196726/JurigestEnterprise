using FluentValidation;

namespace Jurigest.Application.Judicial.Causas.Commands.CrearCausa;

public sealed class CrearCausaValidator
    : AbstractValidator<CrearCausaCommand>
{
    public CrearCausaValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                x.TipoCausaId.HasValue ||
                !string.IsNullOrWhiteSpace(x.Rit))
            .WithMessage(
                "Debe indicar el RIT o seleccionar un tipo de causa.");

        When(
            x => !string.IsNullOrWhiteSpace(x.Rit),
            () =>
            {
                RuleFor(x => x.Rit)
                    .MaximumLength(30)
                    .Must(rit =>
                        rit is null ||
                        rit.IndexOfAny(
                            ['/','\\','?','%','*',':','|','"','<','>']) < 0)
                    .WithMessage(
                        "El RIT contiene caracteres no permitidos.");
            });

        When(
            x => x.TipoCausaId.HasValue,
            () =>
            {
                RuleFor(x => x.TipoCausaId)
                    .Must(id =>
                        id.HasValue &&
                        id.Value != Guid.Empty)
                    .WithMessage(
                        "El tipo de causa no es válido.");

                RuleFor(x => x.NumeroRol)
                    .NotEmpty()
                    .MaximumLength(30);
            });

        RuleFor(x => x.Tribunal)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.FechaEncargoCausa)
            .NotEqual(default(DateTime))
            .WithMessage(
                "Debe indicar la fecha de encargo de la causa.");
    }
}