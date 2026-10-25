using Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
builder.Services.AddScoped<Ropabajo.Church.Sanluis.Objects.Api.Controllers.TemporaryExportRequestLimitFilter>();
builder.Services.AddScoped<Ropabajo.Church.Sanluis.Objects.Api.Controllers.TemporaryExportExceptionFilter>();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.EnvironmentName);
builder.Services.AddFileJobs(builder.Configuration);
builder.Services.AddScoped<Ropabajo.Church.Sanluis.Objects.Api.Controllers.FileJobExceptionFilter>();
var fileJobRuntime = builder.Configuration.GetSection("FileJobs").Get<FileJobsRuntimeSettings>() ?? new FileJobsRuntimeSettings();
builder.Services.AddAuthentication().AddJwtBearer("FileJobsWorker", options =>
{
    options.Authority = fileJobRuntime.WorkerAuthority;
    options.Audience = fileJobRuntime.WorkerAudience;
    options.MapInboundClaims = false;
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30), ValidAudience = fileJobRuntime.WorkerAudience
    };
});
builder.Services.AddAuthorization(options => options.AddPolicy("FileJobsWorker", policy => policy
    .AddAuthenticationSchemes("FileJobsWorker").RequireAuthenticatedUser()
    .RequireAssertion(context => fileJobRuntime.WorkerClients.ContainsKey(context.User.FindFirst("azp")?.Value ?? string.Empty))));
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
