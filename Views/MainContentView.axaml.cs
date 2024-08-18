using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using mystery_app.Constants;
using mystery_app.Models;
using mystery_app.ViewModels;

namespace mystery_app.Views;

public partial class MainContentView : DockPanel
{

    private bool _isResizingNotes;
    private double _initialResizeX;
    private double _lastNotesLen;
    private SplitView _notesSplitView;

    JsonSerializerOptions options = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        WriteIndented = true
    };

    FilePickerFileType jsonFileType = new FilePickerFileType("json") { Patterns = new[] { "*.json" } };

    public MainContentView()
    {
        InitializeComponent();
        _notesSplitView = this.FindControl<SplitView>("NotesSplitView");
    }

    protected async void Save(object sender, RoutedEventArgs e)
    {
        if (((MainContentViewModel)DataContext).WorkspaceFileName == null)
        {
            SaveAs(sender, e);
        }
        else
        {
            if (!Directory.Exists("./Workspaces"))
            {
                Directory.CreateDirectory("./Workspaces");
            }
            IStorageFolder directory = await TopLevel.GetTopLevel(this).StorageProvider.TryGetFolderFromPathAsync("./Workspaces");
            string path = directory.TryGetLocalPath() + "/" + ((MainContentViewModel)DataContext).WorkspaceFileName;
            Logger.TryGet(LogEventLevel.Fatal, LogArea.Control)?.Log(this, path);
            IStorageFile file = await TopLevel.GetTopLevel(this).StorageProvider.TryGetFileFromPathAsync(path);
            if (file is not null)
            {
                _SaveFile(file);
            }
        }
    }

    protected async void SaveAs(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists("./Workspaces"))
        {
            Directory.CreateDirectory("./Workspaces");
        }

        IStorageFolder directory = await TopLevel.GetTopLevel(this).StorageProvider.TryGetFolderFromPathAsync("./Workspaces");

        IStorageFile file = await TopLevel.GetTopLevel(this).StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Workspace",
            SuggestedFileName = "Workspace",
            SuggestedStartLocation = directory,
            DefaultExtension = "json"
        });

        if (file is not null)
        {
            _SaveFile(file);
        }
    }

    private async void _SaveFile(IStorageFile file)
    {
        // Open writing stream from the file.
        await using var stream = await file.OpenWriteAsync();

        List<NodeModelBase> nodes = ((MainContentViewModel)DataContext).Workspace.Nodes.Select(x => x.NodeBase).ToList();
        List<EdgeModel> edges = ((MainContentViewModel)DataContext).Workspace.Edges.Select(x => x.Edge).ToList();
        NotesModel notes = ((MainContentViewModel)DataContext).Notes;
        WorkspaceModel workspace = new WorkspaceModel(nodes, edges, notes);
        JsonSerializer.SerializeAsync(stream, workspace, options);
        ((MainContentViewModel)DataContext).WorkspaceFileName = file.Name;
    }

    protected async void Open(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists("./Workspaces"))
        {
            Directory.CreateDirectory("./Workspaces");
        }

        IStorageFolder directory = await TopLevel.GetTopLevel(this).StorageProvider.TryGetFolderFromPathAsync("./Workspaces");

        var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Workspace",
            AllowMultiple = false,
            SuggestedStartLocation = directory,
            FileTypeFilter = new[] { jsonFileType }
        });

        if (files.Count == 1)
        {
            ((MainContentViewModel)DataContext).NewCommand.Execute(null);
            await using var stream = await files[0].OpenReadAsync();
            WorkspaceModel workspace = JsonSerializer.Deserialize<WorkspaceModel>(stream, options);
            foreach (var node in workspace.Nodes)
            {
                if (node is NodeModel nodeModel)
                {
                    ((MainContentViewModel)DataContext).Workspace.Nodes.Add(new NodeViewModel(nodeModel));
                }
            }
            foreach (EdgeModel edgeModel in workspace.Edges)
            {
                ((MainContentViewModel)DataContext).Workspace.Edges.Add(new EdgeViewModel(edgeModel));
            }
            ((MainContentViewModel)DataContext).Notes = workspace.Notes;
            ((MainContentViewModel)DataContext).WorkspaceFileName = files[0].Name;
        }
    }

    protected void Exit(object sender, RoutedEventArgs e)
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.Close();
        }
    }

    protected void Minimize(object sender, RoutedEventArgs e)
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.WindowState = WindowState.Minimized;
        }
    }

    protected void Maximize(object sender, RoutedEventArgs e)
    {
        ToggleMaximize();
    }

    protected void MoveWindow(object sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(Parent as Visual).Properties.IsLeftButtonPressed) { return; }
        if (e.ClickCount >= 2)
        {
            ToggleMaximize();
            return;
        }
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.BeginMoveDrag(e);
        }
    }

    protected void ToggleMaximize()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow.WindowState == WindowState.Maximized)
            {
                desktop.MainWindow.WindowState = WindowState.Normal;
            }
            else
            {
                desktop.MainWindow.WindowState = WindowState.Maximized;
            }
        }
    }

    // On selecting resize button, get current cursor position and set axis to resize
    private void ResizePointerPressed(object sender, PointerPressedEventArgs e)
    {
        // If not left click, return
        if (!e.GetCurrentPoint(Parent as Visual).Properties.IsLeftButtonPressed) { return; }
        _isResizingNotes = true;
        var pos = e.GetPosition((Visual?)Parent);
        _initialResizeX = pos.X;
        _lastNotesLen = _notesSplitView.OpenPaneLength;
    }

    protected void ResizePointerReleased(object sender, PointerReleasedEventArgs e)
    {
        _isResizingNotes = false;
    }

    protected void ResizePointerMoved(object sender, PointerEventArgs e)
    {
        if (Parent == null || !_isResizingNotes) { return; }
        Point currentPosition = e.GetPosition((Visual?)Parent);

        if (_isResizingNotes)
        {
            // Offset found by subtracting original cursor position on resize press from the current cursor position
            double offsetX = _initialResizeX - currentPosition.X;
            ((MainContentViewModel)DataContext).Notes.PaneLength = Math.Min(ToolbarConstants.NOTES_PANE_MAX_LEN, Math.Max(ToolbarConstants.NOTES_PANE_MIN_LEN, _lastNotesLen + offsetX));
        }
    }
}