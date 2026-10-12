using System.IO;
using SkiaSharp;

namespace SoMeBillede.Tests;

public class ImageProcessorTests
{
    [Fact]
    public void Export_WritesAllPresetDimensionsWithoutOverwriting()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            var sourcePath = Path.Combine(directory, "source.png");
            using (var bitmap = new SKBitmap(new SKImageInfo(160, 90)))
            {
                bitmap.Erase(SKColors.CornflowerBlue);
                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                File.WriteAllBytes(sourcePath, data.ToArray());
            }

            var formats = new[]
            {
                new SocialFormat("Facebook", "Opslag", 1200, 1200),
                new SocialFormat("Facebook", "Historier", 1080, 1920),
                new SocialFormat("Facebook", "Coverbillede", 851, 315),
                new SocialFormat("Facebook", "Profilfoto", 170, 170),
                new SocialFormat("Instagram", "Opslag", 1080, 1080),
                new SocialFormat("Instagram", "Historier", 1080, 1920),
                new SocialFormat("Instagram", "Profilfoto", 320, 320),
                new SocialFormat("LinkedIn", "Profilfoto", 400, 400),
                new SocialFormat("LinkedIn", "Coverbillede", 1128, 191),
                new SocialFormat("LinkedIn", "Opslag", 1080, 1080)
            };

            var firstExport = ImageProcessor.Export(
                sourcePath,
                directory,
                "Test",
                formats,
                true,
                CanvasBackground.Transparent,
                SKEncodedImageFormat.Jpeg);

            Assert.Equal(formats.Length, firstExport.Count);
            for (var index = 0; index < formats.Length; index++)
            {
                using var output = SKBitmap.Decode(firstExport[index]);
                Assert.NotNull(output);
                Assert.Equal(formats[index].Width, output.Width);
                Assert.Equal(formats[index].Height, output.Height);
                Assert.Equal(255, output.GetPixel(0, 0).Alpha);
            }

            var secondExport = ImageProcessor.Export(
                sourcePath,
                directory,
                "Test",
                formats,
                true,
                CanvasBackground.Transparent,
                SKEncodedImageFormat.Jpeg);

            Assert.Equal(formats.Length, secondExport.Count);
            Assert.All(firstExport, path => Assert.True(File.Exists(path)));
            Assert.All(secondExport, path => Assert.Contains("(2).jpg", path));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
