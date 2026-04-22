using System;

namespace AluSimulator
{
    /// <summary>
    /// Symulator 16-bitowej jednostki arytmetyczno-logicznej (ALU).
    /// Operuje na liczbach w kodzie U2 (short: -32768..32767).
    /// Ustawia flagi: ZF (Zero), CF (Carry), SF (Sign), OF (Overflow).
    /// </summary>
    public class Alu
    {
        public const int BITS = 16;
        public const ushort MASK16 = 0xFFFF;

        // Flagi procesora
        public bool ZF { get; private set; } // Zero Flag    - wynik == 0
        public bool CF { get; private set; } // Carry Flag   - przeniesienie/pożyczka (bez znaku)
        public bool SF { get; private set; } // Sign Flag    - bit znaku wyniku (MSB)
        public bool OF { get; private set; } // Overflow Flag - przepełnienie w U2 (ze znakiem)

        public enum Operation
        {
            ADD, SUB, AND, OR, XOR, NOT, NEG,
            SHL, SHR, SAR, ROL, ROR,
            INC, DEC, CMP
        }

        /// <summary>
        /// Wykonuje operację na operandach. Zwraca 16-bitowy wynik (ushort).
        /// Dla operacji jednoargumentowych operand B jest ignorowany.
        /// Dla przesunięć/rotacji operand B to licznik (0..15).
        /// </summary>
        public ushort Execute(Operation op, ushort a, ushort b, out string description)
        {
            ResetFlags();
            ushort result = 0;
            description = "";

            switch (op)
            {
                case Operation.ADD:
                    result = Add(a, b);
                    description = $"ADD: {a} + {b}";
                    break;
                case Operation.SUB:
                    result = Sub(a, b);
                    description = $"SUB: {a} - {b}";
                    break;
                case Operation.CMP:
                    // CMP = SUB, ale nie zapisujemy wyniku (zwracamy A niezmieniony)
                    Sub(a, b);
                    result = a;
                    description = $"CMP: {a} vs {b} (ustawia tylko flagi)";
                    break;
                case Operation.INC:
                    result = Add(a, 1);
                    description = $"INC: {a} + 1";
                    break;
                case Operation.DEC:
                    result = Sub(a, 1);
                    description = $"DEC: {a} - 1";
                    break;
                case Operation.NEG:
                    // NEG = 0 - A
                    result = Sub(0, a);
                    description = $"NEG: -({a})";
                    break;
                case Operation.AND:
                    result = (ushort)(a & b);
                    SetLogicFlags(result);
                    description = $"AND: {a} & {b}";
                    break;
                case Operation.OR:
                    result = (ushort)(a | b);
                    SetLogicFlags(result);
                    description = $"OR: {a} | {b}";
                    break;
                case Operation.XOR:
                    result = (ushort)(a ^ b);
                    SetLogicFlags(result);
                    description = $"XOR: {a} ^ {b}";
                    break;
                case Operation.NOT:
                    result = (ushort)(~a & MASK16);
                    SetLogicFlags(result);
                    description = $"NOT: ~{a}";
                    break;
                case Operation.SHL:
                    result = ShiftLeft(a, b);
                    description = $"SHL: {a} << {b}";
                    break;
                case Operation.SHR:
                    result = ShiftRightLogical(a, b);
                    description = $"SHR: {a} >> {b} (logiczne)";
                    break;
                case Operation.SAR:
                    result = ShiftRightArithmetic(a, b);
                    description = $"SAR: {a} >> {b} (arytmetyczne)";
                    break;
                case Operation.ROL:
                    result = RotateLeft(a, b);
                    description = $"ROL: {a} <<< {b}";
                    break;
                case Operation.ROR:
                    result = RotateRight(a, b);
                    description = $"ROR: {a} >>> {b}";
                    break;
            }

            return result;
        }

        private void ResetFlags()
        {
            ZF = CF = SF = OF = false;
        }

        // --- Arytmetyka ---

        private ushort Add(ushort a, ushort b)
        {
            uint sum = (uint)a + (uint)b;
            ushort result = (ushort)(sum & MASK16);

            CF = sum > MASK16;                       // przeniesienie bez znaku
            ZF = result == 0;
            SF = (result & 0x8000) != 0;
            // Overflow: gdy oba operandy miały ten sam znak, a wynik przeciwny
            bool aSign = (a & 0x8000) != 0;
            bool bSign = (b & 0x8000) != 0;
            bool rSign = SF;
            OF = (aSign == bSign) && (aSign != rSign);

            return result;
        }

