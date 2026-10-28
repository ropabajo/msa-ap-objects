using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Ropabajo.Church.Sanluis.Objects.Api.Controllers;

namespace Ropabajo.Church.Sanluis.Objects.Api.Extensions;

/// <summary>
/// Publica el contrato combinado exclusivamente en Local y retira allí el contrato sin ámbito.
/// </summary>
internal sealed class ImportControllerConvention(bool local) : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        var hidden = local ? typeof(BulkLoadController) : typeof(CombinedImportController);
        foreach (var controller in application.Controllers.Where(x => x.ControllerType.AsType() == hidden || (!local && x.ControllerType.AsType() == typeof(ImportProgressController))).ToArray()) application.Controllers.Remove(controller);
    }
}
