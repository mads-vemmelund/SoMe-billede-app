using System.IO;
using SkiaSharp;

namespace SoMeBillede;

public static class ImageProcessor
{
    public static (int Width, int Height) GetDimensions(string path)
    {
        using var bitmap = SKBitmap.Decode(path);
        if (bitmap is null)
        {
            throw new InvalidDataException("Filen kunne ikke læses som et understøttet billede.");
        }

        return (bitmap.Width, bitmap.Height);
    }

    public static byte[] RenderPreview(
        string path,
        SocialFormat format,
        bool fit,
        CanvasBackground background,
        int quarterTurns = 0)
    {
        using var source = Decode(path);
        using var result = Render(source, format, fit, background, false, quarterTurns);
        using var image = SKImage.FromBitmap(result);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    public static IReadOnlyList<string> Export(
        string sourcePath,
        string destination,
        string filePrefix,
        IReadOnlyList<SocialFormat> formats,
        bool fit,
        CanvasBackground background,
        SKEncodedImageFormat outputFormat,
        int quarterTurns = 0)
    {
        using var source = Decode(sourcePath);
        var extension = outputFormat == SKEncodedImageFormat.Jpeg ? ".jpg" : ".png";
        var quality = outputFormat == SKEncodedImageFormat.Jpeg ? 92 : 100;
        var prefix = SanitizePrefix(filePrefix);
        var savedPaths = new List<string>(formats.Count);

        foreach (var format in formats)
        {
            using var result = Render(
                source, format, fit, background, outputFormat == SKEncodedImageFormat.Jpeg, quarterTurns);
            using var image = SKImage.FromBitmap(result);
            using var data = image.Encode(outputFormat, quality);

            var baseName = $"{prefix}{format.Platform}-{format.Name}-{format.Width}x{format.Height}";
            var path = GetAvailablePath(destination, baseName, extension);
            using var output = File.Create(path);
            data.SaveTo(output);
            savedPaths.Add(path);
        }

        return savedPaths;
    }

    private static SKBitmap Decode(string path)
    {
        return SKBitmap.Decode(path)
            ?? throw new InvalidDataException("Filen kunne ikke læses som et understøttet billede.");
    }

    private static SKBitmap Render(
        SKBitmap source,
        SocialFormat format,
        bool fit,
        CanvasBackground background,
        bool flattenTransparency,
        int quarterTurns)
    {
        using var rotated = Rotate(source, quarterTurns);
        var image = rotated ?? source;
        var result = new SKBitmap(
            new SKImageInfo(format.Width, format.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
        using var canvas = new SKCanvas(result);
        var color = background switch
        {
            CanvasBackground.Black => SKColors.Black,
            CanvasBackground.Transparent when !flattenTransparency => SKColors.Transparent,
            _ => SKColors.White
        };
        canvas.Clear(color);

        var scaleX = (float)format.Width / image.Width;
        var scaleY = (float)format.Height / image.Height;
        var scale = fit ? Math.Min(scaleX, scaleY) : Math.Max(scaleX, scaleY);
        var drawnWidth = image.Width * scale;
        var drawnHeight = image.Height * scale;
        var destination = new SKRect(
            (format.Width - drawnWidth) / 2,
            (format.Height - drawnHeight) / 2,
            (format.Width + drawnWidth) / 2,
            (format.Height + drawnHeight) / 2);

        canvas.DrawBitmap(
            image,
            new SKRect(0, 0, image.Width, image.Height),
            destination,
            new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
        canvas.Flush();
        return result;
    }

    private static SKBitmap? Rotate(SKBitmap source, int quarterTurns)
    {
        var normalizedTurns = ((quarterTurns % 4) + 4) % 4;
        if (normalizedTurns == 0)
        {
            return null;
        }

        var swapsDimensions = normalizedTurns % 2 != 0;
        var width = swapsDimensions ? source.Height : source.Width;
        var height = swapsDimensions ? source.Width : source.Height;
        var rotated = new SKBitmap(new SKImageInfo(width, height, source.ColorType, source.AlphaType));
        using var canvas = new SKCanvas(rotated);
        canvas.Clear(SKColors.Transparent);
        canvas.Translate(width / 2f, height / 2f);
        canvas.RotateDegrees(normalizedTurns * 90);
        canvas.DrawBitmap(
            source,
            new SKRect(-source.Width / 2f, -source.Height / 2f, source.Width / 2f, source.Height / 2f),
            new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
        canvas.Flush();
        return rotated;
    }

    private static string SanitizePrefix(string prefix)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var sanitized = new string(prefix.Select(character =>
            invalidCharacters.Contains(character) ? '-' : character).ToArray()).Trim();
        return sanitized.Length == 0 ? string.Empty : $"{sanitized}-";
    }

    private static string GetAvailablePath(string destination, string baseName, string extension)
    {
        var path = Path.Combine(destination, baseName + extension);
        var suffix = 2;
        while (File.Exists(path))
        {
            path = Path.Combine(destination, $"{baseName} ({suffix++}){extension}");
        }

        return path;
    }
}
