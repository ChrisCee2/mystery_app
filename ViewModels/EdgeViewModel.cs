﻿using Avalonia.Logging;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using dboard.Constants;
using dboard.Messages;
using dboard.Models;

namespace dboard.ViewModels;

public partial class EdgeViewModel : ObservableObject
{
    public EdgeViewModel(EdgeModel edgeModel)
    {
        Edge = edgeModel;
    }

    [ObservableProperty]
    private EdgeModel _edge;
    [ObservableProperty]
    private bool _isSelected;
    public bool DescIsNull() => Edge.Description is null;
    public bool DescIsNotNull() => Edge.Description is not null;

    [RelayCommand]
    private void Delete()
    {
        WeakReferenceMessenger.Default.Send(new DeleteMessage(""));
    }

    [RelayCommand(CanExecute = nameof(DescIsNull))]
    private void AddDesc()
    {
        Edge.Description = "";
    }

    [RelayCommand(CanExecute = nameof(DescIsNotNull))]
    private void RemoveDesc()
    {
        Edge.Description = null;
    }

    public EdgeViewModel Clone()
    {
        return new EdgeViewModel(new EdgeModel(
            Edge.FromNode,
            Edge.ToNode,
            Edge.Description,
            Edge.A,
            Edge.R,
            Edge.G,
            Edge.B,
            Edge.Thickness));
    }

    public EdgeViewModel CloneWithNewNodes(NodeModelBase fromNode, NodeModelBase toNode)
    {
        return new EdgeViewModel(new EdgeModel(
            fromNode,
            toNode,
            Edge.Description,
            Edge.A,
            Edge.R,
            Edge.G,
            Edge.B,
            Edge.Thickness));
    }
}