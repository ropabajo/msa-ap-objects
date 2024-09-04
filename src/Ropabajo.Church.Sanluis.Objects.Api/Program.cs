using Elastic.Apm.NetCoreAll;
using Ropabajo.Churc.Sanluis.Framework.Core;
using Ropabajo.Churc.Sanluis.Framework.MinIo;
using Ropabajo.Churc.Sanluis.Framework.Swagger;
using Ropabajo.Church.Sanluis.Objects.Application;
using Ropabajo.Church.Sanluis.Objects.Infraestructure;
using Steeltoe.Extensions.Configuration.ConfigServer;

var builder = WebApplication.CreateBuilder(args);
builder.AddConfigServer();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddBase();
builder.Services.AddBaseSwagger();
builder.Services.AddBaseMinio();
builder.WebHost.UseBase();

var app = builder.Build();
app.UseBase();
app.UseBaseSwagger();
//app.UseAllElasticApm(app.Configuration);
app.Run();