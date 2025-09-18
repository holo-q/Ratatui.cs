using System;

namespace Ratatui;

public sealed class Block
{
    private Borders _borders = global::Ratatui.Borders.None;
    private BorderType _borderType = global::Ratatui.BorderType.Plain;
    private Padding _pad = global::Ratatui.Padding.All(0);
    private Alignment _titleAlign = Alignment.Left;
    private string? _title;
    private bool _titleExplicit;
    private bool _showBorder;

    public Block() { }

    public static Block Default => new();

    public Block Borders(Borders borders)
    {
        _borders = borders;
        return this;
    }

    public Block BorderType(BorderType type)
    {
        _borderType = type;
        return this;
    }

    public Block Padding(Padding pad)
    {
        _pad = pad;
        return this;
    }

    public Block Padding(ushort value) => Padding(global::Ratatui.Padding.All(value));

    public Block Padding(ushort left, ushort top, ushort right, ushort bottom)
        => Padding(new global::Ratatui.Padding(left, top, right, bottom));

    public Block Title(string? title, bool showBorder = true)
    {
        _title = title;
        _titleExplicit = true;
        _showBorder = showBorder;
        return this;
    }

    public Block TitleAlignment(Alignment alignment)
    {
        _titleAlign = alignment;
        return this;
    }

    internal BlockSpec Spec => new(
        _borders,
        _borderType,
        _pad,
        _titleAlign,
        _title,
        _titleExplicit,
        _showBorder);

    internal readonly struct BlockSpec
    {
        internal BlockSpec(
            Borders borders,
            BorderType borderType,
            Padding padding,
            Alignment titleAlign,
            string? title,
            bool titleExplicit,
            bool showBorder)
        {
            Borders = borders;
            BorderType = borderType;
            Padding = padding;
            TitleAlignment = titleAlign;
            Title = title;
            TitleExplicit = titleExplicit;
            ShowBorder = showBorder;
        }

        internal Borders Borders { get; }
        internal BorderType BorderType { get; }
        internal Padding Padding { get; }
        internal Alignment TitleAlignment { get; }
        internal string? Title { get; }
        internal bool TitleExplicit { get; }
        internal bool ShowBorder { get; }
    }
}
