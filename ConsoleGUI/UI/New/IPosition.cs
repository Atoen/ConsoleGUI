﻿namespace ConsoleGUI.UI.New;

public interface IPosition
{
    Vector Position { get; set; }
    Vector GlobalPosition { get; set; }
    Vector Center { get; set; }
}