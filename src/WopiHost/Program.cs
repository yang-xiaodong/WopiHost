using WopiHost.Abstractions;
using WopiHost.Core;
using WopiHost.Core.Controllers;
using WopiHost.Core.Models;
using WopiHost.FileS3Provider;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IWopiSecurityHandler, WopiSecurityHandler>();
builder.Services.AddSingleton<IWopiStorageProvider, WopiFileS3Provider>();
builder.Services.AddSingleton<IDictionary<string, LockInfo>>(d => new Dictionary<string, LockInfo>());

 
// Configuration
builder.Services.AddOptions();

builder.Services.AddWopi();

builder.Services.AddControllers().AddApplicationPart(typeof(FilesController).Assembly);

var app = builder.Build();

app.UseAuthentication();
app.MapControllers();
app.Run();
