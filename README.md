# Coração Evangelho — API

Backend da plataforma PWA de conteúdo cristão/evangélico.  
**Stack:** .NET 8 · ASP.NET Core · EF Core 8 · MySQL · JWT · AutoMapper · FluentValidation · Swagger

---

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MySQL 8+ (local ou Docker)

```bash
# MySQL via Docker (desenvolvimento)
docker run --name coracao-db \
  -e MYSQL_ROOT_PASSWORD=root \
  -e MYSQL_DATABASE=coracao_evangelho_dev \
  -p 3306:3306 \
  -d mysql:8
```

---

## Configuração

Edite `src/CoracaoEvangelho.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=coracao_evangelho_dev;User=root;Password=root;CharSet=utf8mb4;"
  },
  "Jwt": {
    "Key": "sua-chave-secreta-com-minimo-32-chars",
    "Issuer": "CoracaoEvangelho.API",
    "Audience": "CoracaoEvangelho.Frontend"
  }
}
```

> ⚠️ Em produção, use variáveis de ambiente — nunca commite credenciais.

---

## Rodando localmente

```bash
cd src/CoracaoEvangelho.API

# Restaurar dependências
dotnet restore

# Criar/atualizar banco de dados
dotnet ef database update

# Iniciar API
dotnet run
```

Acesse o Swagger em: **http://localhost:5000/swagger**

---

## Endpoints principais

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| POST | `/api/auth/register` | ❌ | Registra usuário |
| POST | `/api/auth/login` | ❌ | Login → JWT + RefreshToken |
| POST | `/api/auth/refresh` | ❌ | Renova AccessToken |
| GET | `/api/livros` | ❌ | Lista livros |
| GET | `/api/livros/{id}` | ❌ | Detalhe de livro |
| GET | `/api/livros/{id}/capitulos/{n}` | ❌ | Capítulo com versículos |
| GET | `/api/versiculos/pesquisar?termo=` | ❌ | Pesquisa full-text |
| GET | `/api/devocional/hoje` | ❌ | Devocional do dia |
| GET | `/api/devocional/historico` | ❌ | Histórico paginado |
| GET | `/api/favoritos` | ✅ | Lista favoritos |
| POST | `/api/favoritos` | ✅ | Adiciona favorito |
| DELETE | `/api/favoritos/{id}` | ✅ | Remove favorito |
| GET | `/api/configuracoes` | ✅ | Configurações do usuário |
| PUT | `/api/configuracoes/tema` | ✅ | Atualiza tema |
| PUT | `/api/configuracoes/fonte` | ✅ | Atualiza fonte |
| GET | `/api/sync/livros?atualizadoApos=` | ❌ | Sync PWA offline |
| GET | `/health` | ❌ | Health check |

---

## Migrations

```bash
# Nova migration
dotnet ef migrations add NomeDaMigration --project src/CoracaoEvangelho.API

# Aplicar (apenas em desenvolvimento)
dotnet ef database update --project src/CoracaoEvangelho.API

# Gerar SQL para produção (revisar antes de executar!)
dotnet ef migrations script --project src/CoracaoEvangelho.API -o migrations.sql
```

> ⚠️ **NUNCA** usar `database update` ou `MigrateAsync()` automaticamente em produção.

---

## Documentação para IA

Consulte o arquivo [`CLAUDE.md`](./CLAUDE.md) antes de modificar o código.
