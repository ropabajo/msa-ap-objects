using Ropabajo.Churc.Sanluis.Framework.Mediator;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Queries.GetObjects
{
    public class GetObjectsQuery : Query<IEnumerable<ObjectsVm>>
    {
        /// <summary>
        /// Código objeto
        /// </summary>
        /// <example>4352279a-d37b-4f80-8bd8-42e018d7a98a</example>
        public Guid? Code { get; set; }

        /// <summary>
        /// Nombre objeto
        /// </summary>
        /// <example>LIMA</example>
        public string? ObjectName { get; set; }
    }
}
