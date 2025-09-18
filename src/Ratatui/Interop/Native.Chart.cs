using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiChartNew();

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartFree(IntPtr chart);

    [StructLayout(LayoutKind.Sequential)]
    internal struct FfiChartDatasetSpec
    {
        public IntPtr NameUtf8;
        public IntPtr PointsXY;
        public UIntPtr LenPairs;
        public FfiStyle Style;
        public uint Kind;
    }

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_add_datasets", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartAddDatasets(IntPtr chart, [In] FfiChartDatasetSpec[] specs, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_add_dataset_with_type", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartAddDatasetWithType(IntPtr chart, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, double[] pointsXY, UIntPtr lenPairs, FfiStyle style, uint dtype);

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_add_line", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartAddLine(IntPtr chart, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, double[] pointsXY, UIntPtr lenPairs, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_axes_titles", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetAxesTitles(IntPtr chart, [MarshalAs(UnmanagedType.LPUTF8Str)] string? x, [MarshalAs(UnmanagedType.LPUTF8Str)] string? y);

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_x_labels_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetXLabelsSpans(IntPtr chart, IntPtr spans, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_y_labels_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetYLabelsSpans(IntPtr chart, IntPtr spans, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_axis_styles", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetAxisStyles(IntPtr chart, FfiStyle x, FfiStyle y);

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_labels_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetLabelsAlignment(IntPtr chart, uint alignX, uint alignY);

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_bounds", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetBounds(IntPtr chart, double xMin, double xMax, double yMin, double yMax);

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetStyle(IntPtr chart, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_legend_position", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetLegendPosition(IntPtr chart, uint position);

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_hidden_legend_constraints", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetHiddenLegendConstraints(IntPtr chart, uint[] kinds, ushort[] values);

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetBlockTitle(IntPtr chart, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_block_title_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetBlockTitleSpans(IntPtr chart, IntPtr spans, UIntPtr len, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetBlockAdv(IntPtr chart, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_chart_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawChartIn(IntPtr term, IntPtr chart, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_chart", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderChart(ushort width, ushort height, IntPtr chart, out IntPtr utf8Text);
}
