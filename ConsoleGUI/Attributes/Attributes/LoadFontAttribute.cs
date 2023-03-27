using ConsoleGUI.Visuals.Figlet;
using MethodBoundaryAspect.Fody.Attributes;

namespace ConsoleGUI.Attributes.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class LoadFontAttribute : OnMethodBoundaryAspect
{
    private static readonly Dictionary<string, Font> Fonts = new();

    public override void OnExit(MethodExecutionArgs arg)
    {
        var fontName = arg.Method.Name.Replace("get_", "");

        if (Fonts.TryGetValue(fontName, out var font))
        {
            arg.ReturnValue = font;
            return;
        }

        var newFont = LoadFont(fontName);
        Fonts.Add(fontName, newFont);

        arg.ReturnValue = newFont;
    }

    private static Font LoadFont(string fontName)
    {
        Debug.WriteLine($"Loading font {fontName}");

        var enumValue = (BuiltInFonts)Enum.Parse(typeof(BuiltInFonts), fontName);

        return Font.LoadBuiltInFont(enumValue);
    }
}