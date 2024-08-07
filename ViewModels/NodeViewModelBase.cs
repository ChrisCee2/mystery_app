﻿using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using mystery_app.Messages;
using mystery_app.Models;

namespace mystery_app.ViewModels;

public abstract partial class NodeViewModelBase : ObservableObject
{
    public abstract NodeModelBase NodeBase { get; set; }

    [ObservableProperty]
    private bool _initialized;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isEdit;

    [ObservableProperty]
    private Control control;

    [RelayCommand]
    private void DeleteNode()
    {
        WeakReferenceMessenger.Default.Send(new DeleteNodeMessage(this));
    }

    [RelayCommand]
    private void CopyNode()
    {
        WeakReferenceMessenger.Default.Send(new CopyNodeMessage(this));
    }

    public abstract NodeViewModelBase Clone();

    partial void OnIsSelectedChanged(bool value)
    {
        if (value)
        {
            if (!WeakReferenceMessenger.Default.IsRegistered<MoveNodeMessage>(this))
            {
                WeakReferenceMessenger.Default.Register<MoveNodeMessage>(this, (sender, message) =>
                {
                    NodeBase.PositionX += message.Value.X;
                    NodeBase.PositionY += message.Value.Y;
                    control.RenderTransform = new TranslateTransform(NodeBase.PositionX, NodeBase.PositionY);
                });
            }
        }
        else
        {
            if (WeakReferenceMessenger.Default.IsRegistered<MoveNodeMessage>(this))
            {
                WeakReferenceMessenger.Default.Unregister<MoveNodeMessage>(this);
            }
        }
    }
}