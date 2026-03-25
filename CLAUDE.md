# CLAUDE.md — Coração Evangelho API

> Referência técnica para IAs (Claude, Copilot, etc.) que trabalham neste repositório.
> **Leia este arquivo inteiro antes de qualquer modificação no código.**

---

## 1. Visão Geral

**Projeto:** Coração Evangelho — plataforma PWA de cursos cristão/evangélico
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
| FluentValidation.AspNetCore | 11.3.0 | Validação de input |
| Swashbuckle.AspNetCore | 6.5.0 | Swagger/OpenAPI |
| Serilog.AspNetCore | 8.0.0 | Logs estruturados |
| BCrypt.Net-Next | 4.0.3 | Hash de senhas |

> **Proibido** adicionar novos pacotes sem justificativa documentada neste arquivo.

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
│   ├── Request/          # {Nome}RequestDto.cs
│   └── Response/         # {Nome}ResponseDto.cs + ApiResponse<T>
├── Middlewares/          # ExceptionHandlingMiddleware.cs
├── Validators/           # FluentValidation validators
├── Extensions/           # ClaimsPrincipalExtensions.cs
├── Data/                 # AppDbContext.cs + DbSeeder.cs
└── Program.cs            # Bootstrap e DI
tests/CoracaoEvangelho.API.Tests/
├── Services/             # Testes unitários de cada Service
└── Validators/           # Testes de cada Validator
```

> **Regra:** um arquivo por responsabilidade. Nunca juntar múltiplas classes em um único `.cs`.

---

## 4. Convenções Obrigatórias de Nomenclatura

### Classes e Interfaces
| Tipo | Padrão | Exemplo |
|------|--------|---------|
| Controller | `PascalCase + Controller` | `CursoController` |
| Service | `I{Nome}Service` / `{Nome}Service` | `IMatriculaService` / `MatriculaService` |
| Repository | `I{Nome}Repository` / `{Nome}Repository` | `ICursoRepository` / `CursoRepository` |
| DTO Request | `{Nome}RequestDto` | `MatriculaRequestDto` |
| DTO Response | `{Nome}ResponseDto` | `CursoResponseDto` |
| Validator | `{Nome}Validator` | `MatriculaRequestValidator` |
| Entidade EF | Substantivo singular PascalCase | `Matricula`, `Certificado` |
| Middleware | `{Nome}Middleware` | `ExceptionHandlingMiddleware` |

### Métodos
- **Verbos descritivos** em PascalCase: `ObterCursoComAulasAsync`, `RegistrarMatriculaAsync`
- **Métodos async** terminam obrigatoriamente em `Async`: `BuscarPorIdAsync`, não `BuscarPorId`
- **Getters de repositório** seguem: `GetByIdAsync`, `GetByEmailAsync`, `GetTrackedByIdAsync`
- Nunca abreviar: `GetUsrById` é proibido — use `GetByIdAsync` ou `GetUsuarioByIdAsync`

### Variáveis e Parâmetros
- `camelCase` para variáveis locais e parâmetros
- Nomes descritivos mesmo para variáveis de loop: `foreach (var curso in cursos)`, não `foreach (var c in l)`
- Evitar sufixo genérico `Temp`, `Aux`, `Obj`: use o nome do conceito — `cursoExistente`, `matriculaAtualizada`
- **Proibido:** variáveis de uma letra exceto índices numéricos (`i`, `j`)
- `ct` é o único apelido permitido para `CancellationToken`

### Constantes e Magic Values
- **Proibido usar strings ou números mágicos diretamente no código**
- Roles em constantes: `Roles.Aluno`, `Roles.Admin` (nunca `"aluno"`, `"admin"` espalhados)
- Durações JWT em constantes nomeadas: `JwtSettings.AccessTokenHours`, `JwtSettings.RefreshTokenDays`

---

## 5. Princípios Clean Code — Regras Não Negociáveis

### SOLID

| Princípio | Regra prática neste projeto |
|-----------|----------------------------|
| **S** — Single Responsibility | Cada classe faz uma coisa. `CursoService` trata apenas regras de cursos. |
| **O** — Open/Closed | Extensão via novas classes/interfaces; nunca modifique um contrato existente. |
| **L** — Liskov Substitution | Implementações de `IXxxService` devem ser intercambiáveis sem quebrar comportamento. |
| **I** — Interface Segregation | Interfaces pequenas e focadas. Se `IUsuarioService` acumular 15 métodos, divida. |
| **D** — Dependency Inversion | Sempre injetar interfaces, nunca classes concretas. |

### Tamanho e Complexidade

- **Métodos:** máximo 20 linhas de corpo. Extraia método se passar disso.
- **Classes:** máximo 200 linhas. Divida se passar.
- **Parâmetros:** máximo 3 por método. Use um DTO/record se precisar de mais.
- **Nesting:** máximo 2 níveis de indentação dentro de um bloco lógico. Use guard clauses.
- **Ciclomática:** máximo 5 caminhos de decisão por método.

### Guard Clauses (Retorno Antecipado)

```csharp
// ERRADO — nesting desnecessário
public async Task<CursoResponseDto> ObterAsync(string id, CancellationToken ct)
{
    var curso = await _repository.GetByIdAsync(id, ct);
    if (curso != null)
    {
        if (curso.Ativo)
        {
            return _mapper.Map<CursoResponseDto>(curso);
        }
        else throw new InvalidOperationException("Curso inativo.");
    }
    else throw new KeyNotFoundException("Curso não encontrado.");
}

