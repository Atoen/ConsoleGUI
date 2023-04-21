namespace ConsoleGUI.UI.New;

public interface ITextWidget<out TText> where TText : IText
{
    TText Text { get; }

    bool AllowTextOverflow { get; set; }

    Vector TextOffset { get; set; }
}