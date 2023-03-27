using System.Collections.ObjectModel;
using System.Reflection;
using ConsoleGUI.Attributes.Attributes;

namespace ConsoleGUI.Visuals.Figlet;

public class Font
{
    [LoadFont] public static Font Default => default!;
    [LoadFont] public static Font FourMax => default!;
    [LoadFont] public static Font Amc3 => default!;
    [LoadFont] public static Font AnsiRegular => default!;
    [LoadFont] public static Font AnsiShadow => default!;
    [LoadFont] public static Font BigFig => default!;
    [LoadFont] public static Font CalvinS => default!;
    [LoadFont] public static Font Cyber => default!;

    public required int Height { get; init;}
    public required int BaseLine { get; init;}
    public required int MaxLength { get; init;}
    public required ReadOnlyDictionary<char, string[]> Symbols { get; init; }

    private const string FontFileSignature = "flf2a";
    private const char Space = ' ';

    public string GetCharacterLine(char @char, int lineIndex)
    {
        return Symbols.TryGetValue(@char, out var symbol)
            ? symbol[lineIndex]
            : Symbols[Space][lineIndex];
    }

    public static Font LoadBuiltInFont(BuiltInFonts builtInFont)
    {
        var fileName = FontFileName(builtInFont);
        var resourceName = $"{typeof(Font).GetTypeInfo().Namespace}.{fileName}.flf";

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            throw new FileNotFoundException($"{builtInFont} font resource file not found.");
        }

        return LoadFont(stream);
    }

    private static string FontFileName(BuiltInFonts builtInFont) => builtInFont switch
    {
        BuiltInFonts.Default => "default",
        BuiltInFonts.FourMax => "4max",
        BuiltInFonts.Amc3 => "amc3",
        BuiltInFonts.AnsiRegular => "ansi regular",
        BuiltInFonts.AnsiShadow => "ansi shadow",
        BuiltInFonts.BigFig => "bigfig",
        BuiltInFonts.CalvinS => "calvin s",
        BuiltInFonts.Cyber => "cyber",
        _ => throw new ArgumentOutOfRangeException(nameof(builtInFont), builtInFont, null)
    };

    public static Font LoadFont(Stream stream)
    {
        using var reader = new StreamReader(stream);

        var config = reader.ReadLine()?.Split(' ');

        if (config == null || !config[0].StartsWith(FontFileSignature))
        {
            throw new FileLoadException("Font file is missing the signature");
        }

        var hardBlank = config[0].Last();
        var height = int.Parse(config[1]);
        var baseLine = int.Parse(config[2]);
        var maxLength = int.Parse(config[3]);
        var commentLines = int.Parse(config[5]);

        // Skipping comment lines
        for (var line = 0; line < commentLines; line++)
        {
            reader.ReadLine();
        }

        var dict = new Dictionary<char, string[]>();

        var currentChar = Space;

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine() ?? string.Empty;

            if (int.TryParse(line, out var charIndex))
            {
                currentChar = (char)charIndex;
            }

            dict.Add(currentChar, new string[height]);

            var lineIndex = 0;
            while (lineIndex < height)
            {
                dict[currentChar][lineIndex] = line.TrimEnd('@').Replace(hardBlank, Space);

                if (line.EndsWith("@@")) break;

                line = reader.ReadLine() ?? string.Empty;
                lineIndex++;
            }

            currentChar++;
        }

        return new Font
        {
            Height = height,
            BaseLine = baseLine,
            MaxLength = maxLength,
            Symbols = new ReadOnlyDictionary<char, string[]>(dict)
        };
    }
}

public enum BuiltInFonts
{
    Default,
    FourMax,
    Amc3,
    AnsiRegular,
    AnsiShadow,
    BigFig,
    CalvinS,
    Cyber
}

public enum CharacterWidth
{
    Smush,
    Fitted,
    Full
}