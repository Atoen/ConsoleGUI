﻿using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Events;
using ConsoleGUI.Visuals;

namespace ConsoleGUI.UI.Old;

public abstract class OldControl : VisualOldComponent
{
    protected OldControl() => Input.Register(this);

    public bool IsMouseOver { get; private set; }

    public bool Focused { get; private set; }
    public bool Focusable { get; set; } = true;
    public bool ShowFocusedBorder { get; set; } = true;
    public BorderStyle FocusBorderStyle { get; set; } = BorderStyle.Dotted;
    public Color FocusBorderColor { get; set; } = Color.Black;

    public int TabIndex { get; set; }


    private bool _hitTestVisible = true;
    public bool HitTestVisible
    {
        get => _hitTestVisible && Enabled;
        set => _hitTestVisible = value;
    }

    public override void Render()
    {
        base.Render();

        // Focus border should not override normal border
        if (Focused && ShowFocusedBorder && !ShowBorder)
        {
            Display.DrawBorder(GlobalPosition, Size, FocusBorderColor, FocusBorderStyle);
        }
    }

    public override void Remove()
    {
        Input.Unregister(this);
        base.Remove();
    }

    public delegate void MouseEventHandler(OldControl sender, MouseEventArgs e);
    public delegate void KeyboardEventHandler(OldControl sender, KeyboardEventArgs e);
    public delegate void FocusEventHandler(OldControl sender, InputEventArgs e);

    public event MouseEventHandler? MouseEnter;
    public event MouseEventHandler? MouseLeave;
    public event MouseEventHandler? MouseDown;
    public event MouseEventHandler? MouseLeftDown;
    public event MouseEventHandler? MouseRightDown;
    public event MouseEventHandler? MouseLeftUp;
    public event MouseEventHandler? MouseRightUp;
    public event MouseEventHandler? MouseMove;
    public event MouseEventHandler? MouseScroll;
    public event MouseEventHandler? DoubleClick;

    public event KeyboardEventHandler? KeyDown;
    public event KeyboardEventHandler? KeyUp;

    public event FocusEventHandler? GotFocus;
    public event FocusEventHandler? LostFocus;

    internal void SendMouseEvent(MouseEventType mouseEventType, MouseEventArgs e)
    {
        switch (mouseEventType)
        {
            case MouseEventType.MouseMove:
                OnMouseMove(e);
                break;

            case MouseEventType.MouseEnter:
                OnMouseEnter(e);
                break;

            case MouseEventType.MouseExit:
                OnMouseExit(e);
                break;

            case MouseEventType.MouseLeftDown:
                OnMouseLeftDown(e);
                break;

            case MouseEventType.MouseLeftUp:
                OnMouseLeftUp(e);
                break;

            case MouseEventType.MouseRightDown:
                OnMouseRightDown(e);
                break;

            case MouseEventType.MouseRightUp:
                OnMouseRightUp(e);
                break;

            case MouseEventType.MouseMiddleDown:
                OnMouseMiddleDown(e);
                break;

            case MouseEventType.MouseScroll:
                OnMouseScroll(e);
                break;

            case MouseEventType.DoubleClick:
                OnDoubleClick(e);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mouseEventType), mouseEventType, null);
        }

        if (e.Handled) return;

        // e.Source = this;
        (Parent as OldControl)?.SendMouseEvent(mouseEventType, e);
    }

    internal void SendKeyboardEvent(KeyboardEventType keyboardEventType, KeyboardEventArgs e)
    {
        if (keyboardEventType == KeyboardEventType.KeyDown)
        {
            OnKeyDown(e);
            return;
        }

        OnKeyUp(e);
    }

    internal void SendFocusEvent(FocusEventType focusEventType, InputEventArgs e)
    {
        // if (e.OriginalSource != this) return;

        if (focusEventType == FocusEventType.GotFocus)
        {
            OnGotFocus(e);
            return;
        }

        OnLostFocus(e);
    }

    protected virtual void OnMouseEnter(MouseEventArgs e)
    {
        IsMouseOver = true;
        MouseEnter?.Invoke(this, e);
    }

    protected virtual void OnMouseExit(MouseEventArgs e)
    {
        IsMouseOver = false;
        MouseLeave?.Invoke(this, e);
    }

    protected virtual void OnMouseMove(MouseEventArgs e)
    {
        MouseMove?.Invoke(this, e);
    }

    protected virtual void OnMouseLeftDown(MouseEventArgs e)
    {
        MouseLeftDown?.Invoke(this, e);
        MouseDown?.Invoke(this, e);
    }

    protected virtual void OnMouseLeftUp(MouseEventArgs e)
    {
        MouseLeftUp?.Invoke(this, e);
    }

    protected virtual void OnMouseRightDown(MouseEventArgs e)
    {
        MouseRightDown?.Invoke(this, e);
        MouseDown?.Invoke(this, e);
    }

    protected virtual void OnMouseRightUp(MouseEventArgs e)
    {
        MouseRightUp?.Invoke(this, e);
    }

    protected virtual void OnMouseMiddleDown(MouseEventArgs e)
    {
        MouseDown?.Invoke(this, e);
    }

    protected virtual void OnMouseScroll(MouseEventArgs e)
    {
        MouseScroll?.Invoke(this, e);
    }

    protected virtual void OnDoubleClick(MouseEventArgs e)
    {
        DoubleClick?.Invoke(this, e);
    }

    protected virtual void OnGotFocus(InputEventArgs e)
    {
        Focused = true;
        GotFocus?.Invoke(this, e);
    }

    protected virtual void OnLostFocus(InputEventArgs e)
    {
        Focused = false;
        LostFocus?.Invoke(this, e);
    }

    protected virtual void OnKeyDown(KeyboardEventArgs e)
    {
        KeyDown?.Invoke(this, e);
    }

    protected virtual void OnKeyUp(KeyboardEventArgs e)
    {
        KeyUp?.Invoke(this, e);
    }
}

public enum MouseEventType
{
    MouseMove,
    MouseEnter,
    MouseExit,
    MouseLeftDown,
    MouseLeftUp,
    MouseRightDown,
    MouseRightUp,
    MouseMiddleDown,
    MouseScroll,
    DoubleClick
}

public enum KeyboardEventType
{
    KeyDown,
    KeyUp
}

public enum FocusEventType
{
    GotFocus,
    LostFocus
}
