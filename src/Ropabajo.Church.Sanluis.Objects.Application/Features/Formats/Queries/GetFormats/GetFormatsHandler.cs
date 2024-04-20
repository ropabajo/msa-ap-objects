using AutoMapper;
using MediatR;
using Microsoft.Extensions.Options;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Churc.Sanluis.Framework.MinIo;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Formats.Queries.GetFormats
{
    public class GetFormatsHandler : QueryHandler, IRequestHandler<GetFormatsQuery, IEnumerable<FormatsVm>>
    {
        private readonly IMediatorBus _bus;
        private readonly IMapper _mapper;
        private readonly MinIoOptions _minIoOptions;
        private readonly IFormatRepository _formatRepository;

        public GetFormatsHandler(
            IMediatorBus bus,
            IMapper mapper,
            IOptionsSnapshot<MinIoOptions> minIoOptions,
            IFormatRepository formatRepository
            ) : base(bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _minIoOptions = minIoOptions.Value ?? throw new ArgumentNullException(nameof(minIoOptions));
            _formatRepository = formatRepository ?? throw new ArgumentNullException(nameof(formatRepository));
        }

        public async Task<IEnumerable<FormatsVm>> Handle(GetFormatsQuery query, CancellationToken cancellationToken)
        {
            var formats = await _formatRepository.GetAsync(x => !x.Delete);
            if (!formats.Any())
            {
                await _bus.RaiseAsync(new Notification(NotificationType.NotContent));
                return null;
            }

            var formatsVm = _mapper.Map<List<FormatsVm>>(formats);
            foreach (var formatVm in formatsVm)
            {
                formatVm.Template = $"{_minIoOptions.Endpoint}/{_minIoOptions.BucketName}/{formatVm.Template}";
            }

            return formatsVm;
        }
    }
}
