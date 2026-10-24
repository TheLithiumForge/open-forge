using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Remove;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Exclude;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Commands.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Runtime;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Storage;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Presentation.Cleanup.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Status.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.Core.UnitTests.Contracts;

public sealed class PublishedTokensTests
{
    [Fact(DisplayName = "Published-process tokens match their production owners"), Trait("Feature", "public-contract"), Trait("Evidence", "Unit")]
    public void EveryTokenMatchesItsOwner()
    {
#pragma warning disable xUnit2000 // The projected constants are under test, and the production owners supply the expected values.
        Assert.Equal(InstallDefinitions.InstallCommand.Name, PublishedTokens.Install.Command);
        Assert.Equal(InstallDefinitions.Automatic.Name, PublishedTokens.Install.Automatic);
        Assert.Equal(InstallDefinitions.DryRun.Name, PublishedTokens.Install.DryRun);
        Assert.Equal(InstallDefinitions.Frontmatter.Name, PublishedTokens.Install.Frontmatter);

        Assert.Equal(IndexDefinitions.IndexCommand.Name, PublishedTokens.Index.Command);
        Assert.Equal(IndexDefinitions.DryRun.Name, PublishedTokens.Index.DryRun);

        Assert.Equal(StatusDefinitions.StatusCommand.Name, PublishedTokens.Status.Command);

        Assert.Equal(DoctorDefinitions.DoctorCommand.Name, PublishedTokens.Doctor.Command);

        Assert.Equal(CleanupDefinitions.CleanupCommand.Name, PublishedTokens.Cleanup.Command);
        Assert.Equal(CleanupDefinitions.DryRun.Name, PublishedTokens.Cleanup.DryRun);

        Assert.Equal(RemoveDefinitions.CommandIdentity, PublishedTokens.Remove.Command);
        Assert.Equal(RemoveDefinitions.AutomaticOption, PublishedTokens.Remove.Automatic);

        Assert.Equal(RouteDefinitions.RouteGroup.Name, PublishedTokens.Route.Command);

        Assert.Equal(RouteIncludeDefinitions.Command.Name, PublishedTokens.RouteInclude.Command);
        Assert.Equal(RouteIncludeDefinitions.Description.Name, PublishedTokens.RouteInclude.Description);
        Assert.Equal(RouteIncludeDefinitions.RecoveryHistoryCode, PublishedTokens.RouteInclude.RecoveryHistoryCode);

        Assert.Equal(RouteExcludeDefinitions.Command.Name, PublishedTokens.RouteExclude.Command);

        Assert.Equal(RouteCreateDefinitions.CreateCommand.Name, PublishedTokens.RouteCreate.Command);
        Assert.Equal(RouteCreateDefinitions.Description.Name, PublishedTokens.RouteCreate.Description);
        Assert.Equal(RouteCreateDefinitions.Tag.Name, PublishedTokens.RouteCreate.Tag);

        Assert.Equal(RouteUpdateDefinitions.UpdateCommand.Name, PublishedTokens.RouteUpdate.Command);
        Assert.Equal(RouteUpdateDefinitions.Description.Name, PublishedTokens.RouteUpdate.Description);

        Assert.Equal(RouteMoveDefinitions.MoveCommand.Name, PublishedTokens.RouteMove.Command);

        Assert.Equal(RouteRemoveDefinitions.RemoveCommand.Name, PublishedTokens.RouteRemove.Command);
        Assert.Equal(RouteRemoveDefinitions.Automatic.Name, PublishedTokens.RouteRemove.Automatic);

        Assert.Equal(RouteInitDefinitions.InitCommand.Name, PublishedTokens.RouteInit.Command);
        Assert.Equal(RouteInitDefinitions.Description.Name, PublishedTokens.RouteInit.Description);
        Assert.Equal(RouteInitDefinitions.Tag.Name, PublishedTokens.RouteInit.Tag);

        Assert.Equal(ExtensionDefinitions.ExtensionGroup.Name, PublishedTokens.Extension.Command);

        Assert.Equal(ExtensionInstallDefinitions.InstallCommand.Name, PublishedTokens.ExtensionInstall.Command);
        Assert.Equal(ExtensionInstallDefinitions.Automatic.Name, PublishedTokens.ExtensionInstall.Automatic);
        Assert.Equal(ExtensionInstallDefinitions.Source.Name, PublishedTokens.ExtensionInstall.Source);

        Assert.Equal(ExtensionUpdateDefinitions.UpdateCommand.Name, PublishedTokens.ExtensionUpdate.Command);
        Assert.Equal(ExtensionUpdateDefinitions.Automatic.Name, PublishedTokens.ExtensionUpdate.Automatic);
        Assert.Equal(ExtensionUpdateDefinitions.Source.Name, PublishedTokens.ExtensionUpdate.Source);

        Assert.Equal(ExtensionRemoveDefinitions.RemoveCommand.Name, PublishedTokens.ExtensionRemove.Command);
        Assert.Equal(ExtensionRemoveDefinitions.Automatic.Name, PublishedTokens.ExtensionRemove.Automatic);

        Assert.Equal(CliSyntaxDefinitions.Format.Name, PublishedTokens.Syntax.FormatOption);
        Assert.Equal(CliSyntaxDefinitions.Detail.Name, PublishedTokens.Syntax.DetailOption);
        Assert.Equal(CliSyntaxDefinitions.Format.FiniteSpellings.Single(pair => pair.Value == CliFormat.Json).Key, PublishedTokens.Syntax.JsonFormat);

        Assert.Equal(CliPresentationDefinitions.Full, PublishedTokens.Presentation.FullDetail);

        Assert.Equal(CurrentUserDataHome.OverrideVariable, PublishedTokens.DataHome.Variable);

        Assert.Equal(WorkspaceRuntimeBootstrap.IgnoreContent, PublishedTokens.Runtime.IgnoreContent);

        Assert.Equal(SourceLogicalPath.AgentsRoot, PublishedTokens.Sources.AgentsRoot);
        Assert.Equal(SourceLogicalPath.LoaderPath, PublishedTokens.Sources.LoaderPath);

        Assert.Equal(WorkspaceOwnershipDefinitions.RelativePath, PublishedTokens.Ownership.RelativePath);

        Assert.Equal(WorkspaceSettingsDefinitions.FrontmatterScopedName, PublishedTokens.Settings.ScopedFrontmatter);

        Assert.Equal(ExtensionPackageLayout.ContentDirectoryName, PublishedTokens.ExtensionPackage.ContentDirectory);

        Assert.Equal(RecoveryBundleFormatV1.FinalFilePrefix, PublishedTokens.RecoveryFormat.FinalFilePrefix);
        Assert.Equal(RecoveryBundleFormatV1.FinalFileExtension, PublishedTokens.RecoveryFormat.FinalFileExtension);
        Assert.Equal(RecoveryBundleFormatV1.DraftFileExtension, PublishedTokens.RecoveryFormat.DraftFileExtension);
        Assert.Equal(RecoveryBundleFormatV1.OperationIdFormat, PublishedTokens.RecoveryFormat.OperationIdFormat);

        Assert.Equal(RecoveryWireVocabulary.StoreKind(RecoveryStoreKind.Workspace), PublishedTokens.Recovery.WorkspaceStore);
        Assert.Equal(RecoveryWireVocabulary.StoreKind(RecoveryStoreKind.Legacy), PublishedTokens.Recovery.LegacyStore);
        Assert.Equal(RecoveryWireVocabulary.Integrity(RecoveryBundleIntegrity.Verified), PublishedTokens.Recovery.VerifiedIntegrity);

        Assert.Equal(CliStatusDefinitions.Read(CliSemanticStatus.Complete).MachineName, PublishedTokens.Statuses.Complete);
        Assert.Equal(CliStatusDefinitions.Read(CliSemanticStatus.Attention).MachineName, PublishedTokens.Statuses.Attention);
        Assert.Equal(CliStatusDefinitions.Read(CliSemanticStatus.Incomplete).MachineName, PublishedTokens.Statuses.Incomplete);

        Assert.Equal(CliReportVocabulary.Name(CliSeverity.Warning), PublishedTokens.Report.WarningSeverity);

        Assert.Equal(CliJsonFields.Data, PublishedTokens.Json.Data);
        Assert.Equal(CliJsonFields.Command, PublishedTokens.Json.Command);
        Assert.Equal(CliJsonFields.Status, PublishedTokens.Json.Status);
        Assert.Equal(CliJsonFields.Findings, PublishedTokens.Json.Findings);
        Assert.Equal(CliJsonFields.Code, PublishedTokens.Json.Code);
        Assert.Equal(CliJsonFields.Severity, PublishedTokens.Json.Severity);
        Assert.Equal(CliJsonFields.Subject, PublishedTokens.Json.Subject);
        Assert.Equal(CliJsonFields.Path, PublishedTokens.Json.Path);

        Assert.Equal(StatusRecoveryJsonFields.Recovery, PublishedTokens.StatusRecovery.Recovery);
        Assert.Equal(StatusRecoveryJsonFields.Stores, PublishedTokens.StatusRecovery.Stores);
        Assert.Equal(StatusRecoveryJsonFields.Candidates, PublishedTokens.StatusRecovery.Candidates);
        Assert.Equal(StatusRecoveryJsonFields.Kind, PublishedTokens.StatusRecovery.Kind);
        Assert.Equal(StatusRecoveryJsonFields.Path, PublishedTokens.StatusRecovery.Path);
        Assert.Equal(StatusRecoveryJsonFields.Integrity, PublishedTokens.StatusRecovery.Integrity);

        Assert.Equal(DoctorDataJsonFields.RecoveryStores, PublishedTokens.DoctorData.RecoveryStores);
        Assert.Equal(DoctorDataJsonFields.Kind, PublishedTokens.DoctorData.Kind);
        Assert.Equal(DoctorDataJsonFields.Path, PublishedTokens.DoctorData.Path);
        Assert.Equal(DoctorDataJsonFields.Candidates, PublishedTokens.DoctorData.Candidates);

        Assert.Equal(CleanupDataJsonFields.Stores, PublishedTokens.CleanupData.Stores);
        Assert.Equal(CleanupDataJsonFields.Kind, PublishedTokens.CleanupData.Kind);
        Assert.Equal(CleanupDataJsonFields.Path, PublishedTokens.CleanupData.Path);
        Assert.Equal(CleanupDataJsonFields.Candidates, PublishedTokens.CleanupData.Candidates);
        Assert.Equal(CleanupDataJsonFields.Cause, PublishedTokens.CleanupData.Cause);
#pragma warning restore xUnit2000
    }
}
