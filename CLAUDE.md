# CLAUDE.md — Coração Evangelho API

> Referência técnica para IAs (Claude, Copilot, etc.) que trabalham neste repositório.
> Leia este arquivo antes de qualquer modificação no código.

---

## 1. Visão Geral

**Projeto:** Coração Evangelho — plataforma PWA de conteúdo cristão/evangélico  
**Backend:** ASP.NET Core 8 Web API (.NET 8)  
**Repositório frontend:** github.com/denisgraciano/denisgraciano-coracaoevangelho  
**Banco de dados:** MySQL 8+ via Pomelo.EntityFrameworkCore.MySql  

---

## 2. Stack Obrigatória

| Pacote | Versão | Finalidade |
|--------|--------|-----------|
| Microsoft.EntityFrameworkCore | 8.0.0 | ORM |
| Pomelo.EntityFrameworkCore.MySql | 8.0.0 | Driver MySQL |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | Auth JWT |
| AutoMapper | 13.0.1 | Entidade → DTO |
| FluentValidation.AspNetCore | 11.3.0 | Validação de input |
| Swashbuckle.AspNetCore | 6.5.0 | Swagger/OpenAPI |
| Serilog.AspNetCore | 8.0.0 | Logs estruturados |
| BCrypt.Net-Next | 4.0.3 | Hash de senhas |

---

## 3. Estrutura de Pastas

```
src/CoracaoEvangelho.API/
├── Controllers/          # Um arquivo por controller
├── Services/
│   └── Interfaces/       # I{Nome}Service.cs
├── Repositories/
│   └── Interfaces/       # I{Nome}Repository.cs
├── Models/               # Entidades EF Core (sem DTOs aqui)
├── DTOs/
│   ├── Request/          # {Nome}RequestDto
│   └── Response/         # {Nome}ResponseDto + ApiResponse<T>
├── Mappings/             # MappingProfile.cs (AutoMapper)
├── Middlewares/          # ExceptionHandlingMiddleware.cs
├── Validators/           # FluentValidation validators
├── Extensions/           # ClaimsPrincipalExtensions.cs
├── Data/                 # AppDbContext.cs
└── Program.cs            # Bootstrap e DI
```

---

## 4. Convenções Obrigatórias

### Nomenclatura
- Controllers: `PascalCase + Controller` → `LivroController`
- Services: `ILivroService` / `LivroService`
- Repositories: `ILivroRepository` / `LivroRepository`
- DTOs Request: `NomeRequestDto`
- DTOs Response: `NomeResponseDto`
- Migrations: sempre via `dotnet ef migrations add NomeMigration`

### Padrão de Resposta
**TODAS** as respostas devem usar o wrapper `ApiResponse<T>`:

```json
{
  "success": true,
  "data": {},
  "message": "string",
  "errors": []
}
```

### HTTP Status Codes
| Situação | Status |
|----------|--------|
| Leitura com sucesso | 200 OK |
| Criação com sucesso | 201 Created |
| Deleção com sucesso | 204 NoContent |
| Validação falhou | 400 Bad Request |
| Token inválido/ausente | 401 Unauthorized |
| Sem permissão | 403 Forbidden |
| Recurso não encontrado | 404 Not Found |
| Email duplicado / conflito | 409 Conflict |
| Erro interno | 500 Internal Server Error |

---

## 5. Contrato com o Frontend Angular

### ⚠️ Regra de ouro dos DTOs
> Nunca adicionar campos no `ResponseDto` que o frontend não consome.  
> Nunca remover campos que o frontend já usa.  
> Isso quebraria a integração sem erro de compilação.

### Tabela de contratos

| Service Angular | Controller .NET | Método | Rota |
|----------------|-----------------|--------|------|
| `LivroService.getLivros()` | `LivroController` | GET | `/api/livros` |
| `LivroService.getCapitulo(id,cap)` | `LivroController` | GET | `/api/livros/{id}/capitulos/{cap}` |
| `LivroService.pesquisar(termo)` | `VersiculoController` | GET | `/api/versiculos/pesquisar?termo=` |
| `FavoritosService.getFavoritos()` | `FavoritosController` | GET | `/api/favoritos` |
| `FavoritosService.adicionar(v)` | `FavoritosController` | POST | `/api/favoritos` |
| `FavoritosService.remover(id)` | `FavoritosController` | DELETE | `/api/favoritos/{id}` |
| `ConfiguracaoService.getTema()` | `ConfiguracaoController` | GET | `/api/configuracoes` |
| `ConfiguracaoService.setTema()` | `ConfiguracaoController` | PUT | `/api/configuracoes/tema` |
| `ConfiguracaoService.getFonte()` | `ConfiguracaoController` | GET | `/api/configuracoes` |
| `ConfiguracaoService.setFonte()` | `ConfiguracaoController` | PUT | `/api/configuracoes/fonte` |

### Interfaces TypeScript → Records C#

```
// Angular                          // .NET
interface Livro {                   public record LivroResponseDto(
  id: string;                           string Id,
  titulo: string;                       string Titulo,
  subtitulo: string;                    string Subtitulo,
  capa: string;                         string Capa
}                                   );

interface Versiculo {               public record VersiculoResponseDto(
  id: string;                           string Id,
  numero: number;                       int Numero,
  texto: string;                        string Texto,
  capituloId: string;                   string CapituloId,
  isFavorito?: boolean;                 bool IsFavorito
}                                   );

interface Devocional {              public record DevocionalResponseDto(
  id: string;                           string Id,
  data: string;                         DateOnly Data,
  passagem: string;                     string Passagem,
  reflexao: string;                     string Reflexao,
  versiculo: Versiculo;                 VersiculoResponseDto Versiculo
}                                   );
```

