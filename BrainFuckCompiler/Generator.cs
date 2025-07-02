using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Xml;
using static BrainFuckCompiler.NotificationBuilder;
using static BrainFuckCompiler.Processor;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BrainFuckCompiler; 
internal class Generator {
    private Token[] tokens;
    int index; 

    public Generator(Token[] tokens) {
        this.tokens = tokens;
    }

    int maxLoop = 0;
    List<int> activeLoops = new();

    public string GenerateAssembly() {
        Nasm nasm = new();
        nasm.AddHeader();
        while (index < tokens.Length) {
            switch (tokens[index].tt) {
                case TokenType.PTR_INC:
                    nasm.IncCellPtr(tokens[index].repeatCount);
                    break;
                case TokenType.PTR_DEC:
                    nasm.DecCellPtr(tokens[index].repeatCount);
                    break;
                case TokenType.DATA_INC:
                    nasm.IncCellData(tokens[index].repeatCount);
                    break;
                case TokenType.DATA_DEC:
                    nasm.DecCellData(tokens[index].repeatCount);
                    break;
                case TokenType.OUT:
                    nasm.Print();
                    break;
                case TokenType.IN:
                    break;
                case TokenType.OP_BRAC:
                    nasm.LoopStart(maxLoop);
                    activeLoops.Add(maxLoop);
                    maxLoop++;
                    break;
                case TokenType.CL_BRAC:
                    nasm.LoopEnd(activeLoops.Last());
                    activeLoops.Remove(activeLoops.Last());
                    break;
            }
            ConsumeToken();
        }

        nasm.Exit(0);

        Console.WriteLine(BuildNotification("Assembly generated", NotificationType.Suc));
        return nasm.Output;
    }

    private Token? PeekToken(int offset = 1) {
        if (tokens.Length > index + offset) {
            return tokens[index + offset];
        }

        return null;
    }

    private void ConsumeToken(int count = 1) {
        index += count;
    }
}

internal class Nasm {
    public string Output { get; private set; } = "";

    private void AddExtern() {
        Output += "extern ExitProcess".Format(false);
        Output += "extern printf\n".Format(false);
    }

    public void AddHeader() {
        AddExtern();

        Output += "section .bss".Format(false);
        Output += "tape resb 30000\n".Format(false);

        Output += "section .data".Format(false);
        Output += "fmt db \"%c\",0\n".Format();

        Output += "global main\n".Format(false);

        Output += "section .text".Format(false);
        Output += "main:".Format(false);
        Output += "mov rsi, tape".Format();

        Output += "\n";
    }

    public void IncCellData(int count = 1) {
        for (int x = 0; x < count; x++) {
            Output += "inc byte [rsi]".Format();
        }
    }

    public void DecCellData(int count = 1) {
        for (int x = 0; x < count; x++) {
            Output += "dec byte [rsi]".Format();
        }
    }

    public void IncCellPtr(int count = 1) {
        for (int x = 0; x < count; x++) {
            Output += "inc rsi".Format();
        }
        Output += "\n";
    }

    public void DecCellPtr(int count = 1) {
        for (int x = 0; x < count; x++) {
            Output += "dec rsi".Format();
        }
        Output += "\n";
    }

    public void Exit(int code) {
        Output += $"mov rsi, {code}".Format();
        Output += $"call ExitProcess\n".Format();
    }

    public void Print() {
        Output += "sub rsp, 40".Format();
        Output += "mov rcx, fmt".Format();
        Output += $"movzx rdx, byte [rsi]".Format();
        Output += "call printf".Format();
        Output += "add rsp, 40\n".Format();
    }

    public void LoopStart(int loopIndex) {
        Output += "\n";
        Output += $".loop{loopIndex}_start:".Format(false);

        Output += "cmp byte[rsi], 0".Format();
        Output += $"je .loop{loopIndex}_end".Format();
        Output += "\n";
    }

    public void LoopEnd(int loopIndex) {
        Output += $"jmp .loop{loopIndex}_start".Format();

        Output += "\n";
        Output += $".loop{loopIndex}_end:".Format(false);
    }
}

public static class Formatter {
    public static string Format(this string content, bool prefTab = true) {
        if (prefTab) {
            return $"    {content}\n";
        } else {
            return $"{content}\n";
        }
    }
}
