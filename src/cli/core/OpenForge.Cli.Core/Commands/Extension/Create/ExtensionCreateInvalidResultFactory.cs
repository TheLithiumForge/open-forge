using OpenForge.Cli.Core.Commands.Extension.Create.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Create;

internal sealed class ExtensionCreateInvalidResultFactory(ExtensionCreateSymbols symbols)
{
    private readonly ExtensionCreateSymbols _symbols = symbols;

    internal ExtensionCreateResult Create(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var result = input.BindingParse.Result;
        string? stableId;
        string? cataloguePath;
        try
        {
            stableId = result.GetValue(_symbols.StableId);
        }
        catch (InvalidOperationException)
        {
            stableId = null;
        }

        try
        {
            cataloguePath = result.GetValue(_symbols.Path);
        }
        catch (InvalidOperationException)
        {
            cataloguePath = null;
        }

        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        return ExtensionCreateRequestBinder.CreateInvalidResult(
            new Models.Request.ExtensionCreateRequest
            {
                StableId = stableId,
                CataloguePath = cataloguePath,
                Name = null,
                Description = null,
                PackageVersion = null,
                Dependencies = [],
                AllowInteraction = false,
                Mode = result.GetValue(_symbols.DryRun)
                    ? Models.Request.ExtensionCreateMode.DryRun
                    : Models.Request.ExtensionCreateMode.Apply,
            },
            cause);
    }
}
