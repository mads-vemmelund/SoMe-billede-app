using System.Collections.ObjectModel;

namespace SoMeBillede;

public sealed class SocialFormat
{
    public SocialFormat(string platform, string name, int width, int height)
    {
        Platform = platform;
        Name = name;
        Width = width;
        Height = height;
    }

    public string Platform { get; }
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public string Dimensions => $"{Width} × {Height}";
    public string DisplayName => $"{Platform} · {Name} · {Dimensions}";
    public bool IsSelected { get; set; }
}

public sealed class SocialFormatGroup
{
    public SocialFormatGroup(string name, IEnumerable<SocialFormat> formats)
    {
        Name = name;
        Formats = new ObservableCollection<SocialFormat>(formats);
    }

    public string Name { get; }
    public ObservableCollection<SocialFormat> Formats { get; }
}

public enum CanvasBackground
{
    White,
    Black,
    Transparent
}
