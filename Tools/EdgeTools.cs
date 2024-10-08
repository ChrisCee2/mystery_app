﻿using Avalonia;
using dboard.Constants;

namespace dboard.Tools;

class EdgeTools
{

    // Get point where edge should appear given a node's coordinates and width
    public static Point EdgePosFromNode(double x, double y, double width)
    {
        return new Point(x + (width / 2), y + EdgeConstants.distFromEdgeToNodePos);
    }
}