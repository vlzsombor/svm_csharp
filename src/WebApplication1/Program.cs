using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Text.Json;
using ClassLibrary1;
using Microsoft.AspNetCore.Mvc;
using Static = WebApplication1.Static;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Add services to the container.
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

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
var jsonConfig = await File.ReadAllTextAsync("svm/C_5_gamma_0_05.json");
OneVsAllClassifier oneVsAllClassifier = JsonSerializer.Deserialize<OneVsAllClassifier>(jsonConfig) ?? throw new InvalidOperationException();
Random random = new Random();
string[] TestData = await File.ReadAllLinesAsync("svm/mnist_test.csv");
app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

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

        var imagereuslt = SavePixelsWithImageSharp(28, 28, p.Select(x=>Convert.ToByte(x)).ToArray(), Environment.CurrentDirectory);
        return oneVsAllClassifier.Predict(d);
        
        return "42";
/*
        var p = pixels.Split(',').SkipLast(1).Select(x=>(255-Convert.ToInt32(x))/255.0).ToArray();

        return oneVsAllClassifier.Predict(p);
        string base64Data = pixels.Substring(pixels.IndexOf(',') + 1);
        byte[] bytes = Convert.FromBase64String(base64Data);
        pixels = pixels.Split("base64,").Last();
//        byte[] bytes = Convert.FromBase64String(pixels);

        using var ms = new MemoryStream(bytes);
        using var bitmap = new Bitmap(ms);
        var resized = bitmap;
        List<double> doubles = new List<double>();
        var csv = new StringBuilder();
        for (int y = 0; y < resized.Height; y++)
        {
            for (int x = 0; x < resized.Width; x++)
            {
                Color pixel = resized.GetPixel(x, y);
                doubles.Add((254 - pixel.R) / 255.0);
//                arr = (255 - arr) / 255.
//                Color pixel = bitmap.GetPixel(x, y);
//                doubles.Add(pixel.G);
//                csv.Append($"{pixel.R},{pixel.G},{pixel.B}");
//                if (x < bitmap.Width - 1) csv.Append(",");
            }
            csv.AppendLine();
        }

//        var r = csv.ToString();
//        var enumerable = r.Split('\n').Select(Convert.ToDouble).ToArray();
        var result = oneVsAllClassifier.Predict(doubles.ToArray());
        return result;
        */
    })
    .WithName("SvmPrediction")
    .WithOpenApi();

app.Run();



static string SavePixelsWithImageSharp(int width, int height, byte[] rgbData, string filePath)
{
    
    using (Image<Rgb24> image = Image.LoadPixelData<Rgb24>(rgbData, width, height))
    {
        return image.ToBase64String(PngFormat.Instance);
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
