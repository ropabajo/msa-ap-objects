using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.CreateFileJob;

/// <summary>
/// Valida las entradas del caso CreateFileJob.
/// </summary>
public sealed class CreateFileJobCommandValidator : AbstractValidator<CreateFileJobCommand>
{
    /// <summary>
    /// Inicializa las restricciones del contrato.
    /// </summary>
    public CreateFileJobCommandValidator()
    {
        RuleFor(command => command.ChurchCode).NotEmpty()
            .WithMessage(FileJobValidationMessages.ChurchCodeRequired).WithErrorCode(FileJobValidationCodes.ChurchCodeRequired);
        RuleFor(command => command.RequestId).NotEmpty()
            .WithMessage(FileJobValidationMessages.RequestIdRequired).WithErrorCode(FileJobValidationCodes.RequestIdRequired);
        RuleFor(command => command.ReportType).NotEmpty()
            .WithMessage(FileJobValidationMessages.ReportTypeRequired).WithErrorCode(FileJobValidationCodes.ReportTypeRequired);
        RuleFor(command => command.ContractVersion).GreaterThan(0)
            .WithMessage(FileJobValidationMessages.ContractVersionGtZero).WithErrorCode(FileJobValidationCodes.ContractVersionGtZero);
        RuleFor(command => command.Parameters).Must(value => value.ValueKind == System.Text.Json.JsonValueKind.Object)
            .WithMessage(FileJobValidationMessages.ParametersObject).WithErrorCode(FileJobValidationCodes.ParametersObject);
        RuleFor(command => command.ReportType).MaximumLength(100).WithMessage(FileJobValidationMessages.ReportTypeMaxLength).WithErrorCode(FileJobValidationCodes.ReportTypeMaxLength);
        RuleFor(command => command.ContractVersion).LessThanOrEqualTo(999).WithMessage(FileJobValidationMessages.ContractVersionMaximum).WithErrorCode(FileJobValidationCodes.ContractVersionMaximum);
    }
}
