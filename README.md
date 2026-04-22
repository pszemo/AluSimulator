# Symulator ALU (16-bit) - C# / WinForms

Aplikacja okienkowa (WinForms, .NET 8) symulująca działanie 16-bitowej jednostki arytmetyczno-logicznej (ALU) mikroprocesora.

## Funkcjonalność

### Operacje arytmetyczne
- `ADD` - dodawanie
- `SUB` - odejmowanie
- `INC` - inkrementacja (A + 1)
- `DEC` - dekrementacja (A - 1)
- `NEG` - negacja arytmetyczna (0 - A)
- `CMP` - porównanie (A - B, wynik nie jest zapisywany, ustawiane są tylko flagi)

### Operacje logiczne
- `AND`, `OR`, `XOR`, `NOT`

### Przesunięcia i rotacje
- `SHL` - przesunięcie logiczne w lewo
- `SHR` - przesunięcie logiczne w prawo
- `SAR` - przesunięcie arytmetyczne w prawo (z zachowaniem znaku)
- `ROL` - rotacja w lewo
- `ROR` - rotacja w prawo

### Flagi procesora
- **ZF** (Zero Flag) - ustawiana gdy wynik = 0
- **CF** (Carry Flag) - przeniesienie/pożyczka w arytmetyce bez znaku
- **SF** (Sign Flag) - bit znaku wyniku (MSB)
- **OF** (Overflow Flag) - przepełnienie w arytmetyce ze znakiem (U2)

## Format danych

Operandy można wprowadzać w formatach:
- dziesiętny (np. `123`, `-456`)
- szesnastkowy (np. `0xFF`, `0x1A2B`)
- binarny (np. `0b10101010` lub `10101010`)

Zakres: 16 bitów (0..65535 bez znaku, -32768..32767 w U2).

## Struktura projektu

```
AluSimulator/
├── AluSimulator.csproj   # plik projektu .NET
├── Program.cs            # punkt wejścia
├── Alu.cs                # logika ALU (klasa Alu + parser wejścia)
└── MainForm.cs           # GUI (formularz główny)
```

## Uruchomienie

Wymagania: .NET 8 SDK + Windows (WinForms).

```powershell
cd AluSimulator
dotnet run
```

## Uwagi implementacyjne

- Liczby wewnętrznie są przechowywane jako `ushort` (16-bitowe bez znaku).
- Interpretacja w U2 odbywa się przez rzutowanie na `short`.
- Flagi są wyliczane niezależnie - bez polegania na automatyce C#.
- Przesunięcia/rotacje o liczbę > 15 są maskowane do 4 LSB licznika (jak w architekturze x86 dla słowa 16-bit).
- Aplikacja zachowuje historię ostatnich operacji w dolnym panelu.
