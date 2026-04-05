using CoracaoEvangelho.API.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Models;
using CoracaoEvangelho.API.Repositories.Interfaces;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace CoracaoEvangelho.API.Services;

// ── AuthService ───────────────────────────────────────────────
public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IConfiguration _config;

    public AuthService(IUsuarioRepository usuarioRepo, IConfiguration config)
    {
        _usuarioRepo = usuarioRepo;
        _config = config;
    }

    public async Task<AuthResponseDto> RegistrarAsync(
        RegisterRequestDto dto, CancellationToken ct = default)
    {
        var emailNorm = dto.Email.ToLower().Trim();

        if (await _usuarioRepo.GetByEmailAsync(emailNorm, ct) is not null)
            throw new InvalidOperationException("E-mail já cadastrado.");

        var usuario = new Usuario
        {
            Email        = emailNorm,
            Nome         = dto.Nome.Trim(),
            SenhaHash    = BCrypt.Net.BCrypt.HashPassword(dto.Senha, workFactor: 12),
            Role         = "aluno",
            DataCadastro = DateTime.UtcNow
        };

        GerarRefreshToken(usuario);
        await _usuarioRepo.AddAsync(usuario, ct);
        await _usuarioRepo.SaveChangesAsync(ct);
        return GerarAuthResponse(usuario);
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto dto, CancellationToken ct = default)
    {
        // GetByEmailAsync retorna entidade COM tracking — podemos salvar RefreshToken
        var usuario = await _usuarioRepo.GetByEmailAsync(dto.Email.ToLower().Trim(), ct)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!usuario.Ativo)
            throw new UnauthorizedAccessException("Conta desativada. Entre em contato com o suporte.");

        GerarRefreshToken(usuario);
        await _usuarioRepo.SaveChangesAsync(ct);
        return GerarAuthResponse(usuario);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(
        string refreshToken, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByRefreshTokenAsync(refreshToken, ct)
            ?? throw new UnauthorizedAccessException("Refresh token inválido ou expirado.");

        GerarRefreshToken(usuario);
        await _usuarioRepo.SaveChangesAsync(ct);
        return GerarAuthResponse(usuario);
    }

    // ── Privados ──────────────────────────────────────────────

    private AuthResponseDto GerarAuthResponse(Usuario usuario)
    {
        var (token, expira) = GerarJwt(usuario);
        return new AuthResponseDto(
            AccessToken:  token,
            Expira:       expira,
            RefreshToken: usuario.RefreshToken!,
            Usuario:      new UsuarioResponseDto(
                              usuario.Id, usuario.Nome,
                              usuario.Email, usuario.AvatarUrl));
    }

    private (string token, DateTime expira) GerarJwt(Usuario usuario)
    {
        var jwtKey = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key não configurada.");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   usuario.Id),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Role,               usuario.Role),
            new Claim("userId",                      usuario.Id),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddHours(1);

        var token = new JwtSecurityToken(
            issuer:            _config["Jwt:Issuer"],
            audience:          _config["Jwt:Audience"],
            claims:            claims,
            expires:           expira,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }

    private static void GerarRefreshToken(Usuario usuario)
    {
        // 64 bytes de entropia criptográfica → base64 de 88 chars
        usuario.RefreshToken       = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        usuario.RefreshTokenExpira = DateTime.UtcNow.AddDays(7);
    }
}

// ── UsuarioService ────────────────────────────────────────────
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepo;

    public UsuarioService(IUsuarioRepository usuarioRepo) => _usuarioRepo = usuarioRepo;

    public async Task<UsuarioResponseDto> GetPerfilAsync(
        string usuarioId, CancellationToken ct = default)
    {
        // AsNoTracking — só leitura
        var usuario = await _usuarioRepo.GetByIdAsync(usuarioId, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        return Map(usuario);
    }

    public async Task<UsuarioResponseDto> AtualizarPerfilAsync(
        string usuarioId, AtualizarPerfilRequestDto dto, CancellationToken ct = default)
    {
        // GetTrackedByIdAsync — precisa do tracking para salvar
        var usuario = await _usuarioRepo.GetTrackedByIdAsync(usuarioId, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        usuario.Nome = dto.Nome.Trim();

        if (dto.AvatarUrl is not null)
            usuario.AvatarUrl = dto.AvatarUrl;

        await _usuarioRepo.SaveChangesAsync(ct);
        return Map(usuario);
    }

    public async Task AlterarSenhaAsync(
        string usuarioId, AlterarSenhaRequestDto dto, CancellationToken ct = default)
    {
        // GetTrackedByIdAsync — vai salvar SenhaHash
        var usuario = await _usuarioRepo.GetTrackedByIdAsync(usuarioId, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(dto.SenhaAtual, usuario.SenhaHash))
            throw new UnauthorizedAccessException("Senha atual incorreta.");

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha, workFactor: 12);
        await _usuarioRepo.SaveChangesAsync(ct);
    }

    private static UsuarioResponseDto Map(Usuario u) =>
        new(u.Id, u.Nome, u.Email, u.AvatarUrl);
}

