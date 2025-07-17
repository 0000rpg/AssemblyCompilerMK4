using System;
using System.Collections.Generic;
using System.Text;

public class Coder
{
    private readonly Dictionary<int, Dictionary<char, int>> transitions = new();
    private readonly Dictionary<(int state, char symbol), (string mnemonic, string code)> finalStates = new();
    private readonly bool debug;

    private string? currentMnemonic = null;
    private bool isMovCommand = false;
    private bool expectingNumber = false; // Флаг ожидания числа после запятой

    public int? ErrorCharIndex { get; private set; }
    public string? ErrorReason { get; private set; }
    public string? ErrorSymbol { get; private set; }

    public Coder(bool enableDebug = false)
    {
        debug = enableDebug;
        InitializeTransitions();
        InitializeFinalStates();
    }

    public List<byte> Compile(string input)
    {
        var result = new List<byte>();
        input = input.ToUpperInvariant();

        int state = 0;
        StringBuilder buffer = new();
        StringBuilder numberBuffer = new();

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            // Обработка комментариев
            if (c == ';' || c == '#')
            {
                while (i < input.Length && input[i] != '\n') i++;
                state = 0;
                continue;
            }
            if (c == '/' && i + 1 < input.Length && input[i + 1] == '/')
            {
                i += 2;
                while (i < input.Length && input[i] != '\n') i++;
                state = 0;
                continue;
            }

            if (char.IsWhiteSpace(c)) c = ' ';

            if (debug) Console.WriteLine($"[DEBUG] Char: {c}, State: {state}");

            // Специальная обработка для состояния 28 (чтение числа)
            if (state == 28)
            {
                if (IsHexDigit(c))
                {
                    numberBuffer.Append(c);
                    continue;
                }
                else
                {
                    // Число закончилось, обрабатываем его
                    ProcessNumber(numberBuffer.ToString(), result, i, c);
                    numberBuffer.Clear();
                    state = 0;
                    expectingNumber = false;

                    // Если текущий символ не пробел, обрабатываем его заново
                    if (c != ' ')
                    {
                        i--;
                    }
                    continue;
                }
            }

            // Если ожидаем число после запятой и пробела
            if (expectingNumber && c == ' ')
            {
                continue; // Пропускаем пробелы
            }
            else if (expectingNumber && IsHexDigit(c))
            {
                state = 28;
                numberBuffer.Clear();
                numberBuffer.Append(c);
                continue;
            }
            else if (expectingNumber)
            {
                SetError(i, c, "Expected number after comma");
                return result;
            }

            buffer.Append(c);

            if (transitions.TryGetValue(state, out var row) && row.TryGetValue(c, out int next))
            {
                if (next == -1 || next == -2)
                {
                    if (!finalStates.TryGetValue((state, c), out var final))
                    {
                        SetError(i, c, "Final state with no matching output");
                        return result;
                    }

                    // Определяем тип команды
                    isMovCommand = final.mnemonic.StartsWith("MOV A");
                    currentMnemonic = final.mnemonic;

                    // Проверяем, ожидается ли число после этой команды
                    if (c == ',' && (state == 27 || state == 29 || state == 30))
                    {
                        expectingNumber = true;
                    }

                    if (isMovCommand)
                    {
                        // MOV команды - 8 бит
                        byte opcode = Convert.ToByte(final.code, 2);
                        result.Add(opcode);
                    }
                    else
                    {
                        // Остальные команды - 5 бит, но пока добавляем только код
                        byte opcode = Convert.ToByte(final.code, 2);
                        result.Add(opcode);
                    }

                    if (debug)
                    {
                        Console.WriteLine($"[DEBUG] Byte {result.Count - 1}: {Convert.ToString(result[^1], 2).PadLeft(8, '0')}  ({final.mnemonic})");
                    }

                    buffer.Clear();
                    state = 0;
                    continue;
                }
                else if (next == 28)
                {
                    // Начинаем читать число
                    numberBuffer.Clear();
                    if (IsHexDigit(c))
                    {
                        numberBuffer.Append(c);
                    }
                }

                state = next;
            }
            else
            {
                SetError(i, c, $"No transition from state {state} by '{c}'");
                return result;
            }
        }