// CORRETO — guard clauses
public async Task<CursoResponseDto> ObterAsync(string id, CancellationToken ct)
{
    var curso = await _repository.GetByIdAsync(id, ct)
        ?? throw new KeyNotFoundException("Curso não encontrado.");

    if (!curso.Ativo)
        throw new InvalidOperationException("Curso inativo.");

    return _mapper.Map<CursoResponseDto>(curso);
}
```

### DRY — Don't Repeat Yourself

- Lógica repetida em 2+ lugares → extraia para método privado ou classe auxiliar
- Proibido copiar e colar código entre services
- Queries repetidas → novo método no repository com nome descritivo

### YAGNI — You Aren't Gonna Need It

- Não crie abstrações para uso hipotético futuro
- Não adicione parâmetros opcionais antecipando necessidades futuras
- Não implemente feature não solicitada em qualquer issue/PR

---

## 6. Padrões de Código C#

### Nullable Reference Types

- Projeto compilado com `<Nullable>enable</Nullable>` — respeite sempre
- `string?` para campos realmente opcionais; `string` para obrigatórios
- Nunca usar `!` (null-forgiving) sem comentário explicando por quê é seguro
- `?? throw` para lançar exceção em null inesperado (ver guard clauses)

### Async / Await

- **Todo método que acessa I/O (DB, HTTP) deve ser `async Task<T>`**
- **Proibido `.Result` e `.Wait()`** — causam deadlock em ASP.NET
- **Proibido `async void`** exceto em event handlers
- Sempre propagar `CancellationToken ct` até o último `await` da cadeia

```csharp
// CORRETO
public async Task<MatriculaResponseDto> MatricularAsync(
    string usuarioId, MatriculaRequestDto dto, CancellationToken ct)
{
    var curso = await _cursoRepository.GetByIdAsync(dto.CursoId, ct)
        ?? throw new KeyNotFoundException("Curso não encontrado.");

    var jaMatriculado = await _matriculaRepository.ExisteAsync(usuarioId, dto.CursoId, ct);
    if (jaMatriculado)
        throw new InvalidOperationException("Usuário já está matriculado neste curso.");

    var matricula = new Matricula { UsuarioId = usuarioId, CursoId = dto.CursoId };
    await _matriculaRepository.AddAsync(matricula, ct);
    await _matriculaRepository.SaveChangesAsync(ct);

    return new MatriculaResponseDto(matricula.Id, curso.Titulo, matricula.DataMatricula);
}
```

### Tratamento de Exceções

Use exceções semânticas — **nunca retorne `null` ou `bool` para indicar falha de negócio**:

| Situação | Exceção a lançar | HTTP resultante |
|----------|-----------------|----------------|
| Recurso não encontrado | `KeyNotFoundException` | 404 Not Found |
| Regra de negócio violada / duplicata | `InvalidOperationException` | 409 Conflict |
| Input inválido (fora do Validator) | `ArgumentException` | 400 Bad Request |
| Token inválido / sem permissão | `UnauthorizedAccessException` | 401 Unauthorized |
| Qualquer outra exceção | (não capturar) → Middleware | 500 Internal Server Error |

```csharp
// PROIBIDO — retornar null esconde o motivo da falha
public async Task<Curso?> ObterAsync(string id, CancellationToken ct)
    => await _repository.GetByIdAsync(id, ct);

