using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Domain.Common;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
using Object = Ropabajo.Church.Sanluis.Objects.Domain.Entities.Object;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public virtual DbSet<BulkLoad> BulkLoads { get; set; }

        public virtual DbSet<BulkLoadState> BulkLoadStates { get; set; }

        public virtual DbSet<BulkLoadResult> BulkLoadResults { get; set; }

        public virtual DbSet<Format> Formats { get; set; }

        public virtual DbSet<Object> Objects { get; set; }

        public virtual DbSet<ObjectState> ObjectStates { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.Now;
                        entry.Entity.CreatedBy = "XXX"; // TODO: Get UserName
                        entry.Entity.CreatedIp = "YYY"; // TODO: Get IP remote
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedDate = DateTime.Now;
                        entry.Entity.LastModifiedBy = "AAA"; // TODO: Get UserName
                        entry.Entity.LastModifiedIp = "BBB"; // TODO: Get IP remote
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}