using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Domain.Common;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
using System.Security.Claims;
using Object = Ropabajo.Church.Sanluis.Objects.Domain.Entities.Object;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;

public class DatabaseContext : DbContext
{
    private const string SystemUser = "system";
    private const string UnknownIp = "0.0.0.0";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DatabaseContext(DbContextOptions<DatabaseContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public virtual DbSet<BulkLoad> BulkLoads { get; set; }

    public virtual DbSet<BulkLoadState> BulkLoadStates { get; set; }

    public virtual DbSet<BulkLoadResult> BulkLoadResults { get; set; }

    public virtual DbSet<Format> Formats { get; set; }

    public virtual DbSet<Object> Objects { get; set; }

    public virtual DbSet<ObjectState> ObjectStates { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var auditUser = GetAuditUser();
        var auditIp = GetAuditIp();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = DateTime.Now;
                    entry.Entity.CreatedBy = auditUser;
                    entry.Entity.CreatedIp = auditIp;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedDate = DateTime.Now;
                    entry.Entity.LastModifiedBy = auditUser;
                    entry.Entity.LastModifiedIp = auditIp;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    private string GetAuditUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var value = user?.FindFirstValue("sub")
            ?? user?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user?.FindFirstValue("preferred_username")
            ?? user?.FindFirstValue("email")
            ?? user?.Identity?.Name
            ?? SystemUser;

        return Limit(value, 50);
    }

    private string GetAuditIp()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        var forwardedFor = request?.Headers["X-Forwarded-For"].FirstOrDefault();
        var realIp = request?.Headers["X-Real-IP"].FirstOrDefault();
        var remoteIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var ip = forwardedFor?.Split(',').FirstOrDefault()?.Trim()
            ?? realIp
            ?? remoteIp
            ?? UnknownIp;

        return Limit(ip, 40);
    }

    private static string Limit(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}