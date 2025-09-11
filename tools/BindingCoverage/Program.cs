using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            var opts = Options.Parse(args);
            var repoRoot = FindRepoRoot(opts.ProjectDir);
            if (repoRoot == null)
            {
                Console.Error.WriteLine($"error: could not locate repo root from {opts.ProjectDir}");
                return 2;
            }

            var ffiSrcDir = Path.Combine(repoRoot, "native", "ratatui-ffi", "src");
            if (!Directory.Exists(ffiSrcDir))
            {
                Console.Error.WriteLine($"error: FFI src directory not found at {ffiSrcDir}");
                return 2;
            }

            var ffiExports = ExtractFfiExportsFromRust(ffiSrcDir);
            if (ffiExports.Count == 0)
            {
                Console.Error.WriteLine("error: no FFI exports found in Rust sources");
                return 2;
            }

            var bindingSymbols = ExtractDllImportsFromSource(Path.Combine(opts.ProjectDir, "Interop"));
            if (bindingSymbols.Count == 0)
            {
                Console.Error.WriteLine("error: no DllImport entries found in binding assembly");
                return 2;
            }

            var missing = ffiExports.Except(bindingSymbols).OrderBy(s => s).ToList();
            var extra = bindingSymbols.Except(ffiExports).OrderBy(s => s).ToList();

            Console.WriteLine($"FFI coverage check — exports: {ffiExports.Count}, bindings: {bindingSymbols.Count}");
            if (missing.Count > 0)
            {
                Console.Error.WriteLine("Missing bindings (present in FFI, absent in C#):");
                foreach (var m in missing) Console.Error.WriteLine("  " + m);
            }
            if (extra.Count > 0)
            {
                Console.Error.WriteLine("Stale/extra bindings (not found in FFI source):");
                foreach (var e in extra) Console.Error.WriteLine("  " + e);
            }

            // Optional: emit stub suggestions for new/missing exports
            if (opts is { EmitSuggestionsPath: not null } && !string.IsNullOrEmpty(opts.EmitSuggestionsPath))
            {
                EmitSuggestions(opts.EmitSuggestionsPath!, missing, extra);
            }

            if (opts.EmitSuggestionsPath is { Length: >0 })
            {
                EmitSuggestions(opts.EmitSuggestionsPath!, missing, extra);
            }

            if (opts.EmitGeneratedPath is { Length: >0 })
            {
                EmitGenerated(opts.ProjectDir, opts.EmitGeneratedPath!, missing);
            }

            if (missing.Count > 0 || (opts.FailOnExtra && extra.Count > 0))
                return 3;

            Console.WriteLine("Coverage OK.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("coverage checker failed: " + ex);
            return 10;
        }
    }

    private sealed record Options(string AssemblyPath, string ProjectDir, bool FailOnExtra, string? EmitSuggestionsPath, string? EmitGeneratedPath)
    {
        public static Options Parse(string[] args)
        {
            string? asm = null;
            string? proj = null;
            bool failExtra = true;
            string? emit = null;
            string? gen = null;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--assembly": asm = args[++i]; break;
                    case "--project-dir": proj = args[++i]; break;
                    case "--allow-extra": failExtra = false; break;
                    case "--emit-suggestions": emit = args[++i]; break;
                    case "--emit-generated": gen = args[++i]; break;
                }
            }
            if (string.IsNullOrEmpty(asm) || !File.Exists(asm))
                throw new ArgumentException("--assembly path to built Ratatui.dll is required");
            if (string.IsNullOrEmpty(proj) || !Directory.Exists(proj))
                throw new ArgumentException("--project-dir directory is required");
            return new Options(Path.GetFullPath(asm!), Path.GetFullPath(proj!), failExtra, emit, gen);
        }
    }

    private static string? FindRepoRoot(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")) || Directory.Exists(Path.Combine(dir.FullName, "native")))
                return dir.FullName;
        }
        return null;
    }

    private static void EmitSuggestions(string path, List<string> missing, List<string> extra)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var sw = new StreamWriter(path, false, new UTF8Encoding(false));
            sw.WriteLine("// AUTO-GENERATED: FFI binding suggestions (no-op if coverage is 100%)");
            sw.WriteLine("// Missing exports (present in FFI, absent in C#):");
            foreach (var m in missing) sw.WriteLine("//   " + m);
            sw.WriteLine("// Extra/stale bindings (present in C#, absent in FFI):");
            foreach (var e in extra) sw.WriteLine("//   " + e);
            sw.WriteLine();
            if (missing.Count > 0)
            {
                sw.WriteLine("// Templates for missing [DllImport]s (adjust signatures):");
                sw.WriteLine("// using System; using System.Runtime.InteropServices;");
                sw.WriteLine("// internal static partial class Native { /* add to Native.cs */ ");
                foreach (var m in missing)
                {
                    sw.WriteLine($"// [DllImport(\\\"ratatui_ffi\\\", EntryPoint=\\\"{m}\\\", CallingConvention=CallingConvention.Cdecl)]");
                    sw.WriteLine($"// internal static extern void {ToPascal(m)}(IntPtr a, UIntPtr b);");
                }
                sw.WriteLine("// }");
            }
        }
        catch { /* best effort */ }
    }

    private static void EmitGenerated(string projectDir, string path, List<string> missing)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var sw = new StreamWriter(path, false, new UTF8Encoding(false));
            sw.WriteLine("// <auto-generated/>\nusing System;\nusing System.Runtime.InteropServices;\nnamespace Ratatui.Interop;\ninternal static partial class Native\n{");
            foreach (var name in missing)
            {
                // Heuristic stubs: defaults with IntPtr/UIntPtr and void return
                // Humans should refine the signature; this unblocks compilation if wired BeforeCompile.
                sw.WriteLine($"    [DllImport(\"ratatui_ffi\", EntryPoint=\"{name}\", CallingConvention=CallingConvention.Cdecl)]");
                sw.WriteLine($"    internal static extern void {ToPascal(name)}(IntPtr a, UIntPtr b);");
            }
            sw.WriteLine("}");
        }
        catch { /* best effort */ }
    }

    private static string ToPascal(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var parts = name.Split('_');
        var sb = new StringBuilder(parts.Length * 8);
        foreach (var p in parts) if (p.Length>0) sb.Append(char.ToUpperInvariant(p[0])).Append(p.AsSpan(1));
        return sb.ToString();
    }

    private static HashSet<string> ExtractFfiExportsFromRust(string ffiSrcDir)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        var files = Directory.EnumerateFiles(ffiSrcDir, "*.rs", SearchOption.AllDirectories)
            .OrderBy(p => p, StringComparer.Ordinal);

        // Coarse regex that matches no_mangle extern "C" fn names; resilient to whitespace/newlines.
        var rx = new Regex("#\\[no_mangle\\][\\s\\S]{0,200}?extern\\s+\"C\"\\s+fn\\s+([A-Za-z0-9_]+)\\(", RegexOptions.Compiled);
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            var matches = rx.Matches(text);
            foreach (Match m in matches)
            {
                var name = m.Groups[1].Value;
                if (name.StartsWith("ratatui_", StringComparison.Ordinal))
                    set.Add(name);
            }
        }
        return set;
    }

    private static HashSet<string> ExtractDllImportsFromSource(string interopDir)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        if (!Directory.Exists(interopDir)) return set;
        var files = Directory.EnumerateFiles(interopDir, "*.cs", SearchOption.AllDirectories)
            .OrderBy(p => p, StringComparer.Ordinal);
        // Capture EntryPoint names from DllImport attributes.
        var rx = new Regex("\\[DllImport\\([^]]*EntryPoint\\s*=\\s*\"(ratatui_[A-Za-z0-9_]+)\"[^]]*\\)\\]", RegexOptions.Multiline);
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            foreach (Match m in rx.Matches(text))
            {
                var name = m.Groups[1].Value;
                if (!string.IsNullOrEmpty(name)) set.Add(name);
            }
        }
        return set;
    }
}
