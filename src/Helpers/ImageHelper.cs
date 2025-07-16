using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace ClashOpenings.src.Helpers;

public static class ImageHelper
{
    public static BitmapImage? LoadImageSource(string iconName)
    {
        try
        {
            // Get path to the installation folder (where the add-in is deployed)
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

            // Path to the icons folder from the post-build destination
            var iconPath = Path.Combine(assemblyDirectory, "Resources", "Icons", iconName);

            // Check if file exists
            if (!File.Exists(iconPath))
            {
                Debug.WriteLine($"Icon file not found: {iconPath}");
                return null;
            }

            // Create bitmap from file
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
            // Log error if needed
            Debug.WriteLine($"Error loading icon '{iconName}': {ex.Message}");
            return null;
        }
    }
}