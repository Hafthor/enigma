using System.Diagnostics;

namespace enigma;

public static class Program {
    public static readonly Dictionary<string, string> Rotors = new() {
        // ref: https://en.wikipedia.org/wiki/Enigma_rotor_details
        //              ABCDEFGHIJKLMNOPQRSTUVWXYZ
        { "IC", /*****/"DMTWSILRUYQNKFEJCAZBPGXOHV" }, // 1924 Commercial Enigma A, B
        { "IIC", /****/"HQZGPJTMOBLNCIFDYAWVEUSRKX" }, // 1924 Commercial Enigma A, B
        { "IIIC", /***/"UQNTLSZFMREHDPXKIBVYGJCWOA" }, // 1924 Commercial Enigma A, B
        { "IR", /*****/"JGDQOXUSCAMIFRVTPNEWKBLZYH" }, // 1941 German Railway (Rocket)
        { "IIR", /****/"NTZPSFBOKMWRCJDIVLAEYUXHGQ" }, // 1941 German Railway (Rocket)
        { "IIIR", /***/"JVIUBHTCDYAKEQZPOSGXNRMWFL" }, // 1941 German Railway (Rocket)
        { "UKWR", /***/"QYHOGNECVPUZTFDJAXWMKISRBL" }, // 1941 German Railway (Rocket)
        { "ETWR", /***/"QWERTZUIOASDFGHJKPYXCVBNML" }, // 1941 German Railway (Rocket)
        { "I", /******/"EKMFLGDQVZNTOWYHXUSPAIBRCJ" }, // 1930 Enigma I
        { "II", /*****/"AJDKSIRUXBLHWCTMNYPQOZVGFE" }, // 1930 Enigma I
        { "III", /****/"BDFHJLCPRTXVZNYEIWGAKMUSQO" }, // 1930 Enigma I
        { "IV", /*****/"ESOVPZJAYQUIRHXLNFTGKDCMWB" }, // 1938 Enigma M3 Army
        { "V", /******/"VZBRGITYUPSDNHLXAWMJQOFECK" }, // 1938 Enigma M3 Army
        { "VI", /*****/"JPGVOUMFYQBENHZRDKASXLICTW" }, // 1939 Enigma M3 & M4 Naval
        { "VII", /****/"NZJHGRCXMYSWBOUFAIVLPEKQDT" }, // 1939 Enigma M3 & M4 Naval
        { "VIII", /***/"FKQHTLXOCBJSPDZRAMEWNIUYGV" }, // 1939 Enigma M3 & M4 Naval
        { "Beta", /***/"LEYJVCNIXWPBQMDRTAKZGFUHOS" }, // 1941 Enigma M4 R2
        { "Gamma", /**/"FSOKANUERHMBTIYCWLQPZXVGJD" }, // 1942 Enigma M4 R2
        { "A", /******/"EJMZALYXVBWFCRQUONTSPIKHGD" }, // Reflector A
        { "B", /******/"YRUHQSLDPXNGOKMIEBFZCWVJAT" }, // Reflector B
        { "C", /******/"FVPJIAOYEDRZXWGCTKUQSBNMHL" }, // Reflector C
        { "ETW", /****/"ABCDEFGHIJKLMNOPQRSTUVWXYZ" }, // Enigma I
    };

