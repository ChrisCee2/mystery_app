using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using dboard.Messages;
using dboard.ViewModels;

namespace dboard.Views;

public partial class WorkspaceView : UserControl
{
    public WorkspaceView()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<SelectNodeMessage>(this, (sender, args) =>
        {
            if (args.Value.IsEdit)
            {
                return;
            }
            else if (!((WorkspaceViewModel)DataContext).SelectedNodes.Contains(args.Value))
            {
                ((WorkspaceViewModel)DataContext).UpdateSelection(nodesToSelect: new ObservableCollection<NodeViewModelBase> { args.Value });
            }
        });

        WeakReferenceMessenger.Default.Register<SelectNodeEdgeMessage>(this, (sender, args) =>
        {
            ((WorkspaceViewModel)DataContext).UpdateSelection(edgesToSelect: new ObservableCollection<EdgeViewModel> { args.Value });
        });
    }
}
