namespace ConsoleGUI.UI.Widgets;

public interface ITextWidget<out TText> where TText : IText
{
    TText Text { get; }

    bool AllowTextOverflow { get; }

    Vector TextPosition { get; }
}