    // Note: This Enigma machine simulator does not encrypt/decrypt properly yet
    //             3 2 1 | 1 2 3
    // D should go I X R B W M M
    // E should go A A E Q H L L
    // R should go H U A Y O Y X
    public static int Main(string[] args) {
        var sw = Stopwatch.GetTimestamp();
        // default settings
        string[] rs = ["I~1~1", "II~1~1", "III~1~1"];
        string r = "B", p = "ETW";
        string s = "DER SCHNELLE BRAUNE FUCHS SPRINGT UBER DEN FAULEN HUND";
        if (args.Length < 3) {
            Console.WriteLine("Usage: enigma plugboard [rotor[~pos[~rp]]...] reflector message");
            Console.WriteLine("Example: enigma C V~6~19 III~26~4 II~10~20 B 'HELLO'");
            Console.WriteLine($"Default: enigma {p} {string.Join(" ", rs)} {r} '{s}'");
            Console.WriteLine($"You may instead specify the full {SBox.Size} character substitution for any component");
            if (args.Length != 0) return 1;
            Console.WriteLine();
        } else {
            p = args[0];
            rs = args[1..^2];
            r = args[^2];
            s = args[^1];
        }
        p = ParsePlugboard(p);
        var rps = rs.Select(ParseRotor).ToArray();
        r = ParseReflector(r);
        Random rand = new();
        Plugboard plugboard = p == "random" || p.StartsWith("random~") ? 
            new(rand, ParseRandom(p, SBox.Size / 2)) : new(p);
        Rotor[] rotors = rps.Select(x => x.x == "random" || x.x.StartsWith("random~") ? 
            new Rotor(rand, x.p, x.r) : new(x.x, x.p, x.r)).ToArray();
        Reflector reflector = r == "random" || r.StartsWith("random~") ? new(rand) : new(r);
        Enigma enigma = new(plugboard, new(rotors), reflector);

        Console.WriteLine("               p 3 2 1 | 1 2 3 p");
        char c = 'D';
        Console.Write($"Starting with {c} ");
        enigma.RotorSet.Advance();
        c = enigma.Plugboard.Translate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[2].Translate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[1].Translate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[0].Translate(c); Console.Write($"{c} ");
        c = enigma.Reflector.Translate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[0].ReverseTranslate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[1].ReverseTranslate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[2].ReverseTranslate(c); Console.Write($"{c} ");
        c = enigma.Plugboard.ReverseTranslate(c); Console.WriteLine($"{c}");
        if (c != 'M') return 1;
        
        c = 'E';
        Console.Write($"Starting with {c} ");
        enigma.RotorSet.Advance();
        c = enigma.Plugboard.Translate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[2].Translate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[1].Translate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[0].Translate(c); Console.Write($"{c} ");
        c = enigma.Reflector.Translate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[0].ReverseTranslate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[1].ReverseTranslate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[2].ReverseTranslate(c); Console.Write($"{c} ");
        c = enigma.Plugboard.ReverseTranslate(c); Console.WriteLine($"{c}");
        if (c != 'L') return 1;
        
        c = 'R';
        Console.Write($"Starting with {c} ");
        enigma.RotorSet.Advance();
        c = enigma.Plugboard.Translate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[2].Translate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[1].Translate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[0].Translate(c); Console.Write($"{c} ");
        c = enigma.Reflector.Translate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[0].ReverseTranslate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[1].ReverseTranslate(c); Console.Write($"{c} ");
        c = enigma.RotorSet.Rotors[2].ReverseTranslate(c); Console.Write($"{c} ");
        c = enigma.Plugboard.ReverseTranslate(c); Console.WriteLine($"{c}");
        if (c != 'X') return 1;
        
        enigma.Reset();
        Console.WriteLine($"Plugboard..: {ShowTranslate(p, enigma.Plugboard)}");
        for (int i = 0; i < rps.Length; i++)
            Console.WriteLine($"Rotor {i} @{rps[i].p + 1:D2}: {ShowTranslate(rps[i].x, enigma.RotorSet.Rotors[i])}");
        Console.WriteLine($"Reflector..: {ShowTranslate(r, enigma.Reflector)}");
        Console.WriteLine($"Message....: {s} ({s.Count(char.IsLetter)} letters)");
        Console.WriteLine($"Repeating @: {CalcRepeating(s.Count(char.IsLetter))}");
        Console.WriteLine($"Encrypted..: {enigma.Translate(s)}");
        var pos = enigma.RotorSet.Positions.Select(p => (p + 1).ToString("D2"));
        Console.WriteLine($"Ending pos.: {string.Join(",", pos)}");

        int ParseRandom(string s, int def) {
            if (s == "random") return def;
            var ss = s.Split('~');
            if (ss.Length != 2) throw new ArgumentException("Invalid random format");
            if (!int.TryParse(ss[1], out int n)) throw new ArgumentException("Invalid random count");
            return n;
        }

        string ShowTranslate(string x, SBox sbox = null) {
            string warn = sbox is Rotor rotor ? $" Ring pos={rotor.RingPosition + 1:D2}" :
                sbox?.IsReflecting == false ? " Warning: not reflecting" : "";
            foreach (var (k, v) in Rotors)
                if (v == x)
                    return $"{x} ({k}){warn}";
            return $"{x}{warn}";
        }

        int CalcRotorCombinations() => enigma.RotorSet.Rotors.Aggregate(1, (a, _) => a * SBox.Size);

        int CalcRepeating(int n) => CalcRotorCombinations() / Gcd(n, CalcRotorCombinations());

        int Gcd(int a, int b) => b == 0 ? a : Gcd(b, a % b);

        enigma.Reset();
        HashSet<string> set = new();
        for (int rep = 0;; rep++) { // run until we find a duplicate
            var savePos = enigma.RotorSet.Positions;
            string x = enigma.Translate(s);
            if (!set.Add(x)) {
                int pr = CalcRotorCombinations() / rep;
                Console.WriteLine($"Dup found @: {rep} reps, {SBox.Size}^{enigma.RotorSet.Rotors.Length}/reps={pr}");
                break;
            }
            // check if the same character is the same in the original and the encrypted string
            // this is actually a weakness in the Enigma machine's design - another is that a repeated
            // character will not be encrypted to the same character
            for (int i = 0; i < s.Length; i++) {
                if (char.IsLetter(s[i]) && s[i] == x[i]) {
                    Console.WriteLine($"Warning....: Character {i} is the same in rep {rep}");
                    break;
                }
            }
            enigma.RotorSet.Positions = savePos;
            string n = enigma.Translate(x);
            if (s != n) Console.WriteLine($"Error......: {n} <= Encryption/decryption round trip failed @ {rep}");
        }
        Console.WriteLine($"Exec time..: {Stopwatch.GetElapsedTime(sw)}");

        enigma.Reset();
        Console.Write("Benchmark..: ");
        sw = Stopwatch.GetTimestamp();
        for (int rep = 0; rep < 1_000_000; rep++)
            enigma.Translate(s);
        Console.WriteLine($"{Stopwatch.GetElapsedTime(sw).TotalSeconds} µs");
        return 0;
    }