// ── CursoService ──────────────────────────────────────────────
public class CursoService : ICursoService
{
    private readonly ICursoRepository _cursoRepo;

    public CursoService(ICursoRepository cursoRepo) => _cursoRepo = cursoRepo;

    public async Task<IEnumerable<CursoResumoResponseDto>> GetTodosAsync(
        CancellationToken ct = default)
    {
        var cursos = await _cursoRepo.GetAllAsync(ct);
        return cursos.Select(MapResumo);
    }

    public async Task<CursoResponseDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var curso = await _cursoRepo.GetByIdComAulasAsync(id, ct);
        return curso is null ? null : MapCompleto(curso);
    }

    public async Task<IEnumerable<CursoResponseDto>> GetCursosMatriculadosAsync(
        string usuarioId, CancellationToken ct = default)
    {
        var cursos = await _cursoRepo.GetCursosMatriculadosAsync(usuarioId, ct);
        return cursos.Select(MapCompleto);
    }

    public async Task<IEnumerable<CursoResumoResponseDto>> GetSugestoesAsync(
        string usuarioId, CancellationToken ct = default)
    {
        var cursos = await _cursoRepo.GetSugestoesAsync(usuarioId, 3, ct);
        return cursos.Select(MapResumo);
    }

    public async Task<CursoResponseDto> CriarAsync(
        CursoRequestDto dto, CancellationToken ct = default)
    {
        var curso = new Curso
        {
            Titulo                 = dto.Titulo,
            Descricao              = dto.Descricao,
            CategoriaId            = dto.CategoriaId,
            ImagemUrl              = dto.ImagemUrl,
            Instrutor              = dto.Instrutor,
            CertificadoDisponivel  = dto.CertificadoDisponivel,
            CriadoEm              = DateTime.UtcNow,
            AtualizadoEm          = DateTime.UtcNow
        };

        await _cursoRepo.AddAsync(curso, ct);
        await _cursoRepo.SaveChangesAsync(ct);
        return MapCompleto(curso);
    }

    // ── Mapeamentos privados ───────────────────────────────────

    private static CursoResumoResponseDto MapResumo(Curso c) =>
        new(c.Id, c.Titulo, c.Descricao,
            c.Categoria?.Nome ?? string.Empty,
            c.CategoriaId,
            c.ImagemUrl, c.Instrutor,
            c.Aulas.Count(a => a.Ativa),
            c.CertificadoDisponivel);

    private static CursoResponseDto MapCompleto(Curso c) =>
        new(c.Id, c.Titulo, c.Descricao,
            c.Categoria?.Nome ?? string.Empty,
            c.CategoriaId,
            c.ImagemUrl, c.Instrutor,
            c.Aulas.Count(a => a.Ativa),
            c.CertificadoDisponivel,
            c.Aulas
                .Where(a => a.Ativa)
                .OrderBy(a => a.Ordem)
                .Select(a => new AulaResponseDto(
                    a.Id, a.Titulo, a.Descricao,
                    a.YoutubeVideoId, a.DuracaoMinutos, a.Ordem)));
}

// ── MatriculaService ──────────────────────────────────────────
public class MatriculaService : IMatriculaService
{
    private readonly IMatriculaRepository _matriculaRepo;
    private readonly ICursoRepository     _cursoRepo;

    public MatriculaService(
        IMatriculaRepository matriculaRepo,
        ICursoRepository cursoRepo)
    {
        _matriculaRepo = matriculaRepo;
        _cursoRepo     = cursoRepo;
    }

