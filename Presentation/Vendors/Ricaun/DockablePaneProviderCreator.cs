using System.Windows;
using Autodesk.Revit.UI;

namespace ClashOpenings.Presentation.Vendors.Ricaun;

internal class DockablePaneProviderCreator : IDockablePaneProvider
{
    private readonly IDockablePaneProvider dockablePaneProvider;
    private readonly FrameworkElement frameworkElement;

    public DockablePaneProviderCreator(FrameworkElement frameworkElement)
    {
        this.frameworkElement = frameworkElement;
    }

    public DockablePaneProviderCreator(FrameworkElement frameworkElement, IDockablePaneProvider dockablePaneProvider) :
        this(frameworkElement)
    {
        this.dockablePaneProvider = dockablePaneProvider;
    }

    public void SetupDockablePane(DockablePaneProviderData data)
    {
        dockablePaneProvider?.SetupDockablePane(data);
        data.FrameworkElement = frameworkElement;
    }
}