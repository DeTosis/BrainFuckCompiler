using System;
using System.Diagnostics;
using System.IO;

namespace BrainFuckCompiler;
using static NotificationBuilder;
internal class Compiler {
    static void Main(string[] args) {
        if (!ProcessArgs(args, out string exc)) {
            Console.WriteLine(exc);
            Environment.Exit(1);
        } else {
            Console.WriteLine(exc);
        }

        string filePath = args[1];

        Processor processor = new(filePath);
        var tokens = processor.ParseTokens();

        Generator generator = new(tokens);
        var asm = generator.GenerateAssembly();

        Assemble(asm);
        Linq();

        Console.WriteLine(BuildNotification("Compilation finished", NotificationType.Suc));
    }

    public static bool ProcessArgs(string[] args, out string exc) {
        if (args.Length != 2) {
            exc = BuildNotification("Invalid argument list, expected [-f] [filename]", NotificationType.Exc);
            return false;
        }

        if (args[0] != "-f") {
            exc = BuildNotification($"Arg[0] is invalid, expected [-f], got [{args[0]}]", NotificationType.Exc);
            return false;
        }

        string filePath = args[1];
        if (!File.Exists(filePath)) {
            exc = BuildNotification($"Provided file path is invalid, file [{filePath}] does not exist or unreachable", NotificationType.Exc);
            return false;
        }

        exc = BuildNotification($"Data reached successfully", NotificationType.Suc);
        return true;
    }

    public static void Assemble(string asm) {
        if (!Directory.Exists(@".\build")) {
            Directory.CreateDirectory(@".\build");
        }

        foreach (var i in Directory.GetFiles(@".\build")) {
            File.Delete(i);
        }

        File.WriteAllText(@".\build\output.asm", asm);

        Process p = new Process();
        p.StartInfo.FileName = "nasm";
        p.StartInfo.Arguments = @"-f win64 .\build\output.asm";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = false;
        p.Start();
        p.WaitForExit();
        if (p.ExitCode != 0) {
            Console.WriteLine(BuildNotification($"Compilation finished ExitCode:[{p.ExitCode}]", NotificationType.Suc));
            return;
        } else {
            Console.WriteLine(BuildNotification(".obj generated", NotificationType.Suc));
        }
    }

    public static void Linq() {
        Process p = new Process();
        p.StartInfo.FileName = "gcc";
        p.StartInfo.Arguments = @".\build\output.obj -o .\build\output.exe";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = false;
        p.Start();
        p.WaitForExit();
        if (p.ExitCode != 0) {
            Console.WriteLine(BuildNotification($"Compilation finished ExitCode:[{p.ExitCode}]", NotificationType.Suc));
            return;
        } else {
            Console.WriteLine(BuildNotification(".exe generated", NotificationType.Suc));
        }
    }
}