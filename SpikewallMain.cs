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

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"spikewall - process started at " + DateTime.Now.ToString());
Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine("This is experimental software. Normal operation is not guaranteed. Do not use with real databases until the software is stable.\n");
Console.ResetColor();

spikewall.Db.Initialize(ref builder);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
