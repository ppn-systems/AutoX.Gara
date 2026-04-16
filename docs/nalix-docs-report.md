# Nalix Docs Gap Report

Date: 2026-04-16

## Issue 1: `Pbkdf2` usage does not include namespace import

- Location: `nalix-docs/api/security/cryptography.md`
- Current state:
  - The page recommends using `Pbkdf2` and shows `Pbkdf2.Hash(...)`.
  - It does not show the namespace import needed to compile.
- Impact:
  - Migration/refactor can fail with `CS0103: The name 'Pbkdf2' does not exist in the current context`.
  - Teams may incorrectly guess old namespaces (`...Security.Credentials`) that no longer resolve.
- Suggested fix in docs:
  - Add explicit import in quick example:
    - `using Nalix.Framework.Security;`
  - Add one-line note that `Pbkdf2` belongs to `Nalix.Framework.Security`.

