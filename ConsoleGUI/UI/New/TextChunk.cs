using System.Collections;
using ConsoleGUI.ConsoleDisplay;

namespace ConsoleGUI.UI.New;

public class TextChunk : IEnumerable<char>
{
    public TextChunk(string content)
    {
        Content = content;
    }
    
    public Color Foreground { get; set; } = Color.Black;
    public Color Background { get; set; } = Color.Empty;

    public TextMode TextMode { get; set; } = TextMode.Default;
    public string Content { get; set; }
    
    public IEnumerator<char> GetEnumerator()
    {
        return Content.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}