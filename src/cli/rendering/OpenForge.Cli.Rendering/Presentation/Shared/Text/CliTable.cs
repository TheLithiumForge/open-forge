using System.Text;

namespace OpenForge.Cli.Core.Presentation.Shared.Text;

internal static class CliTable
{
    internal static string Render(IReadOnlyList<IReadOnlyList<string>> rows, Func<int, string, string>? styleCell = null)
    {
        ArgumentNullException.ThrowIfNull(rows);
        if (rows.Count == 0)
        {
            return string.Empty;
        }

        var escaped = rows.Select(row => row.Select(CliText.Escape).ToArray()).ToArray();
        var columns = escaped.Max(row => row.Length);
        var widths = Enumerable.Range(0, columns)
            .Select(column => escaped.Max(row => column < row.Length ? row[column].EnumerateRunes().Count() : 0)).ToArray();
        var builder = new StringBuilder();
        foreach (var row in escaped)
        {
            builder.Append("  ");
            for (var column = 0; column < row.Length; column++)
            {
                builder.Append(styleCell is null ? row[column] : styleCell(column, row[column]));
                if (column + 1 < row.Length)
                {
                    builder.Append(' ', widths[column] - row[column].EnumerateRunes().Count() + 2);
                }
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }
}
