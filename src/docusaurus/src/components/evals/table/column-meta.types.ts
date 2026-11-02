import type { RowData } from "@tanstack/react-table";

declare module "@tanstack/react-table" {
  // The library merges this interface into every column definition, so its type parameters have to match the original.
  interface ColumnMeta<TData extends RowData, TValue> {
    /** A plain name for the sort button when the header holds its own buttons, such as a definition label. */
    readonly label?: string;
  }
}
