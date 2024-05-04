

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddPolicy(name: "FlickrOrigins",
    policy =>
    {
        policy.WithOrigins("https://andrebond777.github.io", "https://ambitious-desert-058928710.5.azurestaticapps.net", "http://localhost:4200", "https://photoyearguesser.azurewebsites.net").AllowAnyMethod().AllowAnyHeader();
    }));

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
if (app.Environment.IsDevelopment())
{

}

app.UseCors("FlickrOrigins");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