    public static (string x, int p, int r) ParseRotor(string s) {
        if (s == "random" || s.StartsWith("random~")) return (s, 0, 0);
        var ss = s.Split('~');
        if (ss.Length > 3) throw new ArgumentException("Invalid rotor format");
        int pos = 1, rpos = 1;
        if (ss.Length > 1 && !int.TryParse(ss[1], out pos)) throw new ArgumentException("Invalid rotor position");
        if (pos is < 1 or > SBox.Size)
            throw new ArgumentException($"Rotor position must be between 1 and {SBox.Size}");
        if (ss.Length > 2 && !int.TryParse(ss[2], out rpos)) throw new ArgumentException("Invalid rotor ring position");
        if (rpos is < 1 or > SBox.Size)
            throw new ArgumentException($"Rotor ring position must be between 1 and {SBox.Size}");
        return (ParseReflector(ss[0]), pos - 1, rpos - 1);
    }

    public static string ParseReflector(string s) {
        if (s == "random" || s.StartsWith("random~")) return s;
        if (Rotors.TryGetValue(s, out string r)) return r;
        if (s.Length != SBox.Size)
            throw new ArgumentException(
                $"Reflector must be {SBox.Size} characters long or one of [{string.Join(", ", Rotors.Keys)}]");
        return s;
    }

    public static string ParsePlugboard(string s) => ParseReflector(s == "" ? "ETW" : s);
}