Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFile(pathFormat: "Logs/Log-{Date}.txt", isJson: false, minimumLevel: LogLevel.Warning);
builder.Services.AddControllers();
var app = builder.Build();

string baseDir = app.Environment.ContentRootPath;
AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(baseDir, "App_Data"));

app.MapControllers();
app.Run();
