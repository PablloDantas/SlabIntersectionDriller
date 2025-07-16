using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace ClashOpenings.Helpers;

/// <summary>
///     Fornece métodos utilitários para carregar imagens de recursos incorporados.
/// </summary>
public static class ImageHelper
{
    /// <summary>
    ///     Carrega uma imagem BitmapImage de um arquivo de ícone especificado localizado na pasta de recursos.
    /// </summary>
    /// <param name="iconName">O nome do arquivo do ícone (ex: "my_icon.png").</param>
    /// <returns>
    ///     Um objeto <see cref="BitmapImage" /> se o ícone for carregado com sucesso;
    ///     caso contrário, <see langword="null" />.
    /// </returns>
    public static BitmapImage? LoadImageSource(string iconName)
    {
        try
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

            var iconPath = Path.Combine(assemblyDirectory, "Resources", "Icons", iconName);

            if (!File.Exists(iconPath))
            {
                Debug.WriteLine($"Icon file not found: {iconPath}");
                return null;
            }

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(iconPath, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad; // Load the image when created to avoid file locking
            image.EndInit();
            image.Freeze(); // Optimize for UI thread

            return image;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading icon '{iconName}': {ex.Message}");
            return null;
        }
    }
}