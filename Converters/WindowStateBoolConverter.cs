﻿using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace dboard.Converters;
public class WindowStateBoolConverter : IValueConverter
{
    public static readonly WindowStateBoolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is WindowState val && parameter is string expectedState)
        {
            return val == (WindowState)Enum.Parse(typeof(WindowState), expectedState);
        }
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }

}