        // Обработка числа в конце строки
        if (numberBuffer.Length > 0)
        {
            ProcessNumber(numberBuffer.ToString(), result, input.Length - 1, ' ');
        }
        else if (expectingNumber)
        {
            SetError(input.Length - 1, ' ', "Expected number at end of input");
        }

        return result;
    }

    private void ProcessNumber(string hexStr, List<byte> result, int index, char nextChar)
    {
        if (string.IsNullOrEmpty(hexStr))
        {
            SetError(index, nextChar, "Empty number");
            return;
        }

        if (!byte.TryParse(hexStr, System.Globalization.NumberStyles.HexNumber, null, out byte value))
        {
            SetError(index, nextChar, "Invalid hex number");
            return;
        }

        if (isMovCommand)
        {
            // Для MOV команд число идет отдельным байтом (8 бит)
            result.Add(value);
            if (debug)
            {
                Console.WriteLine($"[DEBUG] Byte {result.Count - 1}: {Convert.ToString(value, 2).PadLeft(8, '0')}  (8-bit constant)");
            }
        }
        else if (currentMnemonic == "CMPS" || currentMnemonic == "MOVS")
        {
            // CMPS и MOVS уже содержат полный байт (5 бит + 000)
            if (debug)
            {
                Console.WriteLine($"[DEBUG] CMPS/MOVS command, already complete");
            }
        }
        else
        {
            // Обычные команды - проверяем, что число помещается в 3 бита
            if (value > 7)
            {
                SetError(index, nextChar, $"ERROR ({value} requires more than 3 bits)");
                return;
            }

            // Объединяем 5-битный код команды с 3-битным числом
            if (result.Count > 0)
            {
                byte lastByte = result[^1];
                result[^1] = (byte)((lastByte << 3) | (value & 0x07));
                if (debug)
                {
                    Console.WriteLine($"[DEBUG] Updated byte {result.Count - 1}: {Convert.ToString(result[^1], 2).PadLeft(8, '0')}  (5-bit opcode + 3-bit value)");
                }
            }
        }

        // Сбрасываем флаги после обработки числа
        currentMnemonic = null;
        isMovCommand = false;
    }

    private bool IsHexDigit(char c) =>
        (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F');

    private void SetError(int index, char symbol, string reason)
    {
        ErrorCharIndex = index;
        ErrorSymbol = symbol.ToString();
        ErrorReason = reason;
        if (debug)
            Console.WriteLine($"[ERROR] Index: {index}, Char: {symbol}, Reason: {reason}");
    }

    private void InitializeTransitions()
    {
        var rawTransitions = new Dictionary<int, Dictionary<char, int>>
        {
            [0] = new() { ['1'] = 28, ['2'] = 28, ['3'] = 28, ['4'] = 28, ['5'] = 28, ['6'] = 28, ['7'] = 28, ['8'] = 28, ['9'] = 28, ['0'] = 28, ['A'] = 1, ['B'] = 28, ['C'] = 5, ['D'] = 9, ['E'] = 28, ['F'] = 28, [' '] = 0, ['J'] = 11, ['M'] = 13, ['N'] = 19, ['O'] = 21, ['S'] = 22, ['X'] = 25 },
            [1] = new() { ['1'] = 28, ['2'] = 28, ['3'] = 28, ['4'] = 28, ['5'] = 28, ['6'] = 28, ['7'] = 28, ['8'] = 28, ['9'] = 28, ['0'] = 28, ['A'] = 28, ['B'] = 2, ['C'] = 28, ['D'] = 3, ['E'] = 28, ['F'] = 28, [' '] = -1, ['N'] = 4 },
            [2] = new() { ['1'] = 28, ['2'] = 28, ['3'] = 28, ['4'] = 28, ['5'] = 28, ['6'] = 28, ['7'] = 28, ['8'] = 28, ['9'] = 28, ['0'] = 28, ['A'] = 28, ['B'] = 28, ['C'] = 28, ['D'] = 28, ['E'] = 28, ['F'] = 28, [' '] = -1, ['S'] = -1 },
            [3] = new() { ['1'] = 28, ['2'] = 28, ['3'] = 28, ['4'] = 28, ['5'] = 28, ['6'] = 28, ['7'] = 28, ['8'] = 28, ['9'] = 28, ['0'] = 28, ['A'] = 28, ['B'] = 28, ['C'] = -2, ['D'] = -2, ['E'] = 28, ['F'] = 28, [' '] = -1 },
            [4] = new() { ['D'] = -1 },
            [5] = new() { ['1'] = 28, ['2'] = 28, ['3'] = 28, ['4'] = 28, ['5'] = 28, ['6'] = 28, ['7'] = 28, ['8'] = 28, ['9'] = 28, ['0'] = 28, ['A'] = 28, ['B'] = 28, ['C'] = 28, ['D'] = 28, ['E'] = 28, ['F'] = 28, [' '] = -1, ['M'] = 6 },
            [6] = new() { ['P'] = 7 },
            [7] = new() { ['S'] = -1 },
            [8] = new() { ['Z'] = -1 },
            [9] = new() { ['I'] = 10, ['1'] = 28, ['2'] = 28, ['3'] = 28, ['4'] = 28, ['5'] = 28, ['6'] = 28, ['7'] = 28, ['8'] = 28, ['9'] = 28, ['0'] = 28, ['A'] = 28, ['B'] = 28, ['C'] = 28, ['D'] = 28, ['E'] = 28, ['F'] = 28, [' '] = -1 },
            [10] = new() { ['V'] = -1 },
            [11] = new() { ['B'] = -1, ['M'] = 12, ['N'] = 8 },
            [12] = new() { ['P'] = -1 },
            [13] = new() { ['O'] = 14, ['U'] = 18 },
            [14] = new() { ['D'] = -1, ['V'] = 15 },
            [15] = new() { [' '] = 16, ['S'] = -1 },
            [16] = new() { ['A'] = 17 },
            [17] = new() { ['1'] = 27, ['2'] = 29, ['3'] = 30 },
            [18] = new() { ['L'] = -1 },
            [19] = new() { ['O'] = 20 },
            [20] = new() { ['T'] = -1 },
            [21] = new() { ['R'] = -1 },
            [22] = new() { ['B'] = 23, ['U'] = 24 },
            [23] = new() { ['B'] = -1 },
            [24] = new() { ['B'] = -1 },
            [25] = new() { ['O'] = 26 },
            [26] = new() { ['R'] = -1 },
            [27] = new() { [','] = -1 },
            [28] = new() { ['1'] = 28, ['2'] = 28, ['3'] = 28, ['4'] = 28, ['5'] = 28, ['6'] = 28, ['7'] = 28, ['8'] = 28, ['9'] = 28, ['0'] = 28, ['A'] = 28, ['B'] = 28, ['C'] = 28, ['D'] = 28, ['E'] = 28, ['F'] = 28 },
            [29] = new() { [','] = -1 },
            [30] = new() { [','] = -1 },
        };

        foreach (var (state, map) in rawTransitions)
            transitions[state] = map;
    }

    private void InitializeFinalStates()
    {
        void Add(int state, char symbol, string name, string code) =>
            finalStates[(state, symbol)] = (name, code);

        // Обычные команды с 5-битными кодами
        Add(2, 'S', "ABS", "01011");
        Add(3, 'C', "ADC", "00101");
        Add(3, 'D', "ADD", "00100");
        Add(4, 'D', "AND", "01100");
        Add(7, 'S', "CMPS", "10111000");
        Add(10, 'V', "DIV", "01001");
        Add(11, 'B', "JB", "10001");
        Add(12, 'P', "JMP", "10000");
        Add(8, 'Z', "JNZ", "10010");
        Add(14, 'D', "MOD", "01010");
        Add(15, 'S', "MOVS", "10101000");
        Add(18, 'L', "MUL", "01000");
        Add(20, 'T', "NOT", "01111");
        Add(21, 'R', "OR", "01101");
        Add(23, 'B', "SBB", "00111");
        Add(24, 'B', "SUB", "00110");
        Add(26, 'R', "XOR", "01110");

        // MOV команды с 8-битными кодами
        Add(27, ',', "MOV A1", "00000000");
        Add(29, ',', "MOV A2", "00001000");
        Add(30, ',', "MOV A3", "00010000");
    }
}