// CORRETO — exceção com mensagem clara
public async Task<Curso> ObterAsync(string id, CancellationToken ct)
    => await _repository.GetByIdAsync(id, ct)
       ?? throw new KeyNotFoundException($"Curso '{id}' não encontrado.");
```

> O `ExceptionHandlingMiddleware` converte automaticamente essas exceções para `ApiResponse<T>` com o status HTTP correto — **nunca trate exceções nos controllers**.

### Logs Estruturados

```csharp
// CORRETO — interpolação Serilog com propriedades nomeadas
_logger.LogInformation("Matrícula {MatriculaId} criada para usuário {UsuarioId}", matricula.Id, usuarioId);
_logger.LogWarning("Tentativa de matrícula duplicada: usuário {UsuarioId}, curso {CursoId}", usuarioId, cursoId);
_logger.LogError(ex, "Erro ao emitir certificado para {UsuarioId}", usuarioId);

// PROIBIDO — string interpolation no log (perde structured data)
_logger.LogInformation($"Matrícula {matricula.Id} criada");
```

---

## 7. Padrão de Resposta

**TODAS** as respostas usam o wrapper `ApiResponse<T>`:

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
| Duplicata / conflito de negócio | 409 Conflict |
| Erro interno | 500 Internal Server Error |

---

## 8. Contrato com o Frontend Angular

### Regra de ouro dos DTOs
> Nunca adicionar campos no `ResponseDto` que o frontend não consome.
> Nunca remover campos que o frontend já usa.
> Isso quebraria a integração sem erro de compilação.

### Tabela de contratos

| Service Angular | Controller .NET | Método | Rota |
|----------------|-----------------|--------|------|
| `CursoService.listar()` | `CursoController` | GET | `/api/cursos` |
| `CursoService.obter(id)` | `CursoController` | GET | `/api/cursos/{id}` |
| `MatriculaService.matricular(dto)` | `MatriculaController` | POST | `/api/matriculas` |
| `MatriculaService.minhasMatriculas()` | `MatriculaController` | GET | `/api/matriculas/minhas` |
| `ProgressoService.marcarConcluida(dto)` | `ProgressoController` | POST | `/api/progresso` |
| `ProgressoService.obterProgresso(id)` | `ProgressoController` | GET | `/api/progresso/{cursoId}` |
| `CertificadoService.emitir(dto)` | `CertificadoController` | POST | `/api/certificados` |
| `CertificadoService.meusCertificados()` | `CertificadoController` | GET | `/api/certificados/meus` |
| `PedidoVibracaoService.enviar(dto)` | `PedidoVibracaoController` | POST | `/api/pedidos-vibracao` |
| `UsuarioService.perfil()` | `UsuarioController` | GET | `/api/usuarios/perfil` |
| `UsuarioService.atualizarPerfil(dto)` | `UsuarioController` | PUT | `/api/usuarios/perfil` |

### Interfaces TypeScript → Records C#

```
// Angular                              // .NET
interface Curso {                        public record CursoResponseDto(
  id: string;                                string Id,
  titulo: string;                            string Titulo,
  descricao: string;                         string Descricao,
  cargaHoraria: number;                      decimal CargaHoraria,
  totalAulas: number;                        int TotalAulas,
  ativo: boolean;                            bool Ativo
}                                        );

interface Matricula {                    public record MatriculaResponseDto(
  id: string;                                string Id,
  cursoTitulo: string;                       string CursoTitulo,
  dataMatricula: string;                     DateTime DataMatricula,
  percentualConcluido: number;               decimal PercentualConcluido
}                                        );

