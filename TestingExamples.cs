using System;

public class Example
{
	public static string Big()
	{
		return """
; Тестовая программа для структуры 4
; Используются все команды из таблицы B.4
; ----------------------------------------

MOV A1, 10         ; A1, = 10
MOV A2, 11         ; A2, = 11
MOV A3, 12         ; A3, = 12

; Инициализация памяти значениями
MOVS             ; [12] = [10]

MOV A1, 13
MOV A2, 14
MOV A3, 15
MOVS             ; [15] = [13]

MOV A1, 20         ; запишем значение 5 в память[20]
MOV A3, 20
MOV A2, 5
MOVS

MOV A1, 21
MOV A2, 6
MOV A3, 21
MOVS             ; память[21] = 6

; Примеры арифметических операций
MOV A1, 10
MOV A2, 11
MOV A3, 12
ADD 0            ; память[12] = память[10] + память[11]

MOV A3, 13
ADC 0            ; память[13] = память[10] + память[11] + флаг переноса

MOV A3, 14
SUB 0            ; память[14] = память[10] - память[11]

MOV A3, 15
SBB 0            ; память[15] = память[10] - память[11] - флаг переноса

MOV A3, 16
MUL 0            ; память[16] = память[10] * память[11]

MOV A3, 17
DIV 0            ; память[17] = память[10] / память[11]

; Побитовые операции
MOV A3, 18
AND 0            ; память[18] = память[10] & память[11]

MOV A3, 19
OR 0             ; память[19] = память[10] | память[11]

MOV A3, 22
XOR 0            ; память[22] = память[10] ^ память[11]

; Побитовая инверсия
MOV A1, 10
MOV A3, 23
NOT 0            ; память[23] = ~память[10]

; Модуль (ABS)
MOV A1, 24
MOV A3, 25
ABS 0            ; память[25] = |память[24]|

; Условные переходы
MOV A1, 50
JMP 0            ; Безусловный переход на адрес [50]

MOV A1, 51
JNZ 0             ; Переход по нулю

MOV A1, 52
JNZ 0            ; Переход по не-нулю

; Сравнение без записи результата
MOV A1, 10
MOV A2, 11
CMPS             ; Устанавливает флаги, не меняя память

; Дополнительные случаи
MOV A1, 30
MOV A2, 31
MOV A3, 32

MOV A1, 33       # адрес для теста MOVS
MOV A2, 55
MOVS             # память[32] = память[30]

MOV A1, 34
MOV A2, 56
MOVS             # ещё один тест MOVS

MOV A1, 35
MOV A2, 36
MOV A3, 37
CMPS             // сравнение без записи

MOV A1, 38
MOV A2, 39
ADD 0

MOV A1, 40
MOV A2, 41
SUB 0

MOV A1, 42
MOV A2, 43
ADC 0

MOV A1, 44
MOV A2, 45
SBB 0

MOV A1, 46
MOV A2, 47
MUL 0

MOV A1, 48
MOV A2, 49
DIV 0

MOV A1, 50
MOV A2, 51
AND 0

MOV A1, 52
MOV A2, 53
OR 0

MOV A1, 54
MOV A2, 55
XOR 0

MOV A1, 56
MOV A3, 57
NOT 0

MOV A1, 58
MOV A3, 59
ABS 0

MOV A1, 60
MOV A2, 61
CMPS

MOV A1, 62
MOV A2, 63
ADD 0

MOV A1, 64
MOV A2, 65
SUB 0

MOV A1, 66
MOV A2, 67
ADC 0

MOV A1, 68
MOV A2, 69
SBB 0

MOV A1, 70
MOV A2, 71
MUL 0

MOV A1, 72
MOV A2, 73
DIV 0

MOV A1, 74
MOV A2, 75
AND 0

MOV A1, 76
MOV A2, 77
OR 0

MOV A1, 78
MOV A2, 79
XOR 0

MOV A1, 80
MOV A3, 81
NOT 0

MOV A1, 82
MOV A3, 83
ABS 0

; Конец программы
MOV A1, 0          ; Переход в начало
JMP 0            ; Бесконечный цикл

// --- Конец тестовой программы ---
; Тестовая программа для структуры 4
; Используются все команды из таблицы B.4
; ----------------------------------------

MOV A1, 10         ; A1, = 10
MOV A2, 11         ; A2, = 11
MOV A3, 12         ; A3, = 12

; Инициализация памяти значениями
MOVS             ; [12] = [10]

MOV A1, 13
MOV A2, 14
MOV A3, 15
MOVS             ; [15] = [13]

MOV A1, 20         ; запишем значение 5 в память[20]
MOV A3, 20
MOV A2, 5
MOVS

MOV A1, 21
MOV A2, 6
MOV A3, 21
MOVS             ; память[21] = 6

; Примеры арифметических операций
MOV A1, 10
MOV A2, 11
MOV A3, 12
ADD 0            ; память[12] = память[10] + память[11]

MOV A3, 13
ADC 0            ; память[13] = память[10] + память[11] + флаг переноса

MOV A3, 14
SUB 0            ; память[14] = память[10] - память[11]

MOV A3, 15
SBB 0            ; память[15] = память[10] - память[11] - флаг переноса

MOV A3, 16
MUL 0            ; память[16] = память[10] * память[11]

MOV A3, 17
DIV 0            ; память[17] = память[10] / память[11]

; Побитовые операции
MOV A3, 18
AND 0            ; память[18] = память[10] & память[11]

MOV A3, 19
OR 0             ; память[19] = память[10] | память[11]

MOV A3, 22
XOR 0            ; память[22] = память[10] ^ память[11]

; Побитовая инверсия
MOV A1, 10
MOV A3, 23
NOT 0            ; память[23] = ~память[10]

; Модуль (ABS)
MOV A1, 24
MOV A3, 25
ABS 0            ; память[25] = |память[24]|

; Условные переходы
MOV A1, 50
JMP 0            ; Безусловный переход на адрес [50]

MOV A1, 51
JNZ 0             ; Переход по нулю

MOV A1, 52
JNZ 0            ; Переход по не-нулю

; Сравнение без записи результата
MOV A1, 10
MOV A2, 11
CMPS             ; Устанавливает флаги, не меняя память

; Дополнительные случаи
MOV A1, 30
MOV A2, 31
MOV A3, 32

MOV A1, 33       # адрес для теста MOVS
MOV A2, 55
MOVS             # память[32] = память[30]

MOV A1, 34
MOV A2, 56
MOVS             # ещё один тест MOVS

MOV A1, 35
MOV A2, 36
MOV A3, 37
CMPS             // сравнение без записи

MOV A1, 38
MOV A2, 39
ADD 0

MOV A1, 40
MOV A2, 41
SUB 0

MOV A1, 42
MOV A2, 43
ADC 0

MOV A1, 44
MOV A2, 45
SBB 0

MOV A1, 46
MOV A2, 47
MUL 0

MOV A1, 48
MOV A2, 49
DIV 0

MOV A1, 50
MOV A2, 51
AND 0

MOV A1, 52
MOV A2, 53
OR 0

MOV A1, 54
MOV A2, 55
XOR 0

MOV A1, 56
MOV A3, 57
NOT 0

MOV A1, 58
MOV A3, 59
ABS 0

MOV A1, 60
MOV A2, 61
CMPS

MOV A1, 62
MOV A2, 63
ADD 0

MOV A1, 64
MOV A2, 65
SUB 0

MOV A1, 66
MOV A2, 67
ADC 0

MOV A1, 68
MOV A2, 69
SBB 0

MOV A1, 70
MOV A2, 71
MUL 0

MOV A1, 72
MOV A2, 73
DIV 0

MOV A1, 74
MOV A2, 75
AND 0

MOV A1, 76
MOV A2, 77
OR 0

MOV A1, 78
MOV A2, 79
XOR 0

MOV A1, 80
MOV A3, 81
NOT 0

MOV A1, 82
MOV A3, 83
ABS 0

; Конец программы
MOV A1, 0          ; Переход в начало
JMP 0            ; Бесконечный цикл

// --- Конец тестовой программы ---

""";
    }

