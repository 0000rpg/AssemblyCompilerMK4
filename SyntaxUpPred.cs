using System;
using System.Collections.Generic;
using System.Text;

public class Syntax
{
    private readonly Dictionary<char, Dictionary<char, char>> transitions = new();
    private readonly Dictionary<string, char> reductions = new();
    private readonly bool debug;

    public int? ErrorLexemeIndex { get; private set; }
    public string? ErrorStack { get; private set; }
    public string? ErrorCause { get; private set; }

    public Syntax(bool enableDebug = false)
    {
        debug = enableDebug;
        InitializeTransitions();
        InitializeReductions();
    }

    private void InitializeTransitions()
    {
        transitions['P'] = new() { ['$'] = '^' };
        transitions['B'] = new() { ['s'] = '=', ['n'] = '=', ['$'] = '>' };
        transitions['m'] = new() { ['n'] = '=' };
        transitions['o'] = new() { ['n'] = '=' };
        transitions['s'] = new() { ['B'] = '=', ['m'] = '<', ['o'] = '<', ['s'] = '<', ['$'] = '>' };
        transitions['n'] = new() { ['B'] = '=', ['m'] = '<', ['o'] = '<', ['s'] = '<', ['$'] = '>' };
        transitions['$'] = new() { ['P'] = '<', ['B'] = '<', ['m'] = '<', ['o'] = '<', ['s'] = '<', ['$'] = '^' };
    }

    private void InitializeReductions()
    {
        reductions["B"] = 'P';
        reductions["mnB"] = 'B';
        reductions["onB"] = 'B';
        reductions["sB"] = 'B';
        reductions["mn"] = 'B';
        reductions["on"] = 'B';
        reductions["s"] = 'B';
    }

    public bool Parse(string input)
    {
        input += "$";
        var stack = new List<char> { '$' };
        int index = 0;

        while (index < input.Length)
        {
            char top = TopTerminal(stack);
            char next = input[index];

            if (debug)
                Console.WriteLine($"[DEBUG] Stack: {string.Join("", stack)}, Input: {input[index..]}");

            if (transitions.TryGetValue(top, out var row) && row.TryGetValue(next, out char action))
            {
                if (debug)
                    Console.WriteLine($"[DEBUG] Action: {action} for top '{top}' and next '{next}'");

                switch (action)
                {
                    case '<':
                    case '=':
                        stack.Add(next);
                        index++;
                        break;
                    case '>':
                        if (!TryReduce(stack))
                        {
                            SetError(index, stack, "No reduction rule matched");
                            return false;
                        }
                        break;
                    case '^':
                        return stack.Count == 2 && stack[1] == 'P'; // Only if stack is [$, P]
                    default:
                        SetError(index, stack, $"Unknown action '{action}'");
                        return false;
                }
            }
            else
            {
                SetError(index, stack, $"No transition for top '{top}' and next '{next}'");
                return false;
            }
        }

        SetError(index, stack, "Unexpected end of input");
        return false;
    }

    private char TopTerminal(List<char> stack)
    {
        for (int i = stack.Count - 1; i >= 0; i--)
        {
            if (!char.IsUpper(stack[i])) return stack[i];
        }
        return '$';
    }

    private bool TryReduce(List<char> stack)
    {
        for (int len = Math.Min(3, stack.Count - 1); len >= 1; len--)
        {
            string pattern = string.Join("", stack.GetRange(stack.Count - len, len));
            if (reductions.TryGetValue(pattern, out char result))
            {
                if (debug)
                    Console.WriteLine($"[DEBUG] Reducing {pattern} -> {result}");

                stack.RemoveRange(stack.Count - len, len);
                stack.Add(result);
                return true;
            }
        }
        return false;
    }

    private void SetError(int index, List<char> stack, string cause)
    {
        ErrorLexemeIndex = index;
        ErrorStack = string.Join("", stack);
        ErrorCause = cause;

        if (debug)
        {
            Console.WriteLine($"[ERROR] Lexeme #{index}, Stack: {ErrorStack}, Cause: {cause}");
        }
    }
}