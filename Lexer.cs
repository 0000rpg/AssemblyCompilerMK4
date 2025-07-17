using System;
using System.Collections.Generic;
using System.Text;

public class Lexer
{
    private readonly Dictionary<int, Dictionary<char, int>> transitions = new();
    private readonly Dictionary<int, char> finalOutputs = new();
    private readonly Dictionary<int, Dictionary<char, char>> finalOutputTable = new();
    private readonly HashSet<char> hexChars = new("0123456789ABCDEF");
    private readonly bool debug;

    // Ошибки
    public int? ErrorCharIndex { get; private set; }
    public int? ErrorLexemeIndex { get; private set; }
    public int? ErrorTransitionCode { get; private set; }

    public Lexer(bool enableDebug = false)
    {
        debug = enableDebug;
        InitializeTransitions();
        InitializeFinalStates();
    }

    private void InitializeTransitions()
    {
        // Таблица переходов по состояниям
        var rawTransitions = new Dictionary<int, Dictionary<char, int>>
        {
            [4] = new() { ['D'] = -1 },
            [6] = new() { ['P'] = 7 },
            [7] = new() { ['S'] = -1 },
            [8] = new() { ['Z'] = -1 },
            [10] = new() { ['V'] = -1 },
            [11] = new() { ['M'] = 12, ['N'] = 8, ['B'] = -1 },
            [12] = new() { ['P'] = -1 },
            [13] = new() { ['O'] = 14, ['U'] = 18 },
            [14] = new() { ['D'] = -1, ['V'] = 15 },
            [15] = new() { [' '] = 16, ['S'] = -1 },
            [16] = new() { ['A'] = 17 },
            [17] = new() { ['1'] = 27, ['2'] = 27, ['3'] = 27 },
            [27] = new() { [','] = -1 },
            [18] = new() { ['L'] = -1 },
            [19] = new() { ['O'] = 20 },
            [20] = new() { ['T'] = -1 },
            [21] = new() { ['R'] = -1 },
            [22] = new() { ['B'] = 23, ['U'] = 24 },
            [23] = new() { ['B'] = -1 },
            [24] = new() { ['B'] = -1 },
            [25] = new() { ['O'] = 26 },
            [26] = new() { ['R'] = -1 },

            // Дополнение в виде цифр:
            [28] = new()
            {
                ['0'] = 28,
                ['1'] = 28,
                ['2'] = 28,
                ['3'] = 28,
                ['4'] = 28,
                ['5'] = 28,
                ['6'] = 28,
                ['7'] = 28,
                ['8'] = 28,
                ['9'] = 28,
                ['A'] = 28,
                ['B'] = 28,
                ['C'] = 28,
                ['D'] = 28,
                ['E'] = 28,
                ['F'] = 28,
                [' '] = -1
            },

            [0] = new()
            {
                ['A'] = 1,
                ['C'] = 5,
                ['D'] = 9,
                ['J'] = 11,
                ['M'] = 13,
                ['N'] = 19,
                ['O'] = 21,
                ['S'] = 22,
                ['X'] = 25,
                ['0'] = 28,
                ['1'] = 28,
                ['2'] = 28,
                ['3'] = 28,
                ['4'] = 28,
                ['5'] = 28,
                ['6'] = 28,
                ['7'] = 28,
                ['8'] = 28,
                ['9'] = 28,
                ['B'] = 28,
                ['E'] = 28,
                ['F'] = 28,
                [' '] = 0
            },

            [1] = new()
            {
                ['B'] = 2,
                ['D'] = 3,
                ['N'] = 4,
                ['0'] = 28,
                ['1'] = 28,
                ['2'] = 28,
                ['3'] = 28,
                ['4'] = 28,
                ['5'] = 28,
                ['6'] = 28,
                ['7'] = 28,
                ['8'] = 28,
                ['9'] = 28,
                ['A'] = 28,
                ['C'] = 28,
                ['E'] = 28,
                ['F'] = 28,
                [' '] = -1
            },

            [2] = new()
            {
                ['S'] = -1,
                ['0'] = 28,
                ['1'] = 28,
                ['2'] = 28,
                ['3'] = 28,
                ['4'] = 28,
                ['5'] = 28,
                ['6'] = 28,
                ['7'] = 28,
                ['8'] = 28,
                ['9'] = 28,
                ['A'] = 28,
                ['B'] = 28,
                ['C'] = 28,
                ['D'] = 28,
                ['E'] = 28,
                ['F'] = 28,
                [' '] = -1
            },

            [3] = new()
            {
                ['C'] = -2,
                ['D'] = -2,  // -2 для условного перехода
                ['0'] = 28,
                ['1'] = 28,
                ['2'] = 28,
                ['3'] = 28,
                ['4'] = 28,
                ['5'] = 28,
                ['6'] = 28,
                ['7'] = 28,
                ['8'] = 28,
                ['9'] = 28,
                ['A'] = 28,
                ['B'] = 28,
                ['E'] = 28,
                ['F'] = 28,
                [' '] = -1
            },

            [5] = new()
            {
                ['M'] = 6,
                ['0'] = 28,
                ['1'] = 28,
                ['2'] = 28,
                ['3'] = 28,
                ['4'] = 28,
                ['5'] = 28,
                ['6'] = 28,
                ['7'] = 28,
                ['8'] = 28,
                ['9'] = 28,
                ['A'] = 28,
                ['B'] = 28,
                ['C'] = 28,
                ['D'] = 28,
                ['E'] = 28,
                ['F'] = 28,
                [' '] = -1
            },

            [9] = new()
            {
                ['I'] = 10,
                ['0'] = 28,
                ['1'] = 28,
                ['2'] = 28,
                ['3'] = 28,
                ['4'] = 28,
                ['5'] = 28,
                ['6'] = 28,
                ['7'] = 28,
                ['8'] = 28,
                ['9'] = 28,
                ['A'] = 28,
                ['B'] = 28,
                ['C'] = 28,
                ['D'] = 28,
                ['E'] = 28,
                ['F'] = 28,
                [' '] = -1
            }
        };

        // Заполняем transitions
        foreach (var kvp in rawTransitions)
        {
            transitions[kvp.Key] = new Dictionary<char, int>(kvp.Value);
        }
    }
    private void InitializeFinalStates()
    {
        void AddFinal(int state, char triggerChar, char output)
        {
            if (!finalOutputTable.ContainsKey(state))
                finalOutputTable[state] = new Dictionary<char, char>();
            finalOutputTable[state][triggerChar] = output;
        }

        // MOV A1-3,
        AddFinal(27, ',', 'm');

        // MOVS и CMPS
        AddFinal(15, 'S', 's');
        AddFinal(7, 'S', 's');

        // Команды общего вида
        AddFinal(2, 'S', 'o');
        AddFinal(3, 'C', 'o');
        AddFinal(3, 'D', 'o');
        AddFinal(4, 'D', 'o');
        AddFinal(8, 'Z', 'o');
        AddFinal(10, 'V', 'o');
        AddFinal(11, 'B', 'o');
        AddFinal(12, 'P', 'o');
        AddFinal(14, 'D', 'o');
        AddFinal(18, 'L', 'o');
        AddFinal(20, 'T', 'o');
        AddFinal(21, 'R', 'o');
        AddFinal(23, 'B', 'o');
        AddFinal(24, 'B', 'o');
        AddFinal(26, 'R', 'o');

        // Числа (hex)
        foreach (char ch in "0123456789ABCDEF ")
        {
            AddFinal(28, ch, 'n');
            AddFinal(1, ch, 'n');
            AddFinal(2, ch, 'n');
            AddFinal(3, ch, 'n');
            AddFinal(5, ch, 'n');
            AddFinal(9, ch, 'n');
        }

        /*/ Особое состояние с 'C' или 'D' -> i
        AddFinal(3, 'C', 'i');
        AddFinal(3, 'D', 'i');*/

        // Начальное состояние может вернуть пустой 'e' по пробелу ERROR
        AddFinal(0, ' ', 'e');
    }