	public static string Middle()
	{
		return """
    
; Тестовая программа для структуры 4
; Используются все команды из таблицы B.4
// ---------------------------------------

ABS 1 
ADC 2 ;23
ADD 3 
AND 4 #tr
CMPS 
DIV 5 
JB 6 
JNZ 7 
JMP 0 
MOD 1 
MOV A1, 01 
MOV A2, FF 
MOV A3, 00 
MOVS 
MUL 2 
NOT 3 
OR 4 
SBB 5 
SUB 6
XOR 7 

; Инициализация памяти значениями

// ---------------------------------------
""";
    }

	public static string Small()
	{
		return """
    
; Тестовая программа для структуры 4
; Используются все команды из таблицы B.4
// ---------------------------------------

MOV A1, AD         ; A1, = 10
MOV A2, ad         ; A2, = 11
MOV A3, ad         ; A3, = 12
adc 2
abs 3
; Инициализация памяти значениями
    
""";
    }

	public static void test()
	{
        //============================================ Lexer
        Console.WriteLine("<<<===Lexer===>>>");

        var lexer = new Lexer(enableDebug: false);
        string inputCode = Example.Big();
        string outputLexemes = lexer.Parse(inputCode);

        Console.WriteLine($"\nResult lexemes: {outputLexemes}");

        if (lexer.ErrorCharIndex is not null)
        {
            Console.WriteLine($"Ошибка на позиции {lexer.ErrorCharIndex}, лексема #{lexer.ErrorLexemeIndex}, переход из состояния {lexer.ErrorTransitionCode}");
        }

        //============================================ Syntax
        Console.WriteLine("<<<===Syntax===>>>");

        var syntax = new Syntax(enableDebug: false);
        syntax.Parse(outputLexemes);

        if (syntax.ErrorLexemeIndex != null)
        {
            Console.WriteLine($"Ошибка в символе #{syntax.ErrorLexemeIndex}: /{syntax.ErrorCause}\\");
            Console.WriteLine($"Содержимое стека: {syntax.ErrorStack}");
        }
        else
        {
            Console.WriteLine("Анализ завершён успешно");
        }

        //============================================ Coder
        Console.WriteLine("<<<===Coder===>>>");

        var coder = new Coder(enableDebug: false);
        List<byte> bytes = coder.Compile(inputCode);

        if (coder.ErrorCharIndex != null)
        {
            Console.WriteLine($"Ошибка в символе #{coder.ErrorCharIndex}: /{coder.ErrorSymbol}\\");
            Console.WriteLine($"Причина (код): {coder.ErrorReason}");
        }
        else
        {
            Console.WriteLine("Анализ завершён успешно");
        }

        Console.WriteLine("Вывод содержимого стека");
        for (int i = 0; i < bytes.Count; i++)
        {
            Console.WriteLine($"{i:X4}: {Convert.ToString(bytes[i], 2).PadLeft(8, '0')} | 0x{bytes[i]:X2} | {bytes[i],3}");
        }

        //============================================ Decoder
        Console.WriteLine("<<<===Decoder===>>>");
        var decoder = new Decoder(false);
        List<string> readable = decoder.Decode(bytes);

        for (int i = 0; i < readable.Count; i++)
        {
            Console.WriteLine(readable[i]);
        }

    }
}