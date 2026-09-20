using System.Globalization;
using OpenForge.Cli.Core.Commands.Repair.Shared.Request;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Request;

internal sealed record RepairSourceLocation
{
    internal RepairSourceLocation(string sourceCanonicalPath, int line, int column)
    {
        SourceCanonicalPath = RepairPathValidation.ValidateMarkdown(
            sourceCanonicalPath,
            nameof(sourceCanonicalPath));
        if (line < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(line),
                line,
                "A Repair source location line must be positive.");
        }

        if (column < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(column),
                column,
                "A Repair source location column must be positive.");
        }

        Line = line;
        Column = column;
    }

    internal string SourceCanonicalPath { get; }

    internal int Line { get; }

    internal int Column { get; }

    internal static RepairSourceLocation Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var at = value.LastIndexOf('@');
        if (at <= 0 || at == value.Length - 1)
        {
            throw new ArgumentException(
                "A Repair source location must end with @<positive-line>:<positive-column>.",
                nameof(value));
        }

        var suffix = value.AsSpan(at + 1);
        var separator = suffix.IndexOf(':');
        if (separator <= 0 || separator == suffix.Length - 1 || suffix[(separator + 1)..].Contains(':'))
        {
            throw new ArgumentException(
                "A Repair source location must end with @<positive-line>:<positive-column>.",
                nameof(value));
        }

        if (!int.TryParse(
                suffix[..separator],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var line)
            || !int.TryParse(
                suffix[(separator + 1)..],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var column)
            || line < 1
            || column < 1)
        {
            throw new ArgumentException(
                "A Repair source location line and column must be positive decimal integers.",
                nameof(value));
        }

        return new RepairSourceLocation(value[..at], line, column);
    }

    public override string ToString()
        => string.Create(
            CultureInfo.InvariantCulture,
            $"{SourceCanonicalPath}@{Line}:{Column}");
}
