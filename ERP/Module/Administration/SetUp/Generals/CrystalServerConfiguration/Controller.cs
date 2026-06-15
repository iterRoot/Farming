using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Sockets;

namespace FarmingApi.Modules.Administration.CrystalServer;

[ApiController]
[Route("[controller]")]
public class CrystalServerConfigController : ControllerBase
{
    private readonly ICrystalServerConfigRepository _repo;
    private readonly IMapper                        _mapper;

    public CrystalServerConfigController(
        ICrystalServerConfigRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    // ── GET /CrystalServerConfig ───────────────────────────────
    // Returns all configs (usually just one record)
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll().OrderBy(x => x.Id).ToList();
        return Ok(_mapper.Map<List<CrystalServerConfigResponse>>(list));
    }

    // ── GET /CrystalServerConfig/Current ──────────────────────
    // Returns the first (active) config — the one the system uses
    [AllowAnonymous][HttpGet("Current")]
    public IActionResult GetCurrent()
    {
        var e = _repo.GetAll().FirstOrDefault(x => x.IsEnabled)
             ?? _repo.GetAll().FirstOrDefault();
        if (e == null) return NotFound(new { message = "No Crystal Server config found. Please create one." });
        return Ok(_mapper.Map<CrystalServerConfigResponse>(e));
    }

    // ── GET /CrystalServerConfig/{id} ─────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<CrystalServerConfigResponse>(e));
    }

    // ── POST /CrystalServerConfig ─────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] CrystalServerConfigRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity       = _mapper.Map<CrystalServerConfig>(dto);
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<CrystalServerConfigResponse>(entity));
    }

    // ── PUT /CrystalServerConfig/{id} ─────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] CrystalServerConfigRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        _mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<CrystalServerConfigResponse>(entity));
    }

    // ── PUT /CrystalServerConfig/Upsert ───────────────────────
    // Create if none exists; update existing — useful for single-config pattern
    [AllowAnonymous][HttpPut("Upsert")]
    public IActionResult Upsert([FromBody] CrystalServerConfigRequest dto)
    {
        var existing = _repo.GetAll().FirstOrDefault();
        if (existing == null)
        {
            var newEntity       = _mapper.Map<CrystalServerConfig>(dto);
            newEntity.CreatedAt = DateTime.UtcNow;
            newEntity.InActive  = false;
            _repo.Add(newEntity);
            _repo.Commit();
            return Ok(_mapper.Map<CrystalServerConfigResponse>(newEntity));
        }
        _mapper.Map(dto, existing);
        existing.UpdatedAt = DateTime.UtcNow;
        _repo.Update(existing);
        _repo.Commit();
        return Ok(_mapper.Map<CrystalServerConfigResponse>(existing));
    }

    // ── DELETE /CrystalServerConfig/{id} ──────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ── POST /CrystalServerConfig/{id}/Test ───────────────────
    // Ping the Crystal Server host:port to verify connectivity
    [AllowAnonymous][HttpPost("{id:int}/Test")]
    public IActionResult TestConnection(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (string.IsNullOrWhiteSpace(entity.Host))
            return Ok(new ConnectionTestResult
            {
                Success = false,
                Message = "No host configured",
            });

        var sw = Stopwatch.StartNew();
        try
        {
            using var tcp = new TcpClient();
            var result = tcp.BeginConnect(entity.Host, entity.Port, null, null);
            bool connected = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));
            sw.Stop();

            if (connected && tcp.Connected)
            {
                tcp.EndConnect(result);
                return Ok(new ConnectionTestResult
                {
                    Success = true,
                    Message = $"Connected to {entity.Host}:{entity.Port}",
                    PingMs  = (int)sw.ElapsedMilliseconds,
                });
            }
            return Ok(new ConnectionTestResult
            {
                Success = false,
                Message = $"Could not reach {entity.Host}:{entity.Port} (timeout)",
                PingMs  = (int)sw.ElapsedMilliseconds,
            });
        }
        catch (Exception ex)
        {
            return Ok(new ConnectionTestResult
            {
                Success = false,
                Message = "Connection failed",
                Detail  = ex.Message,
                PingMs  = (int)sw.ElapsedMilliseconds,
            });
        }
    }

    // ── POST /CrystalServerConfig/{id}/TestSmtp ───────────────
    // Minimal SMTP reachability test (just TCP to SMTP host:port)
    [AllowAnonymous][HttpPost("{id:int}/TestSmtp")]
    public IActionResult TestSmtp(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (string.IsNullOrWhiteSpace(entity.SmtpHost))
            return Ok(new ConnectionTestResult { Success = false, Message = "No SMTP host configured" });

        var port = entity.SmtpPort ?? 587;
        var sw   = Stopwatch.StartNew();
        try
        {
            using var tcp = new TcpClient();
            bool ok = tcp.BeginConnect(entity.SmtpHost, port, null, null)
                         .AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));
            sw.Stop();
            return Ok(new ConnectionTestResult
            {
                Success = ok && tcp.Connected,
                Message = ok && tcp.Connected
                    ? $"SMTP reachable at {entity.SmtpHost}:{port}"
                    : $"SMTP unreachable at {entity.SmtpHost}:{port}",
                PingMs  = (int)sw.ElapsedMilliseconds,
            });
        }
        catch (Exception ex)
        {
            return Ok(new ConnectionTestResult
            {
                Success = false, Message = "SMTP test failed",
                Detail  = ex.Message, PingMs = (int)sw.ElapsedMilliseconds,
            });
        }
    }
}