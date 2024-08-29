﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace mystery_app.Models;

public partial class ImagePathModel : ObservableObject
{
    public ImagePathModel() {}

    public ImagePathModel(string name, string? path)
    {
        Name = name;
        Path = path;
    }

    [ObservableProperty]
    private string _name;
    [ObservableProperty]
    private string? _path;
}