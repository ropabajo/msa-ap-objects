using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Ropabajo.Church.Sanluis.Objects.Api.Controllers;

namespace Ropabajo.Church.Sanluis.Objects.Api.Extensions;

/// <summary>
/// Publica el contrato combinado en Local y Production y retira allí el contrato sin ámbito.
/// </summary>
internal sealed class ImportControllerConvention(bool combined) : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        var hidden = combined ? typeof(BulkLoadController) : typeof(CombinedImportController);
        foreach (var controller in application.Controllers.Where(x => x.ControllerType.AsType() == hidden || (!combined && x.ControllerType.AsType() == typeof(ImportProgressController))).ToArray()) application.Controllers.Remove(controller);
    }
}
