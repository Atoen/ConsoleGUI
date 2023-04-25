using System.Collections.ObjectModel;

namespace ConsoleGUI.Visuals;

public static class BorderSymbols
{
    public static readonly ReadOnlyDictionary<BorderStyle, Dictionary<BorderFragment, char>> Symbols = new(
        new Dictionary<BorderStyle, Dictionary<BorderFragment, char>>
        {
            {
                BorderStyle.Single, new Dictionary<BorderFragment, char>
                {
                    {BorderFragment.Vertical, '│'},
                    {BorderFragment.Horizontal, '─'},
                    {BorderFragment.UpperLeft, '┌'},
                    {BorderFragment.UpperRight, '┐'},
                    {BorderFragment.LowerLeft, '└'},
                    {BorderFragment.LowerRight, '┘'}
                }
            },

            {
                BorderStyle.SingleBold, new Dictionary<BorderFragment, char>
                {
                    {BorderFragment.Vertical, '┃'},
                    {BorderFragment.Horizontal, '━'},
                    {BorderFragment.UpperLeft, '┏'},
                    {BorderFragment.UpperRight, '┓'},
                    {BorderFragment.LowerLeft, '┗'},
                    {BorderFragment.LowerRight, '┛'}
                }
            },

            {
                BorderStyle.Double, new Dictionary<BorderFragment, char>
                {
                    {BorderFragment.Vertical, '║'},
                    {BorderFragment.Horizontal, '═'},
                    {BorderFragment.UpperLeft, '╔'},
                    {BorderFragment.UpperRight, '╗'},
                    {BorderFragment.LowerLeft, '╚'},
                    {BorderFragment.LowerRight, '╝'}
                }
            },

            {
                BorderStyle.Rounded, new Dictionary<BorderFragment, char>
                {
                    {BorderFragment.Vertical, '│'},
                    {BorderFragment.Horizontal, '─'},
                    {BorderFragment.UpperLeft, '╭'},
                    {BorderFragment.UpperRight, '╮'},
                    {BorderFragment.LowerLeft, '╰'},
                    {BorderFragment.LowerRight, '╯'}
                }
            },

            {
                BorderStyle.Dashed, new Dictionary<BorderFragment, char>
                {
                    {BorderFragment.Vertical, '┆'},
                    {BorderFragment.Horizontal, '╌'},
                    {BorderFragment.UpperLeft, '┌'},
                    {BorderFragment.UpperRight, '┐'},
                    {BorderFragment.LowerLeft, '└'},
                    {BorderFragment.LowerRight, '┘'}
                }
            },

            {
                BorderStyle.Dotted, new Dictionary<BorderFragment, char>
                {
                    {BorderFragment.Vertical, '╎'},
                    {BorderFragment.Horizontal, '-'},
                    {BorderFragment.UpperLeft, '·'},
                    {BorderFragment.UpperRight, '·'},
                    {BorderFragment.LowerLeft, '·'},
                    {BorderFragment.LowerRight, '·'}
                }
            },

            {
                BorderStyle.Ascii, new Dictionary<BorderFragment, char>
                {
                    {BorderFragment.Vertical, '|'},
                    {BorderFragment.Horizontal, '-'},
                    {BorderFragment.UpperLeft, '·'},
                    {BorderFragment.UpperRight, '·'},
                    {BorderFragment.LowerLeft, '·'},
                    {BorderFragment.LowerRight, '·'}
                }
            }
        });
}