    public async Task<MatriculaResponseDto> InscreverAsync(
        string? usuarioId, string cursoId,
        MatriculaRequestDto dto, CancellationToken ct = default)
    {
        var curso = await _cursoRepo.GetByIdComAulasAsync(cursoId, ct)
            ?? throw new KeyNotFoundException($"Curso '{cursoId}' não encontrado.");

        var emailNorm = dto.Email.Trim().ToLower();

        var jaMatriculado = await _matriculaRepo
            .GetByEmailCursoAsync(emailNorm, cursoId, ct);

        if (jaMatriculado is not null)
            throw new InvalidOperationException("Este e-mail já possui uma inscrição ativa neste curso.");

        var matricula = new Matricula
        {
            UsuarioId      = usuarioId,
            CursoId        = cursoId,
            DataMatricula  = DateTime.UtcNow,
            Ativa          = true,
            NomeCompleto   = dto.NomeCompleto.Trim(),
            Email          = emailNorm,
            Telefone       = dto.Telefone?.Trim(),
            Cpf            = dto.Cpf?.Trim(),
            DataNascimento = dto.DataNascimento,
            Observacoes    = dto.Observacoes?.Trim(),
            AceitaTermos   = dto.AceitaTermos,
            ReceberEmails  = dto.ReceberEmails,
            Cep            = dto.Endereco?.Cep,
            Logradouro     = dto.Endereco?.Logradouro,
            Numero         = dto.Endereco?.Numero,
            Complemento    = dto.Endereco?.Complemento,
            Bairro         = dto.Endereco?.Bairro,
            Cidade         = dto.Endereco?.Cidade,
            Estado         = dto.Endereco?.Estado
        };

        await _matriculaRepo.AddAsync(matricula, ct);
        await _matriculaRepo.SaveChangesAsync(ct);

        return new MatriculaResponseDto(
            matricula.Id, cursoId, curso.Titulo,
            matricula.DataMatricula, matricula.Ativa);
    }

    public async Task<bool> EstaMatriculadoAsync(
        string usuarioId, string cursoId, CancellationToken ct = default)
    {
        var m = await _matriculaRepo.GetByUsuarioCursoAsync(usuarioId, cursoId, ct);
        return m is { Ativa: true };
    }
}

// ── ProgressoService ──────────────────────────────────────────
public class ProgressoService : IProgressoService
{
    private readonly IProgressoRepository _progressoRepo;
    private readonly ICursoRepository     _cursoRepo;

    public ProgressoService(
        IProgressoRepository progressoRepo,
        ICursoRepository cursoRepo)
    {
        _progressoRepo = progressoRepo;
        _cursoRepo     = cursoRepo;
    }

    public async Task<ProgressoCursoResponseDto> GetProgressoCursoAsync(
        string usuarioId, string cursoId, CancellationToken ct = default)
    {
        var curso = await _cursoRepo.GetByIdComAulasAsync(cursoId, ct)
            ?? throw new KeyNotFoundException("Curso não encontrado.");

        var progressos = await _progressoRepo.GetByCursoAsync(usuarioId, cursoId, ct);
        return Calcular(cursoId, curso.Aulas.Count(a => a.Ativa), progressos);
    }

    public async Task<ProgressoCursoResponseDto> MarcarAulaConcluidaAsync(
        string usuarioId, MarcarAulaConcluidaRequestDto dto, CancellationToken ct = default)
    {
        var novoProgresso = new Progresso
        {
            UsuarioId     = usuarioId,
            AulaId        = dto.AulaId,
            CursoId       = dto.CursoId,
            Concluida     = true,
            DataConclusao = DateTime.UtcNow,
            CriadoEm     = DateTime.UtcNow
        };

        await _progressoRepo.UpsertAsync(novoProgresso, ct);
        await _progressoRepo.SaveChangesAsync(ct);

        // Retorna o progresso completo e atualizado
        return await GetProgressoCursoAsync(usuarioId, dto.CursoId, ct);
    }

    private static ProgressoCursoResponseDto Calcular(
        string cursoId, int totalAulas, IEnumerable<Progresso> progressos)
    {
        var lista      = progressos.ToList();
        var concluidas = lista.Count(p => p.Concluida);
        var percentual = totalAulas > 0
            ? (int)Math.Round((double)concluidas / totalAulas * 100)
            : 0;

        var dataConclusao = percentual == 100
            ? lista.Where(p => p.DataConclusao.HasValue)
                   .Max(p => p.DataConclusao)
                   ?.ToString("o")   // ISO 8601
            : null;

        var certificadoEmitido = false; // atualizado via CertificadoController

        return new ProgressoCursoResponseDto(
            CursoId:             cursoId,
            AulasProgresso:      lista.Select(p => new ProgressoAulaResponseDto(
                                     p.AulaId, p.Concluida,
                                     p.DataConclusao?.ToString("o"))),
            PercentualConcluido: percentual,
            DataConclusao:       dataConclusao,
            CertificadoEmitido:  certificadoEmitido);
    }
}