interface Certificado {                  public record CertificadoResponseDto(
  id: string;                                string Id,
  cursoTitulo: string;                       string CursoTitulo,
  alunoNome: string;                         string AlunoNome,
  dataEmissao: string;                       DateTime DataEmissao,
  cargaHoraria: number;                      decimal CargaHoraria
}                                        );
```

---

## 9. Autenticação JWT

- **Registro:** `POST /api/auth/register`
- **Login:** `POST /api/auth/login` → retorna `AccessToken` (1h) + `RefreshToken` (7d)
- **Refresh:** `POST /api/auth/refresh`
- **Claims no token:** `userId`, `email`, `role`
- **Senhas:** BCrypt com work factor 12
- **ClockSkew = Zero** — token expira exatamente em 1h, sem tolerância

### Rotas públicas (sem `[Authorize]`)
- `GET /api/cursos/**`
- `POST /api/auth/**`
- `POST /api/pedidos-vibracao`
- `GET /health`

### Rotas protegidas (exigem `[Authorize]`)
- `POST|GET /api/matriculas/**`
- `POST|GET /api/progresso/**`
- `POST|GET /api/certificados/**`
- `GET|PUT /api/usuarios/perfil`

---

## 10. Banco de Dados

### Entidades principais

| Entidade | Tabela EF | Observações |
|----------|-----------|-------------|
| `Usuario` | `Usuarios` | Index único em `Email`; sem `SenhaHash` nos DTOs |
| `Categoria` | `Categorias` | FK opcional em `Curso` |
| `Curso` | `Cursos` | Soft delete (`Ativo`), `QueryFilter` ativo |
| `Aula` | `Aulas` | Soft delete (`Ativa`); index único `(CursoId, Ordem)` |
| `Matricula` | `Matriculas` | Index único `(UsuarioId, CursoId)` |
| `Progresso` | `Progressos` | Index único `(UsuarioId, AulaId)`; upsert no repository |
| `Certificado` | `Certificados` | Index único `(UsuarioId, CursoId)`; campos desnormalizados |
| `PedidoVibracao` | `PedidosVibracao` | Email opcional (suporte anônimo) |

### Migrations
```bash
# Criar migration
dotnet ef migrations add NomeDaMigration --project src/CoracaoEvangelho.API

# Aplicar em desenvolvimento
dotnet ef database update --project src/CoracaoEvangelho.API

# NUNCA aplicar automaticamente em produção
# Use scripts SQL gerados: dotnet ef migrations script
```

### Regras de consulta
- **Leituras:** sempre `AsNoTracking()` — tracking é para UPDATE/DELETE
- **Tracking:** usar `GetTrackedByIdAsync()` apenas quando for salvar alterações
- **Joins:** usar `Include()` na query — nunca carregar relacionamentos separadamente
- **N+1:** uma query por request; nunca dentro de loop

---

## 11. Sincronização Offline (PWA)

O endpoint `GET /api/sync/cursos?atualizadoApos=2026-01-01` suporta:
- Registros com `ativo: false` para remoções (soft delete)
- Headers `ETag` e `Last-Modified` para cache HTTP
- `IgnoreQueryFilters()` no repositório para ver registros inativos

---

## 12. Testes

### Estrutura obrigatória

```
tests/CoracaoEvangelho.API.Tests/
├── Services/
│   ├── AuthServiceTests.cs
│   ├── CursoServiceTests.cs
│   ├── MatriculaServiceTests.cs
│   └── ...
└── Validators/
    └── ValidatorTests.cs
```

### Convenção de nomenclatura dos testes

```
{Método}_{Cenário}_{ResultadoEsperado}

Exemplos:
MatricularAsync_CursoNaoEncontrado_ThrowsKeyNotFoundException
MatricularAsync_UsuarioJaMatriculado_ThrowsInvalidOperationException
MatricularAsync_DadosValidos_RetornaMatriculaResponseDto
```

### O que testar

- **Services:** toda lógica de negócio com mocks dos repositories
- **Validators:** casos válidos e inválidos para cada regra
- **Não testar:** controllers (orquestração simples), repositories (dependem de DB)

### Mock strategy

- Use `Moq` ou `NSubstitute` para interfaces de repository
- Nunca use `HttpContext` real em testes de service
- `CancellationToken.None` é aceitável em testes

---

## 13. Checklist de Code Review

Antes de criar ou modificar qualquer endpoint, verifique **todos** os itens:

#### Arquitetura
- [ ] **Zero lógica no Controller** — apenas chama service e retorna HTTP
- [ ] **DbContext Scoped** — nunca injetado em Singleton
- [ ] **Repository Pattern** — service não acessa `AppDbContext` diretamente
- [ ] **CancellationToken** — todos os métodos `async` recebem e propagam `ct`
- [ ] **[Authorize]** — todas as rotas protegidas têm o atributo

#### Clean Code
- [ ] **Métodos ≤ 20 linhas** — extraia método privado se necessário
- [ ] **Guard clauses** — sem nesting desnecessário; retorno antecipado para casos de erro
- [ ] **Sem magic strings** — roles, claims e constantes usam classes de constantes
- [ ] **Nomes descritivos** — métodos, variáveis e parâmetros autoexplicativos
- [ ] **Async correto** — sufixo `Async`, sem `.Result`/`.Wait()`, sem `async void`
- [ ] **Sem código comentado** — código morto deve ser deletado, não comentado

#### Segurança
- [ ] **SenhaHash ausente** — jamais em nenhum ResponseDto
- [ ] **Validação de input** — todo RequestDto tem Validator registrado
- [ ] **CORS explícito** — nunca `AllowAnyOrigin()` em produção

#### Contrato e Resposta
- [ ] **ApiResponse<T>** — toda resposta usa o wrapper
- [ ] **Status HTTP correto** — conforme tabela da seção 7
- [ ] **Contrato Angular** — ResponseDto espelha exatamente a interface TypeScript
- [ ] **Swagger** — todo endpoint tem `[SwaggerOperation]` e `[ProducesResponseType]`

#### Banco de Dados
- [ ] **AsNoTracking()** em todas as queries de leitura
- [ ] **Include() antecipado** — sem lazy loading; sem N+1
- [ ] **Migrations nomeadas** — nome descreve a mudança: `AdicionaColunaTelefoneUsuario`

#### Testes
- [ ] **Novo service** → testes unitários criados em `tests/Services/`
- [ ] **Novo validator** → testes criados em `tests/Validators/`
- [ ] **Regressão** — nenhum teste existente foi quebrado

---

## 14. Anti-Patterns Proibidos

```csharp
// PROIBIDO: lógica de negócio no controller
[HttpPost]
public async Task<IActionResult> Matricular(MatriculaRequestDto dto)
{
    var curso = await _context.Cursos.FindAsync(dto.CursoId); // acesso direto ao DB
    if (curso == null) return NotFound();
    // ...
}

// PROIBIDO: retornar null para indicar falha
public async Task<Matricula?> MatricularAsync(...) => null;

// PROIBIDO: capturar Exception genérica nos services
catch (Exception ex) { return null; }

// PROIBIDO: async sem await (código síncrono)
public async Task<CursoResponseDto> ObterAsync(string id)
    => new CursoResponseDto(id, "Teste"); // nunca usa await

// PROIBIDO: injetar DbContext diretamente no service
public class CursoService(AppDbContext db) { }

// PROIBIDO: magic string de role espalhada
if (User.IsInRole("admin")) { }   // use Roles.Admin

// PROIBIDO: log com interpolação
_logger.LogInformation($"Curso {id} criado");  // use template

// PROIBIDO: .Result ou .Wait() em código async
var curso = _repository.GetByIdAsync(id).Result;
```

---

## 15. Decisões de Arquitetura

| Decisão | Motivo |
|---------|--------|
| Repository Pattern | Desacopla EF Core dos Services; facilita mocks em testes |
| Records para DTOs | Imutabilidade + sintaxe concisa; sem setter acidental |
| HashSet para verificação de coleções | Evita N+1: uma query por request, não uma por item |
| Soft delete com QueryFilter | `IgnoreQueryFilters()` só em endpoints de sync/admin |
| ClockSkew = Zero no JWT | Token expira exatamente em 1h sem tolerância |
| BCrypt work factor 12 | Equilíbrio entre segurança e velocidade em web APIs |
| CamelCase JSON global | Compatibilidade com convenção padrão Angular/TypeScript |
| CORS explícito | Aceita apenas origens do frontend Angular — nunca `*` em prod |
| Exceções semânticas | `KeyNotFoundException`, `InvalidOperationException`, etc. mapeadas no middleware |
| `GetTrackedByIdAsync` separado | Deixa explícito quando tracking é necessário — clareza de intenção |

---

## 16. Como Rodar Localmente

### Pré-requisitos
- .NET 8 SDK
- MySQL 8+ (ou Docker: `docker run -e MYSQL_ROOT_PASSWORD=root -p 3306:3306 mysql:8`)

### Passos

```bash
# 1. Entrar na pasta da API
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
