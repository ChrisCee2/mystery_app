﻿using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using mystery_app.Constants;
using mystery_app.Messages;
using mystery_app.Models;

namespace mystery_app.ViewModels;

public partial class WorkspaceViewModel : ObservableObject
{
    public ObservableCollection<NodeViewModelBase> Nodes { get; set; } = new ObservableCollection<NodeViewModelBase>();
    public EdgeCollectionModel Edges { get; set; } = new EdgeCollectionModel();
    [ObservableProperty]
    private NodeViewModelBase _nodeToCreateEdge = NodeConstants.NULL_NODEVIEWMODEL;
    [ObservableProperty]
    private Point _cursorPosition;
    [ObservableProperty]
    private Point _pressedPosition;
    [ObservableProperty]
    private int _edgeThickness;
    [ObservableProperty]
    private int _multiSelectThickness;
    [ObservableProperty]
    private bool _isMultiSelecting;
    [ObservableProperty]
    private ObservableCollection<NodeViewModelBase> _selectedNodes = new ObservableCollection<NodeViewModelBase>();
    [ObservableProperty]
    private ObservableCollection<EdgeViewModel> _selectedEdges = new ObservableCollection<EdgeViewModel>();
    [ObservableProperty]
    private SettingsModel _sharedSettings;
    [ObservableProperty]
    private ObservableCollection<NodeViewModelBase> _copiedNodes = new ObservableCollection<NodeViewModelBase>();

    private bool CanPaste() => CopiedNodes.Count > 0;
    private bool ItemsAreSelected() => SelectedNodes.Count > 0 || SelectedEdges.Count > 0;
    private bool NodesAreSelected() => SelectedNodes.Count > 0;

    public WorkspaceViewModel(SettingsModel sharedSettings)
    {
        _sharedSettings = sharedSettings;
        
        WeakReferenceMessenger.Default.Register<CreateNodeEdgeMessage>(this, (sender, message) =>
        {
            if (message.Value is NodeViewModelBase nodeVMBase && Nodes.Contains(nodeVMBase))
            {
                EdgeThickness = EdgeConstants.THICKNESS;
                NodeToCreateEdge = nodeVMBase;
                // Manually assign position to where pin is
                CursorPosition = new Point(NodeToCreateEdge.NodeBase.PositionX + (NodeToCreateEdge.NodeBase.Width / 2), NodeToCreateEdge.NodeBase.PositionY + 11);
            }
        });

        WeakReferenceMessenger.Default.Register<ReleaseNodeEdgeMessage>(this, (sender, message) =>
        {
            var enteredNode = message.Value;

            if (Nodes.Contains(enteredNode) 
                && Nodes.Contains(NodeToCreateEdge)
                && !Equals(enteredNode, NodeToCreateEdge)
                && !Edges.ContainsEdge(NodeToCreateEdge.NodeBase, enteredNode.NodeBase))
            {
                Edges.Add(new EdgeViewModel(new EdgeModel(NodeToCreateEdge.NodeBase, enteredNode.NodeBase)));
            }
            NodeToCreateEdge = NodeConstants.NULL_NODEVIEWMODEL;
        });

        WeakReferenceMessenger.Default.Register<DeleteMessage>(this, (sender, message) =>
        {
            DeleteSelectedItems();
        });

        WeakReferenceMessenger.Default.Register<CopyNodeMessage>(this, (sender, message) =>
        {
            _CopyNodes();
        });
    }

    [RelayCommand]
    private void CreateNode()
    {
        _CreateEmptyNode();
    }

    [RelayCommand(CanExecute = nameof(CanPaste))]
    private void PasteNode()
    {
        foreach (var node in CopiedNodes)
        {
            Nodes.Add(node.Clone(Nodes.Count));
        }
    }

    [RelayCommand(CanExecute = nameof(NodesAreSelected))]
    private void CopyNode()
    {
        _CopyNodes();
    }

    private void _CreateEmptyNode()
    {
        Nodes.Add(new NodeViewModel(new NodeModel(Nodes.Count)));
    }