// ── CertificadoService ────────────────────────────────────────
public class CertificadoService : ICertificadoService
{
    private readonly ICertificadoRepository _certRepo;
    private readonly ICursoRepository       _cursoRepo;
    private readonly IProgressoRepository   _progressoRepo;
    private readonly IUsuarioRepository     _usuarioRepo;

    public CertificadoService(
        ICertificadoRepository certRepo,
        ICursoRepository cursoRepo,
        IProgressoRepository progressoRepo,
        IUsuarioRepository usuarioRepo)
    {
        _certRepo      = certRepo;
        _cursoRepo     = cursoRepo;
        _progressoRepo = progressoRepo;
        _usuarioRepo   = usuarioRepo;
    }

    public async Task<IEnumerable<CertificadoResponseDto>> GetByUsuarioAsync(
        string usuarioId, CancellationToken ct = default)
    {
        var certs = await _certRepo.GetByUsuarioAsync(usuarioId, ct);
        return certs.Select(Map);
    }

    public async Task<CertificadoResponseDto?> GetByUsuarioCursoAsync(
        string usuarioId, string cursoId, CancellationToken ct = default)
    {
        var cert = await _certRepo.GetByUsuarioCursoAsync(usuarioId, cursoId, ct);
        return cert is null ? null : Map(cert);
    }

    public async Task<CertificadoResponseDto> EmitirAsync(
        string usuarioId, EmitirCertificadoRequestDto dto, CancellationToken ct = default)
    {
        // Idempotente — se já foi emitido, retorna o existente
        var existente = await _certRepo.GetByUsuarioCursoAsync(usuarioId, dto.CursoId, ct);
        if (existente is not null) return Map(existente);

        // Valida: curso existe e tem aulas
        var curso = await _cursoRepo.GetByIdComAulasAsync(dto.CursoId, ct)
            ?? throw new KeyNotFoundException("Curso não encontrado.");

        var totalAulas = curso.Aulas.Count(a => a.Ativa);

        // Valida: 100% concluído
        var progressos  = await _progressoRepo.GetByCursoAsync(usuarioId, dto.CursoId, ct);
        var concluidas  = progressos.Count(p => p.Concluida);

        if (concluidas < totalAulas)
            throw new InvalidOperationException(
                $"Conclua todas as aulas para emitir o certificado " +
                $"({concluidas}/{totalAulas} concluídas).");

        var usuario = await _usuarioRepo.GetByIdAsync(usuarioId, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        // Carga horária em horas (1 casa decimal)
        var cargaHoraria = Math.Round(
            (decimal)curso.Aulas.Where(a => a.Ativa).Sum(a => a.DuracaoMinutos) / 60m, 1);

        var cert = new Certificado
        {
            UsuarioId    = usuarioId,
            CursoId      = dto.CursoId,
            CursoTitulo  = curso.Titulo,     // desnormalizado: captura o título atual
            AlunoNome    = usuario.Nome,     // idem
            DataEmissao  = DateTime.UtcNow,
            CargaHoraria = cargaHoraria
        };

        await _certRepo.AddAsync(cert, ct);
        await _certRepo.SaveChangesAsync(ct);
        return Map(cert);
    }

    private static CertificadoResponseDto Map(Certificado c) =>
        new(c.Id, c.CursoId, c.CursoTitulo, c.AlunoNome,
            c.DataEmissao.ToString("o"), c.CargaHoraria);
}

// ── AdminService ──────────────────────────────────────────────
public class AdminService : IAdminService
{
    private readonly IUsuarioRepository        _usuarioRepo;
    private readonly ICursoRepository          _cursoRepo;
    private readonly IAulaRepository           _aulaRepo;
    private readonly IPedidoVibracaoRepository _pedidoRepo;
    private readonly IMatriculaRepository      _matriculaRepo;

