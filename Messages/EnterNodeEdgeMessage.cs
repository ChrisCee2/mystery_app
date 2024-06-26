﻿using CommunityToolkit.Mvvm.Messaging.Messages;
using mystery_app.ViewModels;

namespace mystery_app.Messages;
public class EnterNodeEdgeMessage : ValueChangedMessage<NodeViewModel>
{
    // Can change from string to a custom node class
    public EnterNodeEdgeMessage(NodeViewModel value) : base(value)
    {
    }
}