﻿using System.Collections;
using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Old.Widgets;
using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.Widgets;

[DebuggerDisplay("Text {Content}, Fg: {Foreground.ToString()}, Bg: {Background.ToString()}")]
public class Text : Visual, IText
{
    public Text(string text)
    {
        _content = text;
        Size = new Vector(Length, 1);

        SetHandlers();
    }

    protected internal Text(string text, Component parent) : this(text) => Parent = parent;

    public virtual Color Foreground { get; set; } = Color.Black;
    public virtual Color Background { get; set; } = Color.Empty;

    public virtual TextMode TextMode { get; set; } = TextMode.Default;
    public Alignment Alignment { get; set; } = Alignment.Center;

    public string Content
    {
        get => _content;
        set => SetField(ref _content, value);
    }

    public int Length => Content.Length;

    public override bool Visible
    {
        get
        {
            if (Parent is null) return false;
            return _visible && Parent.GetProperty<bool>(nameof(Visible));
        }
        set => _visible = value;
    }

    private bool _visible = true;
    private string _content;

    private void SetHandlers()
    {
        void SetSize(Text component, PropertyChangedEventArgs args)
        {
            component.Size = new Vector((args.NewValue as string)!.Length, 1);
        }

        var handlers = new PropertyHandlerDefinitionCollection<Text>
        {
            {(nameof(Content), nameof(MinSize), nameof(MaxSize)), SetSize},
            {nameof(Size), (component, _) =>
            {
                component.Parent?.SetProperty("RequestedContentSpace", component.Size);
                if (component.Parent is not null) component.GlobalPosition = (component.Parent as ITextWidget<IText>).TextPosition;
            }}
        };

        HandlerManager.AddHandlers(handlers);
    }

    internal static TText CreateDefault<TText>(Component parent, string? content = null) where TText : Text
    {
        var args = new object[1];

        args[0] = content ?? parent.GetType().Name;

        var text = (TText) Activator.CreateInstance(typeof(TText), args)!;
        text.Parent = parent;

        return text;
    }

    internal override void Render()
    {
        if (Parent is null || Length == 0) return;

        var visibleSize = GetVisibleSize();
        if (visibleSize is {X: <= 0, Y: <= 0}) return;

        var span = Content.AsSpan(0, visibleSize.X);

        Display.Print(GlobalPosition.X, GlobalPosition.Y, span, Foreground, Background, Alignment, TextMode);
    }

    protected Vector GetVisibleSize()
    {
        var visibleSize = Size;

        var parentAllowsOverflow = Parent!.GetProperty<bool>("AllowTextOverflow");

        if (!parentAllowsOverflow)
        {
            var parentAllowedSpace = Parent!.GetProperty<Vector>("InnerSize");

            visibleSize.X = Math.Min(visibleSize.X, parentAllowedSpace.X);
            visibleSize.Y = Math.Min(visibleSize.Y, parentAllowedSpace.Y);
        }

        return visibleSize;
    }

    internal override void Clear()
    {
        var startPos = Alignment switch
        {
            Alignment.Left => GlobalPosition,
            Alignment.Right => new Vector(GlobalPosition.X - Width, GlobalPosition.Y),
            _ => new Vector(GlobalPosition.X - Width / 2, GlobalPosition.Y)
        };

        Display.ClearRect(startPos, Size);
    }

    public override void Delete()
    {
        Parent = null;
        base.Delete();
    }

    public IEnumerator<char> GetEnumerator() => Content.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => Content;
}