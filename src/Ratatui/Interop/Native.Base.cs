using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    internal const string LibraryName = "ratatui_ffi";

    internal static void EnsureResolver()
    {
        try
        {
            NativeLibrary.SetDllImportResolver(typeof(Native).Assembly, Resolve);
            RatatuiFfiVersion(out _, out _, out _);
            ValidateVersion();
        }
        catch
        {
            // Resolver already registered for this assembly.
        }
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        EnsureResolver();
        if (!string.Equals(libraryName, LibraryName, StringComparison.Ordinal))
            return IntPtr.Zero;

        var baseDir = AppContext.BaseDirectory;
        var fileName = GetPlatformLibraryFileName(LibraryName);

        static bool TryLoad(string path, out IntPtr handle)
        {
            if (File.Exists(path) && NativeLibrary.TryLoad(path, out handle))
                return true;
            handle = IntPtr.Zero;
            return false;
        }

        var envDir = Environment.GetEnvironmentVariable("RATATUI_FFI_DIR");
        if (!string.IsNullOrEmpty(envDir) && TryLoad(Path.Combine(envDir, fileName), out var handle))
            return handle;

        var envPath = Environment.GetEnvironmentVariable("RATATUI_FFI_PATH");
        if (!string.IsNullOrEmpty(envPath) && TryLoad(Path.GetFullPath(envPath), out handle))
            return handle;

        ReadOnlySpan<string> candidates = new[]
        {
            Path.Combine(baseDir, fileName),
            Path.Combine(baseDir, "runtimes", GetRid(), "native", fileName)
        };
        foreach (var path in candidates)
            if (TryLoad(path, out handle)) return handle;

        var runtimesDir = Path.Combine(baseDir, "runtimes");
        if (Directory.Exists(runtimesDir))
        {
            foreach (var ridDir in Directory.EnumerateDirectories(runtimesDir))
            {
                var candidate = Path.Combine(ridDir, "native", fileName);
                if (TryLoad(candidate, out handle)) return handle;
            }
        }

        for (int up = 3; up <= 6; up++)
        {
            var ups = Enumerable.Repeat("..", up).ToArray();
            var debugPath = Path.Combine(new[] { baseDir }.Concat(ups).Concat(new[] { "native", "ratatui_ffi", "target", "debug", fileName }).ToArray());
            if (TryLoad(debugPath, out handle)) return handle;
            var releasePath = Path.Combine(new[] { baseDir }.Concat(ups).Concat(new[] { "native", "ratatui_ffi", "target", "release", fileName }).ToArray());
            if (TryLoad(releasePath, out handle)) return handle;
        }

        return IntPtr.Zero;
    }

    private static string GetPlatformLibraryFileName(string baseName)
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? baseName + ".dll"
         : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "lib" + baseName + ".dylib"
         : "lib" + baseName + ".so";

    private static string GetRid()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "win-arm64" : (Environment.Is64BitProcess ? "win-x64" : "win-x86");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "linux-arm64" : "linux-x64";
        return "linux-x64";
    }

    [DllImport(LibraryName, EntryPoint = "ratatui_ffi_version", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiFfiVersion(out uint major, out uint minor, out uint patch);

    [DllImport(LibraryName, EntryPoint = "ratatui_ffi_feature_bits", CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint RatatuiFfiFeatureBits();

    private static void ValidateVersion()
    {
        if (!RatatuiFfiVersion(out _, out _, out _))
            throw new DllNotFoundException("ratatui_ffi version query failed; ensure compatible native library present.");
    }

    internal enum FfiEventKind : uint { None = 0, Key = 1, Resize = 2, Mouse = 3 }

    internal enum FfiMouseKind : uint { Down = 1, Up = 2, Drag = 3, Moved = 4, ScrollUp = 5, ScrollDown = 6 }

    internal enum FfiMouseButton : uint { None = 0, Left = 1, Right = 2, Middle = 3 }

    internal enum FfiWidgetKind : uint
    {
        Paragraph = 1,
        List = 2,
        Table = 3,
        Gauge = 4,
        Tabs = 5,
        BarChart = 6,
        Sparkline = 7,
        Chart = 8,
        Scrollbar = 9,
    }

    internal enum FfiColor : uint
    {
        Reset = 0,
        Black,
        Red,
        Green,
        Yellow,
        Blue,
        Magenta,
        Cyan,
        Gray,
        DarkGray,
        LightRed,
        LightGreen,
        LightYellow,
        LightBlue,
        LightMagenta,
        LightCyan,
        White,
    }

    [Flags]
    internal enum FfiStyleMods : ushort
    {
        None = 0,
        Bold = 1 << 0,
        Italic = 1 << 1,
        Underline = 1 << 2,
        Dim = 1 << 3,
        Crossed = 1 << 4,
        Reversed = 1 << 5,
        RapidBlink = 1 << 6,
        SlowBlink = 1 << 7,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FfiStyle
    {
        public uint Fg;
        public uint Bg;
        public ushort Mods;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FfiRect
    {
        public ushort X;
        public ushort Y;
        public ushort Width;
        public ushort Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FfiKeyEvent
    {
        public uint Code;
        public uint Ch;
        public byte Mods;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FfiEvent
    {
        public uint Kind;
        public FfiKeyEvent Key;
        public ushort Width;
        public ushort Height;
        public ushort MouseX;
        public ushort MouseY;
        public uint MouseKind;
        public uint MouseBtn;
        public byte MouseMods;
    }

    [StructLayout(LayoutKind.Sequential)] internal struct FfiSpan { public IntPtr TextUtf8; public FfiStyle Style; }
    [StructLayout(LayoutKind.Sequential)] internal struct FfiLineSpans { public IntPtr Spans; public UIntPtr Len; }
    [StructLayout(LayoutKind.Sequential)] internal struct FfiCellLines { public IntPtr Lines; public UIntPtr Len; }
    [StructLayout(LayoutKind.Sequential)] internal struct FfiRowCellsLines { public IntPtr Cells; public UIntPtr Len; }

    [StructLayout(LayoutKind.Sequential)] internal struct FfiU16Slice { public IntPtr Ptr; public UIntPtr Len; }

    [StructLayout(LayoutKind.Sequential)] internal struct FfiDrawCmd { public uint Kind; public IntPtr Handle; public FfiRect Rect; }
}
