public class Decoder
{
    private readonly bool debug;
    private readonly Dictionary<byte, string> fiveBitOpcodes = new();
    private readonly HashSet<byte> noArgOpcodes = new();
    private readonly Dictionary<byte, string> movOpcodes = new();

    public Decoder(bool enableDebug = false)
    {
        debug = enableDebug;
        Initialize();
    }

    public List<string> Decode(List<byte> machineCode)
    {
        var result = new List<string>();

        for (int i = 0; i < machineCode.Count; i++)
        {
            byte code = machineCode[i];

            if (debug)
                Console.Write($"[DEBUG] {i:X4}: {Convert.ToString(code, 2).PadLeft(8, '0')} | 0x{code:X2} | {code}\n");

            // Проверяем, является ли это MOV командой
            if (movOpcodes.TryGetValue(code, out string movReg))
            {
                if (i + 1 < machineCode.Count)
                {
                    byte value = machineCode[i + 1];
                    result.Add($"{movReg}, {value:X2}");
                    if (debug)
                        Console.WriteLine($"[DEBUG] {movReg}, {value:X2}");
                    i++; // Пропускаем следующий байт (значение)
                }
                else
                {
                    result.Add($"{movReg}, ??");
                    if (debug)
                        Console.WriteLine($"[DEBUG] {movReg}, ?? (missing byte)");
                }
                continue;
            }

            // MOVS, CMPS — 8-битные, но без аргументов
            if (code == 0xA8)
            {
                result.Add("MOVS");
                if (debug)
                    Console.WriteLine($"[DEBUG] MOVS");
                continue;
            }

            // Поиск по 5-битной маске
            byte opcode5 = (byte)(code >> 3);
            byte arg = (byte)(code & 0x07);

            if (fiveBitOpcodes.TryGetValue(opcode5, out string mnemonic))
            {
                if (noArgOpcodes.Contains(opcode5))
                {
                    result.Add(mnemonic);
                    if (debug)
                        Console.WriteLine($"[DEBUG] {mnemonic}");
                }
                else
                {
                    result.Add($"{mnemonic} {arg}");
                    if (debug)
                        Console.WriteLine($"[DEBUG] {mnemonic} {arg}");
                }
            }
            else
            {
                result.Add($"??? ({code:X2})");
                if (debug)
                    Console.WriteLine($"[DEBUG] ??? ({code:X2})");
            }
        }

        return result;
    }

    private void Initialize()
    {
        // MOV команды для разных регистров
        movOpcodes[0x00] = "MOV A1";
        movOpcodes[0x08] = "MOV A2";
        movOpcodes[0x10] = "MOV A3";

        // команды с аргументом (3 бита)
        fiveBitOpcodes[0b01011] = "ABS";
        fiveBitOpcodes[0b00101] = "ADC";
        fiveBitOpcodes[0b00100] = "ADD";
        fiveBitOpcodes[0b01100] = "AND";
        fiveBitOpcodes[0b01001] = "DIV";
        fiveBitOpcodes[0b10001] = "JB";
        fiveBitOpcodes[0b10010] = "JNZ";
        fiveBitOpcodes[0b10000] = "JMP";
        fiveBitOpcodes[0b01010] = "MOD";
        fiveBitOpcodes[0b01000] = "MUL";
        fiveBitOpcodes[0b01111] = "NOT";
        fiveBitOpcodes[0b01101] = "OR";
        fiveBitOpcodes[0b00111] = "SBB";
        fiveBitOpcodes[0b00110] = "SUB";
        fiveBitOpcodes[0b01110] = "XOR";

        // команды без аргумента
        fiveBitOpcodes[0b10111] = "CMPS";  // 10101xxx -> CMPS
        fiveBitOpcodes[0b10101] = "MOVS";  // 10111xxx -> MOVS

        noArgOpcodes.Add(0b10111); // CMPS без аргументов
        noArgOpcodes.Add(0b10101); // MOVS без аргументов
    }
}