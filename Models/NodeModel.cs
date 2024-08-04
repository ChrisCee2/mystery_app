﻿using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia;
using mystery_app.Constants;
using Avalonia.Media.Imaging;

namespace mystery_app.Models;

public partial class NodeModel : NodeModelBase
{
    public NodeModel()
    {
        Name = "";
        Desc = "";
        Image = NodeConstants.DEFAULT_IMAGE;
        Width = NodeConstants.MIN_WIDTH;
        Height = NodeConstants.MIN_HEIGHT;
        Position = new Point(0, 0);
        Notes = "";
    }

    public NodeModel(
        string name,
        string desc,
        Bitmap image,
        double width,
        double height,
        Point position,
        string notes)
    {
        Name = name;
        Desc = desc;
        Image = image;
        Width = width;
        Height = height;
        Position = position;
        Notes = notes;
    }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private Bitmap _image;

    [ObservableProperty]
    private string _notes;
}