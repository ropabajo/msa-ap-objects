using Ropabajo.Church.Sanluis.Objects.Domain.Common;

namespace Ropabajo.Church.Sanluis.Objects.Domain
{
    public partial class Province : BaseEntity
    {
        public int Id { get; set; }

        public int Code { get; set; }

        public int DepartamentId { get; set; }

        public int DepartamentCode { get; set; }

        public string IneiCode { get; set; } = null!;

        public string? ReniecCode { get; set; }

        public string Description { get; set; } = null!;

        public string? Abbreviation { get; set; }

        public virtual ICollection<District> Districts { get; } = new List<District>();

        public virtual Departament IdDepartamentoNavigation { get; set; } = null!;

        protected Province() { }

        public Province(
            int code,
            int departamentCode,
            string ineiCode,
            string? reniecCode,
            string description)
        {
            Code = code;
            DepartamentCode = departamentCode;
            IneiCode = ineiCode;
            ReniecCode = reniecCode;
            Description = description;
        }

        public void AddDistrict(District item)
        {
            if (item is not null)
            {
                Districts.Add(item);
            }
        }
    }
}