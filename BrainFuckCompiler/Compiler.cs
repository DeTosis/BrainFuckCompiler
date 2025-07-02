using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;

namespace BrainFuckCompiler;
using static NotificationBuilder;
internal class Compiler {
    public struct BuildOptions {
        public static string filePath = "";
        public static string outputFile = "";
        public static bool buildAndRun = false;
    }

    static void Main(string[] args) {
        var arguments = args.ToList();
        if (arguments.Contains("-f")) {
            var index = arguments.IndexOf("-f");
            string path = "";
            if (arguments.Count > index + 1) {
                path = args[index + 1];
            } else {
                Console.WriteLine(BuildNotification("Incorrect [-f] usage", NotificationType.Exc));
                Environment.Exit(1);
            }

            if (!path.EndsWith(".bf")) {
                Console.WriteLine(BuildNotification("Incorrect input file format", NotificationType.Exc));
                Environment.Exit(1);
            }

            if (!File.Exists(path)) {
                Console.WriteLine(BuildNotification("System culd not find file specified", NotificationType.Exc));
                Environment.Exit(1);
            }

            path = path.TrimEnd(['.','b','f']);
            BuildOptions.filePath = path;
        } else {
            Console.WriteLine(BuildNotification("Incorrect usage", NotificationType.Exc));
            Environment.Exit(1);
        }

        if (arguments.Contains("-o")) {
            var index = arguments.IndexOf("-o");
            var file = "";
            if (arguments.Count > index + 1)
                file = args[index + 1];
            else {
                Console.WriteLine(BuildNotification("Incorrect [-o] usage", NotificationType.Exc));
                Environment.Exit(1);
            }

            if (!file.EndsWith(".exe")) {
                Console.WriteLine(BuildNotification("File format is not supported", NotificationType.Exc));
                Environment.Exit(1);
            }



            BuildOptions.outputFile = file;
        } else {
            BuildOptions.outputFile = BuildOptions.filePath + ".exe";
        }

        if (arguments.Contains("-run")) {
            BuildOptions.buildAndRun = true;
        }

        string asm = Compile();
        string exc;
        if (!Assemble(asm, out exc) || !Linq(out exc)) {
            Console.WriteLine(exc);
            Environment.Exit(1);
        }

        if (BuildOptions.buildAndRun) {
            RunProgram();
        }
    }

    public static string Compile() {
        string filePath = BuildOptions.filePath + ".bf";
        Processor processor = new(filePath);
        var tokens = processor.ParseTokens();

        Generator generator = new(tokens);
        var asm = generator.GenerateAssembly();

        return asm;
    }
    public static bool Assemble(string asm, out string exc) {
        File.WriteAllText($"{BuildOptions.outputFile.TrimEnd(".exe".ToArray())}.asm", asm);

        Process p = new Process();
        var psi = new ProcessStartInfo() {
            FileName = "nasm",
            Arguments = $"-f win64 {BuildOptions.outputFile.TrimEnd(".exe".ToArray())}.asm -o {BuildOptions.outputFile.TrimEnd(".exe".ToArray())}.obj",
            UseShellExecute = false,
            CreateNoWindow = false,
        };
        p.StartInfo = psi;

        p.Start();
        p.WaitForExit();
        if (p.ExitCode != 0) {
            exc = BuildNotification($"Compilation finished ExitCode:[{p.ExitCode}]", NotificationType.Suc);
            return false;
        } else {
            exc = BuildNotification(".obj generated", NotificationType.Suc);
            return true;
        }
    }
    public static bool Linq(out string exc) {
        Process p = new Process();

        var psi = new ProcessStartInfo() {
            FileName = "gcc",
            Arguments = $@"{BuildOptions.outputFile.TrimEnd(".exe".ToArray())}.obj -o {BuildOptions.outputFile}",
            UseShellExecute = false,
            CreateNoWindow = false,
        };

        p.StartInfo = psi;
        p.Start();
        p.WaitForExit();
        if (p.ExitCode != 0) {
            exc = BuildNotification($"Compilation finished ExitCode:[{p.ExitCode}]", NotificationType.Suc);
            return false;
        } else {
            exc = BuildNotification(".exe generated", NotificationType.Suc);
            return true;
        }
    }
    private static void RunProgram() {
        Process p = new();
        var psi = new ProcessStartInfo() {
            FileName = BuildOptions.outputFile,
            UseShellExecute = false,
        };

        p.StartInfo = psi;
        p.Start();
        p.WaitForExit();
    }
}