    public AdminService(
        IUsuarioRepository usuarioRepo,
        ICursoRepository cursoRepo,
        IAulaRepository aulaRepo,
        IPedidoVibracaoRepository pedidoRepo,
        IMatriculaRepository matriculaRepo)
    {
        _usuarioRepo   = usuarioRepo;
        _cursoRepo     = cursoRepo;
        _aulaRepo      = aulaRepo;
        _pedidoRepo    = pedidoRepo;
        _matriculaRepo = matriculaRepo;
    }

    // ── Usuários ──────────────────────────────────────────────

    public async Task<PagedResultDto<UsuarioAdminDto>> ListarUsuariosAsync(
        int pagina, int tamanho, CancellationToken ct = default)
    {
        var total   = await _usuarioRepo.CountAsync(ct);
        var usuarios = await _usuarioRepo.GetAllPagedAsync(pagina, tamanho, ct);

        return new PagedResultDto<UsuarioAdminDto>(
            Items:         usuarios.Select(MapUsuario),
            TotalItens:    total,
            Pagina:        pagina,
            TamanhoPagina: tamanho,
            TotalPaginas:  (int)Math.Ceiling((double)total / tamanho),
            TemProxima:    pagina * tamanho < total,
            TemAnterior:   pagina > 1);
    }

    public async Task<UsuarioAdminDto> AtualizarUsuarioAdminAsync(
        string usuarioId, AtualizarUsuarioAdminRequestDto dto, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetTrackedByIdAsync(usuarioId, ct)
            ?? throw new KeyNotFoundException($"Usuário '{usuarioId}' não encontrado.");

        var emailNorm = dto.Email.ToLower().Trim();
        if (!string.Equals(usuario.Email, emailNorm, StringComparison.Ordinal))
        {
            var emailExistente = await _usuarioRepo.GetByEmailAsync(emailNorm, ct);
            if (emailExistente is not null)
                throw new InvalidOperationException("E-mail já está em uso por outro usuário.");
        }

        usuario.Nome      = dto.Nome.Trim();
        usuario.Email     = emailNorm;
        usuario.AvatarUrl = dto.AvatarUrl;
        usuario.Role      = dto.Role;

        await _usuarioRepo.SaveChangesAsync(ct);
        return MapUsuario(usuario);
    }

    public async Task AlterarStatusUsuarioAsync(
        string usuarioId, bool ativo, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetTrackedByIdAsync(usuarioId, ct)
            ?? throw new KeyNotFoundException($"Usuário '{usuarioId}' não encontrado.");

        usuario.Ativo = ativo;
        await _usuarioRepo.SaveChangesAsync(ct);
    }

    // ── Matrículas ────────────────────────────────────────────

    public async Task<PagedResultDto<MatriculaAdminDto>> ListarMatriculasAsync(
        int pagina, int tamanho, CancellationToken ct = default)
    {
        var total = await _matriculaRepo.CountAllAsync(ct);
        var itens = await _matriculaRepo.GetAllAsync(pagina, tamanho, ct);

        return new PagedResultDto<MatriculaAdminDto>(
            Items: itens.Select(m => new MatriculaAdminDto(
                m.Id,
                m.UsuarioId,
                m.Usuario?.Nome ?? m.NomeCompleto,
                m.Usuario?.Email ?? m.Email,
                m.CursoId,
                m.Curso.Titulo,
                m.DataMatricula,
                m.Ativa)),
            TotalItens:    total,
            Pagina:        pagina,
            TamanhoPagina: tamanho,
            TotalPaginas:  (int)Math.Ceiling((double)total / tamanho),
            TemProxima:    pagina * tamanho < total,
            TemAnterior:   pagina > 1);
    }

    // ── Pedidos de Vibração ───────────────────────────────────

    public async Task<PagedResultDto<PedidoVibracaoAdminDto>> ListarPedidosVibracaoAsync(
        int pagina, int tamanho, CancellationToken ct = default)
    {
        var total = await _pedidoRepo.CountAsync(ct);
        var itens  = await _pedidoRepo.GetAllAsync(pagina, tamanho, ct);

        return new PagedResultDto<PedidoVibracaoAdminDto>(
            Items:         itens.Select(p => new PedidoVibracaoAdminDto(
                               p.Id, p.Nome, p.Email ?? string.Empty,
                               p.Pedido, p.Cidade, p.Estado, p.CriadoEm, p.Lido)),
            TotalItens:    total,
            Pagina:        pagina,
            TamanhoPagina: tamanho,
            TotalPaginas:  (int)Math.Ceiling((double)total / tamanho),
            TemProxima:    pagina * tamanho < total,
            TemAnterior:   pagina > 1);
    }

