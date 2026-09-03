import eslint from "@eslint/js";
import eslintConfigPrettier from "eslint-config-prettier";
import typescriptEslint from "typescript-eslint";

const authoredSourceFiles = ["**/*.{js,mjs,cjs,ts,mts,cts}"];
const frozenCliBuildFiles = ["src/cli-mvp/build/**/*.ts"];
const typeOnlyModuleFiles = ["**/*.types.ts"];

const importSyntaxRestrictions = [
  {
    selector: "TSImportType",
    message: "Use a named static type import instead of an import type expression.",
  },
  {
    selector: "ImportExpression",
    message: "Use a static import unless this is an explicitly justified asynchronous lazy boundary.",
  },
];

const typeOnlyModuleMessage = "A *.types.ts module may contain only type declarations and type-only imports or exports.";
const typeOnlyModuleRestrictions = [
  {
    selector: [
      "Program > :not(ImportDeclaration[importKind='type'])",
      ":not(TSInterfaceDeclaration)",
      ":not(TSTypeAliasDeclaration)",
      ":not(ExportNamedDeclaration[exportKind='type'])",
      ":not(ExportNamedDeclaration[declaration!=null])",
      ":not(ExportNamedDeclaration[declaration=null]:not(:has(ExportSpecifier:not([exportKind='type']))))",
      ":not(ExportAllDeclaration[exportKind='type'])",
      ":not(ExportDefaultDeclaration)",
      ":not(EmptyStatement)",
    ].join(""),
    message: typeOnlyModuleMessage,
  },
  {
    selector: ["Program > :matches(ExportNamedDeclaration[declaration!=null], ExportDefaultDeclaration)", " > :not(TSInterfaceDeclaration):not(TSTypeAliasDeclaration)"].join(""),
    message: typeOnlyModuleMessage,
  },
];

const typeCheckedFrozenCliBuildConfigs = [...typescriptEslint.configs.strictTypeChecked, ...typescriptEslint.configs.stylisticTypeChecked].map((configuration) => ({
  ...configuration,
  files: frozenCliBuildFiles,
}));

export default typescriptEslint.config(
  {
    ignores: ["artifacts/**", "coverage/**", "dist/**", "node_modules/**", ".agents/memory/archived/**", ".temp/**"],
  },
  {
    ...eslint.configs.recommended,
    files: frozenCliBuildFiles,
  },
  ...typeCheckedFrozenCliBuildConfigs,
  {
    files: frozenCliBuildFiles,
    languageOptions: {
      parserOptions: {
        project: ["./tsconfig.json"],
        tsconfigRootDir: import.meta.dirname,
      },
    },
  },
  {
    files: authoredSourceFiles,
    languageOptions: {
      parser: typescriptEslint.parser,
    },
    plugins: {
      "@typescript-eslint": typescriptEslint.plugin,
    },
    rules: {
      "@typescript-eslint/consistent-type-imports": [
        "error",
        {
          disallowTypeAnnotations: true,
          fixStyle: "separate-type-imports",
          prefer: "type-imports",
        },
      ],
      "no-restricted-syntax": ["error", ...importSyntaxRestrictions],
    },
  },
  {
    files: typeOnlyModuleFiles,
    rules: {
      "no-restricted-syntax": ["error", ...importSyntaxRestrictions, ...typeOnlyModuleRestrictions],
    },
  },
  eslintConfigPrettier,
);