---

## 6. Autenticação JWT

- **Registro:** `POST /api/auth/register`
- **Login:** `POST /api/auth/login` → retorna `AccessToken` (1h) + `RefreshToken` (7d)
- **Refresh:** `POST /api/auth/refresh`
- **Claims no token:** `userId`, `email`, `role`
- **Senhas:** BCrypt com work factor 12

### Rotas públicas (sem [Authorize])
- `GET /api/livros/**`
- `GET /api/versiculos/**`
- `GET /api/devocional/**`
- `GET /api/sync/**`
- `POST /api/auth/**`
- `GET /health`

### Rotas protegidas (exigem [Authorize])
- `GET|POST|DELETE /api/favoritos/**`
- `GET|PUT /api/configuracoes/**`

---

## 7. Banco de Dados

### Entidades principais

| Entidade | Tabela EF | Observações |
|----------|-----------|-------------|
| `Livro` | `Livros` | Soft delete (`Deletado`), `QueryFilter` ativo |
| `Capitulo` | `Capitulos` | Index único `(LivroId, Numero)` |
| `Versiculo` | `Versiculos` | Coluna `TEXT` para `Texto` |
| `Devocional` | `Devocionais` | Index único em `Data` (1 por dia) |
| `Usuario` | `Usuarios` | Index único em `Email`; sem `SenhaHash` nos DTOs |
| `Favorito` | `Favoritos` | Index único `(UsuarioId, VersiculoId)` |

### Migrations
```bash
# Criar migration
dotnet ef migrations add NomeDaMigration --project src/CoracaoEvangelho.API

# Aplicar em desenvolvimento
dotnet ef database update --project src/CoracaoEvangelho.API

# NUNCA aplicar automaticamente em produção
# Use scripts SQL gerados: dotnet ef migrations script
```

---

## 8. Sincronização Offline (PWA)

O endpoint `GET /api/sync/livros?atualizadoApos=2026-01-01` suporta:
- Registros com `deletado: true` para remoções (soft delete)
- Headers `ETag` e `Last-Modified` para cache HTTP
- `IgnoreQueryFilters()` no repositório para ver registros deletados

---

## 9. Checklist de Code Review

Antes de criar ou modificar qualquer endpoint, verifique:

- [ ] **N+1 queries:** Usar `Include()` + `AsNoTracking()` em leituras; HashSet para verificar favoritos
- [ ] **DbContext:** Nunca injetar fora do escopo Scoped (não usar em Singleton)
- [ ] **Lógica no Controller:** Zero lógica de negócio — apenas orquestração de Service + retorno HTTP
- [ ] **CancellationToken:** Todos os métodos `async` devem receber e propagar `ct`
- [ ] **[Authorize]:** Todas as rotas de favoritos e configurações devem ter o atributo
- [ ] **SenhaHash:** Jamais deve aparecer em nenhum ResponseDto
- [ ] **Contrato Angular:** O ResponseDto deve espelhar exatamente a interface TypeScript
- [ ] **FluentValidation:** Todo RequestDto deve ter um Validator registrado
- [ ] **ApiResponse<T>:** Toda resposta usa o wrapper — nunca retornar objeto cru
- [ ] **Swagger:** Todo endpoint deve ter `[SwaggerOperation]` e `[ProducesResponseType]`

---

## 10. Como Rodar Localmente

### Pré-requisitos
- .NET 8 SDK
- MySQL 8+ (ou Docker: `docker run -e MYSQL_ROOT_PASSWORD=root -p 3306:3306 mysql:8`)

### Passos

```bash
# 1. Clonar e entrar na pasta
cd src/CoracaoEvangelho.API

# 2. Configurar connection string e JWT key no appsettings.Development.json
#    (nunca commitar credenciais reais)

# 3. Criar o banco
dotnet ef database update

# 4. Rodar
dotnet run

# 5. Acessar Swagger
# http://localhost:5000/swagger
```

### Variáveis de ambiente em produção
```bash
ConnectionStrings__DefaultConnection="Server=...;Database=...;User=...;Password=..."
Jwt__Key="sua-chave-super-segura-32-chars-minimo"
Jwt__Issuer="CoracaoEvangelho.API"
Jwt__Audience="CoracaoEvangelho.Frontend"
```

---

## 11. Decisões de Arquitetura

| Decisão | Motivo |
|---------|--------|
| Repository Pattern | Desacopla EF Core dos Services; facilita testes unitários |
| Records para DTOs | Imutabilidade + sintaxe concisa; sem setter acidental |
| HashSet para isFavorito | Evita N+1: uma query por request, não uma por versículo |
| Soft delete com QueryFilter | `IgnoreQueryFilters()` só no endpoint de sync |
| ClockSkew = Zero no JWT | Token expira exatamente em 1h sem tolerância |
| BCrypt work factor 12 | Equilíbrio entre segurança e velocidade em web APIs |
| CamelCase JSON global | Compatibilidade com convenção padrão Angular/TypeScript |
| CORS explícito | Aceita apenas origens do frontend Angular — nunca `*` em prod |
