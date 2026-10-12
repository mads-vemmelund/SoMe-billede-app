using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using SkiaSharp;

namespace SoMeBillede;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string? _sourcePath;
    private string? _lastDestination;
    private CancellationTokenSource? _previewCancellation;

    public MainWindow()
    {
        InitializeComponent();
        FormatGroups = new ObservableCollection<SocialFormatGroup>
        {
            new("FACEBOOK", new[]
            {
                new SocialFormat("Facebook", "Opslag", 1200, 1200),
                new SocialFormat("Facebook", "Historier", 1080, 1920),
                new SocialFormat("Facebook", "Coverbillede", 851, 315),
                new SocialFormat("Facebook", "Profilfoto", 170, 170)
            }),
            new("INSTAGRAM", new[]
            {
                new SocialFormat("Instagram", "Opslag", 1080, 1080),
                new SocialFormat("Instagram", "Historier", 1080, 1920),
                new SocialFormat("Instagram", "Profilfoto", 320, 320)
            }),
            new("LINKEDIN", new[]
            {
                new SocialFormat("LinkedIn", "Profilfoto", 400, 400),
                new SocialFormat("LinkedIn", "Coverbillede", 1128, 191),
                new SocialFormat("LinkedIn", "Opslag", 1080, 1080)
            })
        };
        DataContext = this;
        PreviewFormatBox.ItemsSource = FormatGroups.SelectMany(group => group.Formats).ToList();
        PreviewFormatBox.SelectedIndex = 0;
    }

    public ObservableCollection<SocialFormatGroup> FormatGroups { get; }

    private async void ChooseImageButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Vælg et billede",
            Filter = "Billedfiler|*.jpg;*.jpeg;*.png;*.webp|Alle filer|*.*",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            var dimensions = await Task.Run(() => ImageProcessor.GetDimensions(dialog.FileName));
            _sourcePath = dialog.FileName;
            SourceNameText.Text = Path.GetFileName(_sourcePath);
            SourceNameText.ToolTip = _sourcePath;
            ImageDimensionsText.Text = $"Originalbillede · {dimensions.Width} × {dimensions.Height} px";
            EmptyPreview.Visibility = Visibility.Collapsed;
            PreviewImage.Visibility = Visibility.Visible;
            OpenFolderButton.Visibility = Visibility.Collapsed;
            StatusText.Text = "Billedet er klar til tilpasning.";
            await RefreshPreviewAsync();
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, exception.Message, "Kunne ikke åbne billedet", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void PreviewFormatBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _ = RefreshPreviewAsync();
    }

    private void Adaptation_Checked(object sender, RoutedEventArgs e)
    {
        _ = RefreshPreviewAsync();
    }

    private void BackgroundBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _ = RefreshPreviewAsync();
    }

    private async Task RefreshPreviewAsync()
    {
        if (_sourcePath is null || PreviewFormatBox.SelectedItem is not SocialFormat format)
        {
            return;
        }

        _previewCancellation?.Cancel();
        _previewCancellation?.Dispose();
        var cancellation = _previewCancellation = new CancellationTokenSource();
        var sourcePath = _sourcePath;
        var fit = FitRadio.IsChecked == true;
        var background = GetBackground();
        PreviewSizeText.Text = $"{format.Width} × {format.Height} px";

        try
        {
            var bytes = await Task.Run(
                () => ImageProcessor.RenderPreview(sourcePath, format, fit, background),
                cancellation.Token);
            if (cancellation.IsCancellationRequested)
            {
                return;
            }

            using var stream = new MemoryStream(bytes);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            PreviewImage.Source = bitmap;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            StatusText.Text = $"Forhåndsvisningen kunne ikke opdateres: {exception.Message}";
        }
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_sourcePath is null)
        {
            MessageBox.Show(this, "Vælg først et billede, du vil eksportere.", "Intet billede valgt",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var selectedFormats = FormatGroups.SelectMany(group => group.Formats)
            .Where(format => format.IsSelected).ToList();
        if (selectedFormats.Count == 0)
        {
            MessageBox.Show(this, "Vælg mindst ét format i listen.", "Ingen formater valgt",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var destination = _lastDestination;
        if (destination is null || !Directory.Exists(destination))
        {
            var folderDialog = new OpenFolderDialog
            {
                Title = "Vælg destinationsmappe",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };
            if (folderDialog.ShowDialog(this) != true)
            {
                return;
            }

            destination = folderDialog.FolderName;
        }

        var selectedOutput = OutputFormatBox.SelectedItem as ComboBoxItem;
        var outputFormat = selectedOutput?.Tag?.ToString() == "Png"
            ? SKEncodedImageFormat.Png
            : SKEncodedImageFormat.Jpeg;
        var sourcePath = _sourcePath;
        var fit = FitRadio.IsChecked == true;
        var background = GetBackground();
        var prefix = FilePrefixBox.Text;

        ExportButton.IsEnabled = false;
        StatusText.Text = $"Eksporterer {selectedFormats.Count} billeder …";
        try
        {
            var savedPaths = await Task.Run(() => ImageProcessor.Export(
                sourcePath, destination, prefix, selectedFormats, fit, background, outputFormat));
            _lastDestination = destination;
            DestinationText.Text = _lastDestination;
            StatusText.Text = $"Færdig. {savedPaths.Count} billeder gemt.";
            OpenFolderButton.Visibility = Visibility.Visible;
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            StatusText.Text = "Eksporten mislykkedes.";
            MessageBox.Show(this, exception.Message, "Kunne ikke eksportere billederne",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ExportButton.IsEnabled = true;
        }
    }

    private void ChooseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Vælg destinationsmappe",
            InitialDirectory = Directory.Exists(_lastDestination) ? _lastDestination : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
        };
        if (dialog.ShowDialog(this) == true)
        {
            _lastDestination = dialog.FolderName;
            DestinationText.Text = _lastDestination;
        }
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (_lastDestination is { } destination && Directory.Exists(destination))
        {
            Process.Start(new ProcessStartInfo(destination) { UseShellExecute = true });
        }
    }

    private CanvasBackground GetBackground()
    {
        var selected = BackgroundBox.SelectedItem as ComboBoxItem;
        return selected?.Tag?.ToString() switch
        {
            "Black" => CanvasBackground.Black,
            "Transparent" => CanvasBackground.Transparent,
            _ => CanvasBackground.White
        };
    }
}