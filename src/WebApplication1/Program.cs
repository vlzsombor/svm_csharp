using System.Text.Json;
using ClassLibrary1;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

var jsonConfig = await File.ReadAllTextAsync("svm/C_5_gamma_0_05.json");
OneVsAllClassifier oneVsAllClassifier = JsonSerializer.Deserialize<OneVsAllClassifier>(jsonConfig) ?? throw new InvalidOperationException();
Random random = new Random();

string[] TestData = await File.ReadAllLinesAsync("svm/mnist_test.csv");

app.MapGet("/getTestData", () =>
{
    var r = random.Next(TestData.Length);
    var line = TestData[r];
    return line[2..];
}).WithOpenApi();

app.MapPost("/predict", ([FromBody] string pixels) =>
    {
        var p = pixels.Split(',');
        double[] d = new double[28 * 28];
        for (int i = 0; i < p.Length; i += 4)
        {
            var r = Convert.ToInt32(p[i]);
            var g = Convert.ToInt32(p[i + 1]);
            var b = Convert.ToInt32(p[i + 2]);
            var a = Convert.ToInt32(p[i + 3]);
            var gray = 0.299 * r + 0.587 * g + 0.114 * b;
            d[i / 4] = gray/255;
        }
        return oneVsAllClassifier.Predict(d);
    })
    .WithName("SvmPrediction")
    .WithOpenApi();

app.Run();
