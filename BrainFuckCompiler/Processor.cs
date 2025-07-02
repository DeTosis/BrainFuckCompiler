using System;
using System.Collections.Generic;
using System.IO;

namespace BrainFuckCompiler;
using static NotificationBuilder;
internal class Processor {
    private string data;
    private int index;
    public enum TokenType {
        PTR_INC,
        PTR_DEC,
        DATA_INC,
        DATA_DEC,
        OUT,
        IN,
        OP_BRAC,
        CL_BRAC
    }

    public struct Token {
        public TokenType tt;
        public int repeatCount;
    }

    public Processor(string filePath) {
        try {
            data = File.ReadAllText(filePath);
            Console.WriteLine(BuildNotification("Data loaded", NotificationType.Suc));
        } catch (Exception e) {
            Console.WriteLine(BuildNotification(e.Message, NotificationType.Exc));
            Environment.Exit(1);
        }
    }

    public Token[] ParseTokens() {
        List<Token> tokens = new();

        while (index < data.Length) {
            switch (data[index]) {
                case '>':
                    tokens.Add(ParseToken('>', TokenType.PTR_INC));
                    break;
                case '<':
                    tokens.Add(ParseToken('<', TokenType.PTR_DEC));
                    break;
                case '+':
                    tokens.Add(ParseToken('+', TokenType.DATA_INC));
                    break;
                case '-':
                    tokens.Add(ParseToken('-', TokenType.DATA_DEC));
                    break;
                case '.':
                    tokens.Add(ParseToken('.', TokenType.OUT));
                    break;
                case ',':
                    tokens.Add(ParseToken(',', TokenType.IN));
                    break;
                case '[':
                    tokens.Add(new () { tt = TokenType.OP_BRAC });
                    ConsumeChar();
                    break;
                case ']':
                    tokens.Add(new() { tt = TokenType.CL_BRAC });
                    ConsumeChar();
                    break;
                default:
                    ConsumeChar();
                    break;
            }
        }

        Console.WriteLine(BuildNotification("Tokens Parsed", NotificationType.Suc));
        return tokens.ToArray();
    }

    private Token ParseToken(char rep, TokenType tt) {
        int repeatCount = 1;
        while (PeekChar(repeatCount) is char c && c == rep) {
            repeatCount++;
        }
        ConsumeChar(repeatCount);
        return new Token() { tt = tt, repeatCount = repeatCount };
    }

    private char? PeekChar(int offset = 1) {
        if (data.Length > index + offset) {
            return data[index + offset];
        }

        return null;
    }

    private void ConsumeChar(int count = 1) {
        index += count;
    }
}