using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using dboard.Constants;
using dboard.ViewModels;

namespace dboard.Views;

public partial class WorkspaceSettingsView : UserControl
{
    public WorkspaceSettingsView()
    {
        InitializeComponent();
    }

    public async void OpenFileButton_Clicked(object sender, RoutedEventArgs args)
    {
        var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Image",
            AllowMultiple = false
        });

        if (files.Count == 1)
        {
            if (((Control)sender).FindAncestorOfType<Control>().Name == WorkspaceConstants.CANVAS_IMAGE_NAME)
            {
                ((WorkspaceViewModel)DataContext).CanvasImagePath.Path = files[0].Path.LocalPath;
            }
            else if (((Control)sender).FindAncestorOfType<Control>().Name == WorkspaceConstants.WORKSPACE_IMAGE_NAME)
            {
                ((WorkspaceViewModel)DataContext).WorkspaceImagePath.Path = files[0].Path.LocalPath;
            }
            else if (((Control)sender).FindAncestorOfType<Control>().Name == WorkspaceConstants.WINDOW_IMAGE_NAME)
            {
                ((WorkspaceViewModel)DataContext).WindowImagePath.Path = files[0].Path.LocalPath;
            }
        }
    }

    public void RemoveImage(object sender, RoutedEventArgs args)
    {
        if (((Control)sender).FindAncestorOfType<Control>().Name == WorkspaceConstants.CANVAS_IMAGE_NAME)
        {
            ((WorkspaceViewModel)DataContext).CanvasImagePath.Path = null;
        }
        else if (((Control)sender).FindAncestorOfType<Control>().Name == WorkspaceConstants.WORKSPACE_IMAGE_NAME)
        {
            ((WorkspaceViewModel)DataContext).WorkspaceImagePath.Path = null;
        }
        else if (((Control)sender).FindAncestorOfType<Control>().Name == WorkspaceConstants.WINDOW_IMAGE_NAME)
        {
            ((WorkspaceViewModel)DataContext).WindowImagePath.Path = null;
        }
    }
}