using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Storage;

internal sealed record RecoveryStoreObservation(
    RecoveryStoreKind Kind,
    RecoveryStoreLocation? Location,
    RecoveryBundleCatalogueResult Catalogue);