    public async Task MarcarPedidoLidoAsync(string pedidoId, CancellationToken ct = default)
    {
        var pedido = await _pedidoRepo.GetTrackedByIdAsync(pedidoId, ct)
            ?? throw new KeyNotFoundException($"Pedido '{pedidoId}' não encontrado.");

        pedido.Lido = true;
        await _pedidoRepo.SaveChangesAsync(ct);
    }

    // ── Cursos ────────────────────────────────────────────────

    public async Task<IEnumerable<CursoAdminResponseDto>> ListarCursosAsync(
        CancellationToken ct = default)
    {
        var cursos = await _cursoRepo.GetAllAdminAsync(ct);
        return cursos.Select(MapCurso);
    }

    public async Task<CursoAdminResponseDto> CriarCursoAsync(
        CursoRequestDto dto, CancellationToken ct = default)
    {
        var curso = new Curso
        {
            Titulo                = dto.Titulo,
            Descricao             = dto.Descricao,
            CategoriaId           = dto.CategoriaId,
            ImagemUrl             = dto.ImagemUrl,
            Instrutor             = dto.Instrutor,
            CertificadoDisponivel = dto.CertificadoDisponivel,
            CriadoEm             = DateTime.UtcNow,
            AtualizadoEm         = DateTime.UtcNow
        };

        await _cursoRepo.AddAsync(curso, ct);
        await _cursoRepo.SaveChangesAsync(ct);
        return MapCurso(curso);
    }

    public async Task<CursoAdminResponseDto> AtualizarCursoAsync(
        string cursoId, CursoRequestDto dto, CancellationToken ct = default)
    {
        var curso = await _cursoRepo.GetTrackedByIdAsync(cursoId, ct)
            ?? throw new KeyNotFoundException($"Curso '{cursoId}' não encontrado.");

        curso.Titulo                = dto.Titulo;
        curso.Descricao             = dto.Descricao;
        curso.CategoriaId           = dto.CategoriaId;
        curso.ImagemUrl             = dto.ImagemUrl;
        curso.Instrutor             = dto.Instrutor;
        curso.CertificadoDisponivel = dto.CertificadoDisponivel;
        curso.AtualizadoEm         = DateTime.UtcNow;

        await _cursoRepo.SaveChangesAsync(ct);
        return MapCurso(curso);
    }

    public async Task RemoverCursoAsync(string cursoId, CancellationToken ct = default)
    {
        var curso = await _cursoRepo.GetTrackedByIdAsync(cursoId, ct)
            ?? throw new KeyNotFoundException($"Curso '{cursoId}' não encontrado.");

        curso.Ativo      = false;
        curso.AtualizadoEm = DateTime.UtcNow;
        await _cursoRepo.SaveChangesAsync(ct);
    }

    // ── Aulas ─────────────────────────────────────────────────

    public async Task<AulaAdminResponseDto> AdicionarAulaAsync(
        string cursoId, AulaRequestDto dto, CancellationToken ct = default)
    {
        var curso = await _cursoRepo.GetTrackedByIdAsync(cursoId, ct)
            ?? throw new KeyNotFoundException($"Curso '{cursoId}' não encontrado.");

        var ordemOcupada = curso.Aulas.Any(a => a.Ordem == dto.Ordem);
        if (ordemOcupada)
            throw new InvalidOperationException(
                $"Já existe uma aula na posição {dto.Ordem} neste curso.");

        var aula = new Aula
        {
            CursoId        = cursoId,
            Titulo         = dto.Titulo,
            Descricao      = dto.Descricao,
            YoutubeVideoId = dto.YoutubeVideoId,
            DuracaoMinutos = dto.DuracaoMinutos,
            Ordem          = dto.Ordem
        };

        await _aulaRepo.AddAsync(aula, ct);
        await _aulaRepo.SaveChangesAsync(ct);
        return MapAula(aula);
    }

