using System.Runtime.CompilerServices;

namespace Ratatui.Interop;

internal static class ModuleInit
{
    [ModuleInitializer]
    internal static void Init()
    {
        // Ensure DllImport resolver is registered at assembly load time,
        // before any DllImport is resolved.
        Native.EnsureResolver();
    }
}

