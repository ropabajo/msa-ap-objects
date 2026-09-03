using Ropabajo.Church.Sanluis.Objects.Application;
using Ropabajo.Church.Sanluis.Objects.Infraestructure;
using Ropabajo.Churc.Sanluis.Framework.Authz;
using Ropabajo.Churc.Sanluis.Framework.Core;
using Ropabajo.Churc.Sanluis.Framework.MinIo;
using Ropabajo.Churc.Sanluis.Framework.Swagger;
using Steeltoe.Configuration.ConfigServer;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddConfigServer();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddAuthz(builder.Configuration);
builder.Services.AddBase();
builder.Services.AddBaseSwagger();
builder.Services.AddBaseMinio();
builder.WebHost.UseBase();

var app = builder.Build();
app.UseBase();
app.UseKeycloak();
app.UseBaseSwagger();
//app.UseAllElasticApm(app.Configuration);
app.Run();