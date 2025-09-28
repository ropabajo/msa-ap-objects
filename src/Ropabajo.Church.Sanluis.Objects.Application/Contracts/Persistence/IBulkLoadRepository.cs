﻿using Ropabajo.Church.Sanluis.Objects.Domain.Entities;

namespace Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence
{
    public interface IBulkLoadRepository : IBaseRepository<BulkLoad>
    {
        Task<IEnumerable<BulkLoad>> GetPagedAsync(
            Guid? formatCode,
            int pageNumber,
            int pageSize
            );

        Task<int> GetTotalAsync(Guid? formatCode);
    }
}
