using System;
using System.Collections.Generic;
using System.Linq;

class WorkerProgram
{
    public static void Main()
    {
        string? modeLine = Console.ReadLine();
        if (modeLine == null)
        {
            Console.WriteLine("ERROR\n100\nNo input data");
            return;
        }

        int mode = modeLine.Trim() == "1" ? 1 : 0;

        // Считываем всё остальное — построчно до конца входного потока
        var lines = new List<string>();
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            lines.Add(line + "\n");
        }

        if (lines.Count == 0)
        {
            Console.WriteLine("ERROR\n100\nNo input content");
            return;
        }

        string fullInput = string.Join(" ", lines).Trim();

        try
        {
            if (mode == 1)
            {
                // === Encoding Mode ===
                var lexer = new Lexer();
                var lexemes = lexer.Parse(fullInput + " ");
                if (lexer.ErrorCharIndex is not null)
                {
                    Console.WriteLine($"ERROR\n101\n{string.Join("", lexer.ErrorCharIndex)}");
                    return;
                }

                var parser = new Syntax();
                parser.Parse(lexemes);
                if (parser.ErrorLexemeIndex is not null)
                {
                    Console.WriteLine($"ERROR\n102\n{string.Join("", parser.ErrorLexemeIndex)}");
                    return;
                }

                var coder = new Coder();
                var bytes = coder.Compile(fullInput);

                if (coder.ErrorReason != null)
                {
                    string partial = string.Join(" ", bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
                    Console.WriteLine($"ERROR\n103\n{partial}");
                    return;
                }

                string binaryOutput = string.Join(" ", bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
                Console.WriteLine("OK\n" + binaryOutput);
            }
            else
            {
                // === Decoding Mode ===
                string[] bitStrings = fullInput.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                var byteList = new List<byte>();

                foreach (var b in bitStrings)
                {
                    if (b.Length != 8 || !b.All(c => c == '0' || c == '1'))
                    {
                        Console.WriteLine("ERROR\n201\n");
                        return;
                    }

                    byteList.Add(Convert.ToByte(b, 2));
                }

                var decoder = new Decoder();
                var decoded = decoder.Decode(byteList);
                Console.WriteLine("OK\n" + string.Join("\n", decoded));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR\n999\n" + ex.Message);
        }
    }
}
