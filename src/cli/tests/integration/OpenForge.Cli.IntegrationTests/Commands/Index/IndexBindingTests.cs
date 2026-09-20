using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Presentation.Index.Shared.Help;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index;

public sealed class IndexBindingTests
{

    [Fact(DisplayName = "Index binding closes the exact command over direct typed dependencies"), Trait("Feature", "index-command"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public void BindingClosesExactCommandOverDirectDependencies()
    {
        var symbols = IndexBinding.CreateSymbols();
        var help = IndexHelpSections.Create();

        var binding = OpenForge.Cli.Core.Shell.Composition.CliReportBinding.Close(IndexBinding.CreateRequestBinding(
            symbols: symbols,
            help: help,
            operation: IndexOperationFactory.Create().ExecuteAsync), OpenForge.Cli.Core.Presentation.Index.IndexPresentation.Rendering);

        Assert.Same(symbols.IndexCommand, binding.Command);
        Assert.Same(help, binding.Help);
        Assert.Equal(CliWorkspaceRequirement.Required, binding.WorkspaceRequirement);
    }

}
