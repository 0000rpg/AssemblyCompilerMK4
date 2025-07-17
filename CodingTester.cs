using System;
using System.Collections.Generic;
using System.Linq;

class CodeTester
{
    public static void Main()
    {
        var coder = new Coder(enableDebug: true);

        // Тестовые примеры
        var testCases = new[]
        {
            "MOV A1, 10",
            "MOV A2, 88",
            "MUL 7",
            "MUL 8",
            "ADD 5",
            "JMP 0",
            "CMPS",
            "MOVS",
            "MOV A3, FF",
            "DIV 3"
        };

        foreach (var testCase in testCases)
        {
            Console.WriteLine($"\n{'='.ToString().PadLeft(60, '=')}");
            Console.WriteLine($"Input: {testCase}");
            Console.WriteLine($"{'='.ToString().PadLeft(60, '=')}");

            var result = coder.Compile(testCase);

            if (coder.ErrorCharIndex.HasValue)
            {
                Console.WriteLine($"\nERROR at position {coder.ErrorCharIndex}: {coder.ErrorReason}");
                Console.WriteLine($"Symbol: '{coder.ErrorSymbol}'");
            }
            else
            {
                Console.WriteLine($"\nCompilation successful!");
                PrintResult(result, testCase);
            }

            // Сброс ошибок для следующего теста
            coder = new Coder(enableDebug: false);
        }

        // Тест составной программы
        Console.WriteLine($"\n{'='.ToString().PadLeft(60, '=')}");
        Console.WriteLine("COMPOSITE PROGRAM TEST");
        Console.WriteLine($"{'='.ToString().PadLeft(60, '=')}");

        string program = @"
            MOV A1, 10
            ADD 5
            MUL 2
            MOV A2, FF
            SUB 3
        ";

        var programResult = coder.Compile(program);
        if (!coder.ErrorCharIndex.HasValue)
        {
            PrintDetailedResult(programResult, program);
        }
    }

    static void PrintResult(List<byte> bytes, string originalInput)
    {
        Console.WriteLine($"\nBytes count: {bytes.Count}");
        Console.WriteLine("\nHex representation:");
        Console.WriteLine(string.Join(" ", bytes.Select(b => b.ToString("X2"))));

        Console.WriteLine("\nBinary representation:");
        for (int i = 0; i < bytes.Count; i++)
        {
            Console.WriteLine($"Byte {i}: {Convert.ToString(bytes[i], 2).PadLeft(8, '0')} (0x{bytes[i]:X2}, {bytes[i],3})");
        }

        Console.WriteLine("\nDecoded:");
        DecodeBytes(bytes, originalInput);
    }

    static void PrintDetailedResult(List<byte> bytes, string program)
    {
        Console.WriteLine("\nProgram bytes:");
        Console.WriteLine($"Total bytes: {bytes.Count}");

        Console.WriteLine("\nMemory dump:");
        for (int i = 0; i < bytes.Count; i++)
        {
            if (i % 8 == 0) Console.Write($"\n{i:X4}: ");
            Console.Write($"{bytes[i]:X2} ");
        }
        Console.WriteLine();

        Console.WriteLine("\nBinary dump:");
        for (int i = 0; i < bytes.Count; i++)
        {
            Console.WriteLine($"{i:X4}: {Convert.ToString(bytes[i], 2).PadLeft(8, '0')} | 0x{bytes[i]:X2} | {bytes[i],3}");
        }
    }

    static void DecodeBytes(List<byte> bytes, string originalInput)
    {
        int i = 0;
        bool isMovCommand = originalInput.Contains("MOV A");

        while (i < bytes.Count)
        {
            byte currentByte = bytes[i];

            if (isMovCommand && i == 0)
            {
                // MOV команда - 8 бит код + 8 бит данные
                Console.WriteLine($"  Command: MOV (8-bit opcode: {Convert.ToString(currentByte, 2).PadLeft(8, '0')})");
                if (i + 1 < bytes.Count)
                {
                    Console.WriteLine($"  Data: {bytes[i + 1]} (0x{bytes[i + 1]:X2}, binary: {Convert.ToString(bytes[i + 1], 2).PadLeft(8, '0')})");
                    i += 2;
                }
                else
                {
                    i++;
                }
            }
            else
            {
                // Обычная команда - 5 бит код + 3 бит данные
                byte opcode = (byte)(currentByte >> 3);
                byte data = (byte)(currentByte & 0x07);

                string command = GetCommandName(opcode);
                Console.WriteLine($"  Command: {command} (5-bit opcode: {Convert.ToString(opcode, 2).PadLeft(5, '0')})");
                Console.WriteLine($"  Data: {data} (3-bit value: {Convert.ToString(data, 2).PadLeft(3, '0')})");
                i++;
            }
        }
    }

    static string GetCommandName(byte opcode)
    {
        var commands = new Dictionary<byte, string>
        {
            [0b00100] = "ADD",
            [0b00101] = "ADC",
            [0b00110] = "SUB",
            [0b00111] = "SBB",
            [0b01000] = "MUL",
            [0b01001] = "DIV",
            [0b01010] = "MOD",
            [0b01011] = "ABS",
            [0b01100] = "AND",
            [0b01101] = "OR",
            [0b01110] = "XOR",
            [0b01111] = "NOT",
            [0b10000] = "JMP",
            [0b10001] = "JB",
            [0b10010] = "JNZ",
            [0b10101] = "CMPS/MOVS"
        };

        return commands.TryGetValue(opcode, out string? name) ? name : $"UNKNOWN({opcode})";
    }
}