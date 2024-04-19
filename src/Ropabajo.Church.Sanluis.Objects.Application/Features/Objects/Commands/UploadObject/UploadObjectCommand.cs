using Ropabajo.Churc.Sanluis.Framework.Mediator;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject
{
    public class UploadObjectCommand : Command
    {
        public UploadObjectCommand(string objectCode)
        {
            ObjectCode = objectCode;
        }

        public string? ObjectCode { get; set; }
    }
}
