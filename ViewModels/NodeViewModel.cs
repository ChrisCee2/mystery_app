﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using mystery_app.Models;

namespace mystery_app.ViewModels;
    
public partial class NodeViewModel : ObservableObject
{

    public NodeViewModel(string name, string desc)
    {
        Node = new NodeModel(name, desc);
    }

    [ObservableProperty]
    private NodeModel _node;
}