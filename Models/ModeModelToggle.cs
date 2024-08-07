﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace mystery_app.Models;

public partial class ModeModelToggle : ModeModel
{
    public ModeModelToggle() {}

    public ModeModelToggle(string name, ModeModel offMode, ModeModel onMode)
    {
        Name = name;
        OffMode = offMode;
        OnMode = onMode;

        // Settings default to off mode
        Name = OffMode.Name;
        A = OffMode.A;
        R = OffMode.R;
        G = OffMode.G;
        B = OffMode.B;
        ShowItems = OffMode.ShowItems;
        WorkspaceOpacity = OffMode.WorkspaceOpacity;
        WindowState = OffMode.WindowState;
    }

    [ObservableProperty]
    private bool _isToggled = false;

    [ObservableProperty]
    private ModeModel _offMode; // Untoggled

    [ObservableProperty]
    private ModeModel _onMode; // Toggled

    public void Toggle()
    {
        IsToggled = !IsToggled;
        UpdateMode();
    }

    public void Toggle(bool isToggled)
    {
        IsToggled = isToggled;
        UpdateMode();
    }

    public void UpdateMode()
    {
        var mode = IsToggled ? OnMode : OffMode;
        A = mode.A;
        R = mode.R;
        G = mode.G;
        B = mode.B;
        ShowItems = mode.ShowItems;
        WorkspaceOpacity = mode.WorkspaceOpacity;
    }
}
