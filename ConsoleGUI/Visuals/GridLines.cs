using System.Collections.ObjectModel;

namespace ConsoleGUI.Visuals;

public static class GridLines
{
    public static readonly ReadOnlyDictionary<GridLineStyle, Dictionary<GridLineFragment, char>> Symbols = new(
        new Dictionary<GridLineStyle, Dictionary<GridLineFragment, char>>
        {
            {
                GridLineStyle.Single, new Dictionary<GridLineFragment, char>
                {
                    {GridLineFragment.Vertical, '│'},
                    {GridLineFragment.Horizontal, '─'},
                    {GridLineFragment.HorizontalDown, '┬'},
                    {GridLineFragment.HorizontalUp, '┴'},
                    {GridLineFragment.VerticalRight, '├'},
                    {GridLineFragment.VerticalLeft, '┤'},
                    {GridLineFragment.Cross, '┼'}
                }
            },

            {
                GridLineStyle.SingleBold, new Dictionary<GridLineFragment, char>
                {
                    {GridLineFragment.Vertical, '┃'},
                    {GridLineFragment.Horizontal, '━'},
                    {GridLineFragment.HorizontalDown, '┳'},
                    {GridLineFragment.HorizontalUp, '┻'},
                    {GridLineFragment.VerticalRight, '┣'},
                    {GridLineFragment.VerticalLeft, '┫'},
                    {GridLineFragment.Cross, '╋'}
                }
            },

            {
                GridLineStyle.Double, new Dictionary<GridLineFragment, char>
                {
                    {GridLineFragment.Vertical, '║'},
                    {GridLineFragment.Horizontal, '═'},
                    {GridLineFragment.HorizontalDown, '╦'},
                    {GridLineFragment.HorizontalUp, '╩'},
                    {GridLineFragment.VerticalRight, '╠'},
                    {GridLineFragment.VerticalLeft, '╣'},
                    {GridLineFragment.Cross, '╬'}
                }
            },

            {
                GridLineStyle.Dashed, new Dictionary<GridLineFragment, char>
                {
                    {GridLineFragment.Vertical, '╎'},
                    {GridLineFragment.Horizontal, '-'},
                    {GridLineFragment.HorizontalDown, '·'},
                    {GridLineFragment.HorizontalUp, '·'},
                    {GridLineFragment.VerticalRight, '·'},
                    {GridLineFragment.VerticalLeft, '·'},
                    {GridLineFragment.Cross, '+'}
                }
            },

            {
                GridLineStyle.Ascii, new Dictionary<GridLineFragment, char>
                {
                    {GridLineFragment.Vertical, '|'},
                    {GridLineFragment.Horizontal, '-'},
                    {GridLineFragment.HorizontalDown, '·'},
                    {GridLineFragment.HorizontalUp, '·'},
                    {GridLineFragment.VerticalRight, '·'},
                    {GridLineFragment.VerticalLeft, '·'},
                    {GridLineFragment.Cross, '+'}
                }
            }
        });

}