using spikewall.Debug;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

DebugHelper.ColorfulWrite(new ColorfulString(ConsoleColor.Green, Console.BackgroundColor, $"spikewall - process started at " + DateTime.Now.ToString() + "\n"));
DebugHelper.ColorfulWrite(new ColorfulString(ConsoleColor.Red, Console.BackgroundColor, "This is experimental software. Normal operation is not guaranteed. Do not use with real databases until the software is stable.\n\n"));

// TODO: May be a cleaner way of doing this
spikewall.Db.Initialize(ref builder);
spikewall.Db.SetupTables();

app.UseHttpLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
