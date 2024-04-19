using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using OfficeOpenXml;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Churc.Sanluis.Framework.MinIo;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Domain;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Commands.CreateDepartment
{
    internal class CreateDepartmentCommandHandler : CommandHandler, IRequestHandler<CreateDepartmentCommand>
    {
        private readonly ILogger<CreateDepartmentCommandHandler> _logger;
        private readonly IMediatorBus _bus;
        private readonly IValidator<CreateDepartmentCommand> _validator;
        private readonly MinIoOptions _minIoOptions;
        private readonly IDepartmentRepository _departmentRepository;

        public CreateDepartmentCommandHandler(
                ILogger<CreateDepartmentCommandHandler> logger,
                IMediatorBus bus,
                IValidator<CreateDepartmentCommand> validator,
                IOptionsSnapshot<MinIoOptions> minIoOptions,
                IDepartmentRepository departmentRepository
            ) : base(bus)
        {
            _logger = logger;
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _minIoOptions = minIoOptions.Value ?? throw new ArgumentNullException(nameof(minIoOptions));
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        }

        public async Task<Unit> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                await RaiseErrrosAsync(validationResult);
                return Unit.Value;
            }

            var memory = await GetFileByNameAsync(request.ObjectName);
            if (memory is null) return Unit.Value;

            var items = GetRowsExcelAsync(memory);
            if (!items.Any()) return Unit.Value;

            Create(items.ToList());

            await _departmentRepository.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        private async Task<MemoryStream> GetFileByNameAsync(string fullPath)
        {
            MemoryStream memory = new();

            var statObjectArgs = new StatObjectArgs()
                                     .WithBucket(_minIoOptions.BucketName)
                                     .WithObject(fullPath);

            var _minioClient = new MinioClient()
                                 .WithEndpoint(_minIoOptions.Endpoint)
                                 .WithCredentials(_minIoOptions.AccessKey, _minIoOptions.SecretKey)
                                 .WithSSL(_minIoOptions.UseSsl)
                                 .Build();

            await _minioClient.StatObjectAsync(statObjectArgs);

            var getObjectArgs = new GetObjectArgs()
                                    .WithBucket(_minIoOptions.BucketName)
                                    .WithObject(fullPath)
                                    .WithCallbackStream((stream) =>
                                    {
                                        stream.CopyTo(memory);
                                    });

            await _minioClient.GetObjectAsync(getObjectArgs);

            return memory;
        }

        private static UbigeoExcel CreateRowExcel(ExcelWorksheet excelWorksheet, int i, int line)
        {
            return new UbigeoExcel
            {
                //Line = line,
                CODIGO_DEPARTAMENTO = GetCellValue(excelWorksheet, i, 1),
                CODIGO_DEPARTAMENTO_INEI = GetCellValue(excelWorksheet, i, 2),
                CODIGO_DEPARTAMENTO_RENIEC = GetCellValue(excelWorksheet, i, 3),
                DESCRIPCION_DEPARTAMENTO = GetCellValue(excelWorksheet, i, 4),
                CODIGO_PROVINCIA = GetCellValue(excelWorksheet, i, 5),
                CODIGO_PROVINCIA_INEI = GetCellValue(excelWorksheet, i, 6),
                CODIGO_PROVINCIA_RENIEC = GetCellValue(excelWorksheet, i, 7),
                DESCRIPCION_PROVINCIA = GetCellValue(excelWorksheet, i, 8),
                CODIGO_DISTRITO = GetCellValue(excelWorksheet, i, 9),
                CODIGO_DISTRITO_INEI = GetCellValue(excelWorksheet, i, 10),
                CODIGO_DISTRITO_RENIEC = GetCellValue(excelWorksheet, i, 11),
                DESCRIPCION_DISTRITO = GetCellValue(excelWorksheet, i, 12)
            };
        }

        private static string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            return (worksheet.Cells[row, col].Text ?? "").Trim();
        }

        private static IEnumerable<UbigeoExcel> GetRowsExcelAsync(MemoryStream memory)
        {
            List<UbigeoExcel> rows = [];
            var epArchivo = new ExcelPackage(memory);
            if (epArchivo != null)
            {
                ExcelWorksheet excelWorksheet = epArchivo.Workbook.Worksheets[0];
                int line = 0;
                for (int i = 2; i <= excelWorksheet.Dimension.End.Row; i++)
                {
                    line++;
                    rows.Add(CreateRowExcel(excelWorksheet, i, line));
                }
            }
            return rows;
        }

        private void Create(List<UbigeoExcel> items)
        {
            int districtCode = 0;
            int provenceCode = 0;
            int departamentCode = 0;

            //25
            var distinctDepartaments = items
                                    .GroupBy(x => new { x.CODIGO_DEPARTAMENTO, x.CODIGO_DEPARTAMENTO_INEI, x.CODIGO_DEPARTAMENTO_RENIEC, x.DESCRIPCION_DEPARTAMENTO })
                                    .Select(group => group.First())
                                    .ToList();


            foreach (var departament in distinctDepartaments)
            {
                departamentCode++;
                var departamentToCreate = new Departament(
                                            code: departamentCode,
                                            ineiCode: departament.CODIGO_DEPARTAMENTO_INEI.ToUpper(),
                                            reniecCode: departament.CODIGO_DEPARTAMENTO_RENIEC.ToUpper(),
                                            description: departament.DESCRIPCION_DEPARTAMENTO.ToUpper()
                                            );

                var provincesByCodeDepartament = items
                                                .Where(x => x.CODIGO_DEPARTAMENTO == departament.CODIGO_DEPARTAMENTO)
                                                .GroupBy(x => new { x.CODIGO_PROVINCIA, x.CODIGO_PROVINCIA_INEI, x.CODIGO_PROVINCIA_RENIEC, x.DESCRIPCION_PROVINCIA })
                                                .Select(group => group.First())
                                                .ToList();


                foreach (var province in provincesByCodeDepartament)
                {
                    provenceCode++;
                    var provinceToCreate = new Province(
                                               code: provenceCode,
                                               departamentCode: departamentCode,
                                               ineiCode: province.CODIGO_PROVINCIA_INEI.ToUpper(),
                                               reniecCode: province.CODIGO_PROVINCIA_RENIEC.ToUpper(),
                                               description: province.DESCRIPCION_PROVINCIA.ToUpper()
                                                );

                    var districtsByCodeDepartament = items
                                                .Where(x => x.CODIGO_DEPARTAMENTO == departament.CODIGO_DEPARTAMENTO && x.CODIGO_PROVINCIA == province.CODIGO_PROVINCIA)
                                                .GroupBy(x => new { x.CODIGO_DISTRITO, x.CODIGO_DISTRITO_INEI, x.CODIGO_DISTRITO_RENIEC, x.DESCRIPCION_DISTRITO })
                                                .Select(group => group.First())
                                                .ToList();

                    foreach (var district in districtsByCodeDepartament)
                    {
                        districtCode++;

                        var districtToCreate = new District(
                                                code: districtCode,
                                                departamentCode: departamentCode,
                                                provinceCode: provenceCode,
                                                ineiCode: district.CODIGO_DISTRITO_INEI.ToUpper(),
                                                reniecCode: district.CODIGO_DISTRITO_RENIEC.ToUpper(),
                                                description: district.DESCRIPCION_DISTRITO.ToUpper()
                                                );

                        provinceToCreate.AddDistrict(districtToCreate);
                        departamentToCreate.AddDistrict(districtToCreate);
                    }


                    departamentToCreate.AddProvince(provinceToCreate);
                }

                _departmentRepository.Add(departamentToCreate);
            }
        }

    }
}