    private void _CopyNodes()
    {
        CopiedNodes = new ObservableCollection<NodeViewModelBase>();
        foreach (var node in SelectedNodes)
        {
            CopiedNodes.Add(node.Clone());
        }
        // Reorder nodes
        CopiedNodes = new ObservableCollection<NodeViewModelBase>(CopiedNodes.OrderBy(nodevm => nodevm.NodeBase.ZIndex).ToList());
    }

    private void _DeleteNodes()
    {
        // Deregisters move messenger through the property listener in InteractiveView
        foreach (var nodeVM in SelectedNodes)
        {
            nodeVM.IsSelected = false;
        }

        // Remove nodes
        Nodes.RemoveMany(SelectedNodes);

        // Remove edges
        var edgesToRemove = new Collection<EdgeViewModel>();
        foreach (EdgeViewModel edgeViewModel in Edges)
        {
            foreach (NodeViewModelBase nodeVM in SelectedNodes)
            {
                if (nodeVM.NodeBase == edgeViewModel.Edge.FromNode || nodeVM.NodeBase == edgeViewModel.Edge.ToNode)
                {
                    edgesToRemove.Add(edgeViewModel);
                    break;
                }
            }
        }
        Edges.RemoveMany(edgesToRemove);

        // Reorder z indexes
        foreach (var nodeVM in Nodes)
        {
            var decrement = 0;
            foreach (var deletedNodeVM in SelectedNodes)
            {
                if (nodeVM.NodeBase.ZIndex > deletedNodeVM.NodeBase.ZIndex)
                {
                    decrement++;
                }
            }
            nodeVM.NodeBase.ZIndex -= decrement;
        }

        SelectedNodes = new ObservableCollection<NodeViewModelBase>();
    }

    private void _DeleteEdges()
    {
        // Remove edges
        Edges.RemoveMany(SelectedEdges);
        SelectedEdges = new ObservableCollection<EdgeViewModel>();
    }

    [RelayCommand(CanExecute = nameof(ItemsAreSelected))]
    private void DeleteSelectedItems()
    {
        _DeleteNodes();
        _DeleteEdges();
    }

    private void _UpdateSelectedNodes(ObservableCollection<NodeViewModelBase> nodesToSelect)
    {
        foreach (var nodeVM in SelectedNodes)
        {
            nodeVM.IsSelected = false;
            nodeVM.IsEdit = false;
        }

        foreach (NodeViewModelBase nodeVM in nodesToSelect)
        {
            nodeVM.IsSelected = true;
            nodeVM.IsEdit = false;
        }

        // Reorder z indexes
        nodesToSelect = new ObservableCollection<NodeViewModelBase>(nodesToSelect.OrderBy(nodevm => nodevm.NodeBase.ZIndex).ToList());
        foreach (var nodeVM in Nodes)
        {
            if (!nodesToSelect.Contains(nodeVM))
            {
                var decrement = 0;
                foreach (var selectedNode in nodesToSelect)
                {
                    if (nodeVM.NodeBase.ZIndex > selectedNode.NodeBase.ZIndex)
                    {
                        decrement++;
                    }
                }
                nodeVM.NodeBase.ZIndex -= decrement;
            }
        }
        foreach (var nodeVM in nodesToSelect)
        {
            nodeVM.NodeBase.ZIndex = Nodes.Count - (nodesToSelect.Count - nodesToSelect.IndexOf(nodeVM));
        }

        SelectedNodes = nodesToSelect;
    }

    private void _UpdateSelectedEdges(ObservableCollection<EdgeViewModel> edgesToSelect)
    {
        foreach (var edgeVM in SelectedEdges)
        {
            edgeVM.IsSelected = false;
        }

        foreach (EdgeViewModel edgeVM in edgesToSelect)
        {
            edgeVM.IsSelected = true;
        }

        SelectedEdges = edgesToSelect;
    }

    public void UpdateSelection(ObservableCollection<NodeViewModelBase>? nodesToSelect = null, ObservableCollection<EdgeViewModel>? edgesToSelect = null)
    {
        nodesToSelect = nodesToSelect is null ? new ObservableCollection<NodeViewModelBase> () : nodesToSelect;
        edgesToSelect = edgesToSelect is null ? new ObservableCollection<EdgeViewModel>() : edgesToSelect;
        _UpdateSelectedNodes(nodesToSelect);
        _UpdateSelectedEdges(edgesToSelect);
    }
}
