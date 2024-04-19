using Ropabajo.Churc.Sanluis.Framework.Mediator;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject
{
    public class UploadObjectCommand : Command
    {
        public UploadObjectCommand(Guid objectCode)
        {
            ObjectCode = objectCode;
        }

        public Guid? ObjectCode { get; set; }
    }
}
