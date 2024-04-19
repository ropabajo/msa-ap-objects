using Ropabajo.Church.Sanluis.Objects.Domain.Common;

namespace Ropabajo.Church.Sanluis.Objects.Domain
{
    public partial class Departament : BaseEntity
    {
        public int Id { get; set; }

        public int Code { get; set; }

        public string IneiCode { get; set; } = null!;

        public string? ReniecCode { get; set; }

        public string Description { get; set; } = null!;

        public string? Abbreviation { get; set; }

        //public ulong Eliminado { get; set; }

        public virtual ICollection<District> Districts { get; } = new List<District>();

        public virtual ICollection<Province> Provinces { get; } = new List<Province>();

        protected Departament()
        { }

        public Departament(
            int code,
            string ineiCode,
            string? reniecCode,
            string description
            )
        {
            Code = code;
            IneiCode = ineiCode;
            ReniecCode = reniecCode;
            Description = description;
        }

        public void AddProvince(Province item)
        {
            if (item is not null)
            {
                Provinces.Add(item);
            }
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