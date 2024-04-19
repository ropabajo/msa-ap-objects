using Ropabajo.Church.Sanluis.Objects.Domain.Common;

namespace Ropabajo.Church.Sanluis.Objects.Domain
{
    public partial class District : BaseEntity
    {
        public int Id { get; set; }

        public int Code { get; set; }

        public int DepartamentId { get; set; }

        public int DepartamentCode { get; set; }

        public int ProvinceId { get; set; }

        public int ProvinceCode { get; set; }

        public string IneiCode { get; set; } = null!;

        public string? ReniecCode { get; set; }

        public string Description { get; set; } = null!;

        public string? Abbreviation { get; set; }

        public virtual Departament IdDepartamentoNavigation { get; set; } = null!;

        public virtual Province IdProvinciaNavigation { get; set; } = null!;

        protected District()
        { }

        public District(
            int code,
            int departamentCode,
            int provinceCode,
            string ineiCode,
            string? reniecCode,
            string description)
        {
            Code = code;
            DepartamentCode = departamentCode;
            ProvinceCode = provinceCode;
            IneiCode = ineiCode;
            ReniecCode = reniecCode;
            Description = description;
        }
    }
}