    public async Task<AulaAdminResponseDto> AtualizarAulaAsync(
        string cursoId, string aulaId, AulaRequestDto dto, CancellationToken ct = default)
    {
        var aula = await _aulaRepo.GetTrackedByIdAsync(aulaId, ct)
            ?? throw new KeyNotFoundException($"Aula '{aulaId}' não encontrada.");

        if (aula.CursoId != cursoId)
            throw new ArgumentException("Aula não pertence ao curso informado.");

        aula.Titulo         = dto.Titulo;
        aula.Descricao      = dto.Descricao;
        aula.YoutubeVideoId = dto.YoutubeVideoId;
        aula.DuracaoMinutos = dto.DuracaoMinutos;
        aula.Ordem          = dto.Ordem;

        await _aulaRepo.SaveChangesAsync(ct);
        return MapAula(aula);
    }

    public async Task RemoverAulaAsync(
        string cursoId, string aulaId, CancellationToken ct = default)
    {
        var aula = await _aulaRepo.GetTrackedByIdAsync(aulaId, ct)
            ?? throw new KeyNotFoundException($"Aula '{aulaId}' não encontrada.");

        if (aula.CursoId != cursoId)
            throw new ArgumentException("Aula não pertence ao curso informado.");

        aula.Ativa = false;
        await _aulaRepo.SaveChangesAsync(ct);
    }

    // ── Mapeamentos privados ──────────────────────────────────

    private static UsuarioAdminDto MapUsuario(Usuario u) =>
        new(u.Id, u.Nome, u.Email, u.AvatarUrl, u.Role, u.DataCadastro, u.Ativo);

    private static CursoAdminResponseDto MapCurso(Curso c) =>
        new(c.Id, c.Titulo, c.Descricao,
            c.CategoriaId, c.Categoria?.Nome,
            c.ImagemUrl, c.Instrutor,
            c.CertificadoDisponivel, c.Ativo,
            c.Aulas.Count(a => a.Ativa),
            c.CriadoEm, c.AtualizadoEm,
            c.Aulas.OrderBy(a => a.Ordem).Select(MapAula));

    private static AulaAdminResponseDto MapAula(Aula a) =>
        new(a.Id, a.CursoId, a.Titulo, a.Descricao,
            a.YoutubeVideoId, a.DuracaoMinutos, a.Ordem, a.Ativa);
}

// ── PedidoVibracaoService ─────────────────────────────────────
public class PedidoVibracaoService : IPedidoVibracaoService
{
    private readonly IPedidoVibracaoRepository _pedidoRepo;

    public PedidoVibracaoService(IPedidoVibracaoRepository pedidoRepo) =>
        _pedidoRepo = pedidoRepo;

    public async Task<PedidoVibracaoResponseDto> EnviarAsync(
        string? usuarioId, PedidoVibracaoRequestDto dto, CancellationToken ct = default)
    {
        var pedido = new PedidoVibracao
        {
            UsuarioId  = usuarioId,
            Nome       = dto.Nome.Trim(),
            Email      = dto.Email?.Trim().ToLower(),
            Pedido     = dto.Pedido.Trim(),
            Cep        = dto.Endereco?.Cep,
            Logradouro = dto.Endereco?.Logradouro,
            Cidade     = dto.Endereco?.Cidade,
            Estado     = dto.Endereco?.Estado,
            CriadoEm  = DateTime.UtcNow,
            Lido       = false
        };

        await _pedidoRepo.AddAsync(pedido, ct);
        await _pedidoRepo.SaveChangesAsync(ct);

        return new PedidoVibracaoResponseDto(
            pedido.Id,
            "Seu pedido foi recebido com muito carinho e será incluído em nossas orações. Vibraremos por você");
    }

    public async Task<PagedResultDto<PedidoVibracaoAdminDto>> ListarAsync(
        int pagina, int tamanho, CancellationToken ct = default)
    {
        var total = await _pedidoRepo.CountAsync(ct);
        var itens = await _pedidoRepo.GetAllAsync(pagina, tamanho, ct);

        return new PagedResultDto<PedidoVibracaoAdminDto>(
            Items: itens.Select(p => new PedidoVibracaoAdminDto(
                p.Id, p.Nome, p.Email, p.Pedido,
                p.Cidade, p.Estado, p.CriadoEm, p.Lido)),
            TotalItens:    total,
            Pagina:        pagina,
            TamanhoPagina: tamanho,
            TotalPaginas:  (int)Math.Ceiling((double)total / tamanho),
            TemProxima:    pagina * tamanho < total,
            TemAnterior:   pagina > 1
        );
    }
}
