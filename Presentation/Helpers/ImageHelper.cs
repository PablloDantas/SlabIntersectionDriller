using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace ClashOpenings.Presentation.Helpers
{
    public static class ImageHelper
    {
        public static BitmapImage? LoadImageSource(string iconName)
        {
            try
            {
                // Get path to the installation folder (where the add-in is deployed)
                string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                string assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
                
                // Path to the icons folder from the post-build destination
                string iconPath = Path.Combine(assemblyDirectory, "Resources", "Icons", iconName);
                
                // Check if file exists
                if (!File.Exists(iconPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Icon file not found: {iconPath}");
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
                System.Diagnostics.Debug.WriteLine($"Error loading icon '{iconName}': {ex.Message}");
                return null;
            }
        }
    }
}