        private ushort Sub(ushort a, ushort b)
        {
            // A - B = A + (~B + 1); flagi liczymy bezpośrednio
            int diff = (int)a - (int)b;
            ushort result = (ushort)(diff & MASK16);

            CF = a < b;                              // pożyczka (bez znaku)
            ZF = result == 0;
            SF = (result & 0x8000) != 0;
            // Overflow przy odejmowaniu: znaki A i B różne, a znak wyniku != znak A
            bool aSign = (a & 0x8000) != 0;
            bool bSign = (b & 0x8000) != 0;
            bool rSign = SF;
            OF = (aSign != bSign) && (aSign != rSign);

            return result;
        }

        // --- Logika ---

        private void SetLogicFlags(ushort result)
        {
            ZF = result == 0;
            SF = (result & 0x8000) != 0;
            CF = false;
            OF = false;
        }

        // --- Przesunięcia / rotacje ---

        private ushort ShiftLeft(ushort a, ushort count)
        {
            int n = count & 0x0F;
            if (n == 0) { SetLogicFlags(a); return a; }

            ushort result = (ushort)((a << n) & MASK16);
            // CF = ostatni bit "wypchnięty" z lewej
            CF = ((a >> (BITS - n)) & 1) != 0;
            ZF = result == 0;
            SF = (result & 0x8000) != 0;
            // OF (tylko dla n=1): zmiana bitu znaku
            OF = (n == 1) && (((a ^ result) & 0x8000) != 0);
            return result;
        }

        private ushort ShiftRightLogical(ushort a, ushort count)
        {
            int n = count & 0x0F;
            if (n == 0) { SetLogicFlags(a); return a; }

            ushort result = (ushort)(a >> n);
            // CF = ostatni bit "wypchnięty" z prawej
            CF = ((a >> (n - 1)) & 1) != 0;
            ZF = result == 0;
            SF = (result & 0x8000) != 0;
            // OF (tylko dla n=1): = MSB oryginału
            OF = (n == 1) && ((a & 0x8000) != 0);
            return result;
        }

        private ushort ShiftRightArithmetic(ushort a, ushort count)
        {
            int n = count & 0x0F;
            if (n == 0) { SetLogicFlags(a); return a; }

            short signed = (short)a;
            ushort result = (ushort)(signed >> n); // C# zachowuje znak dla short
            CF = ((a >> (n - 1)) & 1) != 0;
            ZF = result == 0;
            SF = (result & 0x8000) != 0;
            OF = false; // SAR nigdy nie zmienia znaku
            return result;
        }

        private ushort RotateLeft(ushort a, ushort count)
        {
            int n = count & 0x0F;
            if (n == 0) { SetLogicFlags(a); return a; }

            ushort result = (ushort)(((a << n) | (a >> (BITS - n))) & MASK16);
            CF = (result & 1) != 0;                  // nowy LSB
            ZF = result == 0;
            SF = (result & 0x8000) != 0;
            OF = (n == 1) && (((result >> 15) & 1) != (CF ? 1 : 0));
            return result;
        }

        private ushort RotateRight(ushort a, ushort count)
        {
            int n = count & 0x0F;
            if (n == 0) { SetLogicFlags(a); return a; }

            ushort result = (ushort)(((a >> n) | (a << (BITS - n))) & MASK16);
            CF = (result & 0x8000) != 0;             // nowy MSB
            ZF = result == 0;
            SF = (result & 0x8000) != 0;
            OF = (n == 1) && (((result >> 15) & 1) != ((result >> 14) & 1));
            return result;
        }

        // --- Narzędzia pomocnicze ---

        public static string ToBinary(ushort value)
        {
            string s = Convert.ToString(value, 2).PadLeft(BITS, '0');
            // Dla czytelności: 4 grupy po 4 bity
            return $"{s.Substring(0, 4)} {s.Substring(4, 4)} {s.Substring(8, 4)} {s.Substring(12, 4)}";
        }

        public static string ToHex(ushort value)
        {
            return "0x" + value.ToString("X4");
        }

        public static short ToSigned(ushort value)
        {
            return (short)value;
        }

        /// <summary>
        /// Parsuje wejście: dziesiętne (z minusem), binarne (0b... lub z samych 0/1),
        /// szesnastkowe (0x...). Obsługuje liczby ujemne przez konwersję do U2.
        /// </summary>
        public static bool TryParse(string input, out ushort value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;
            input = input.Trim().Replace(" ", "").Replace("_", "");

            try
            {
                if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    value = Convert.ToUInt16(input.Substring(2), 16);
                    return true;
                }
                if (input.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
                {
                    value = Convert.ToUInt16(input.Substring(2), 2);
                    return true;
                }
                // Bin-only (same 0/1, długość >=2)
                bool allBin = input.Length >= 2;
                foreach (char c in input) if (c != '0' && c != '1') { allBin = false; break; }
                if (allBin)
                {
                    value = Convert.ToUInt16(input, 2);
                    return true;
                }
                // Dec (w tym ujemne w zakresie short)
                if (int.TryParse(input, out int n))
                {
                    if (n < -32768 || n > 65535) return false;
                    value = (ushort)(n & MASK16);
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}
