# VivendoSobreo

## Fontes

- Consulte o Atlas no projeto `vivendo-sobre-o` e confirme estado no Git.
- Fluxo: `docs/adr/0001-adocao-policy-v4.md`.
- O projeto atual é `src/SoPorHoje.App/SoPorHoje.App.csproj`; não renomeie app ou namespaces incidentalmente.

## Projeto e gates

- Trunk: `master`.
- Stack: .NET 9 MAUI.
- SQLite, notificações e dados locais exigem preservação e teste real.

Windows:

```powershell
dotnet build src/SoPorHoje.App/SoPorHoje.App.csproj -f net9.0-windows10.0.19041.0 --configuration Release
```

Android:

```powershell
dotnet build src/SoPorHoje.App/SoPorHoje.App.csproj -f net9.0-android --configuration Release
```

Não há testes ou CI confiável documentados. Faça smoke da plataforma afetada e registre a limitação.

Preserve WIP. Uma tarefa usa issue, branch e PR. Só conclua após build, smoke, mergeabilidade, merge, limpeza e atualização do Atlas.