    public string Parse(string input)
    {
        var output = new StringBuilder();
        int state = 0, lexemeIndex = 0;
        char? lastOutput = null;

        for (int i = 0; i < input.Length; i++)
        {
            char c = char.ToUpperInvariant(input[i]);

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

            if (c == '\n' || c == '\r' || c == '\t')
                c = ' ';

            if (debug) Console.WriteLine($"[DEBUG] Char: '{c}', State: {state}, LastOut: {lastOutput}");

            if (transitions.TryGetValue(state, out var trans) && trans.TryGetValue(c, out int nextState))
            {
                if (nextState == -2)
                {
                    char outChar = (lastOutput == 'n' || lastOutput == 's') ? 'o' : 'n';
                    output.Append(outChar);
                    if (debug) Console.WriteLine($"[DEBUG] → -2 emit '{outChar}'");
                    lastOutput = outChar;
                    state = 0;
                    lexemeIndex++;
                    continue;
                }
                else if (nextState == -1)
                {
                    if (finalOutputTable.TryGetValue(state, out var charMap) && charMap.TryGetValue(c, out char outChar))
                    {
                        output.Append(outChar);
                        if (debug) Console.WriteLine($"[DEBUG] → FINAL {state} on '{c}' emit '{outChar}'");
                        lastOutput = outChar;
                        lexemeIndex++;
                    }
                    else
                    {
                        ErrorCharIndex = i;
                        ErrorLexemeIndex = lexemeIndex;
                        ErrorTransitionCode = -1;
                        if (debug) Console.WriteLine($"[ERROR] No final output for state {state} on '{c}'");
                        break;
                    }
                    state = 0;
                }
                else
                {
                    state = nextState;
                }
            }
            else
            {
                if (state == 0 && hexChars.Contains(c))
                {
                    output.Append('n');
                    lastOutput = 'n';
                    if (debug) Console.WriteLine("[DEBUG] Emit 'n' from HEX");
                    lexemeIndex++;
                    continue;
                }

                ErrorCharIndex = i;
                ErrorLexemeIndex = lexemeIndex;
                ErrorTransitionCode = transitions.ContainsKey(state) ? -3 : -4;
                if (debug) Console.WriteLine($"[ERROR] Invalid transition from state {state} on '{c}'");
                break;
            }
        }

        return output.ToString();
    }
}