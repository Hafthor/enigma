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

    // Note: Have not validated the correctness of this Enigma machine simulator yet
    public static int Main(string[] args) {
        var sw = Stopwatch.GetTimestamp();
        // default settings
        string r1 = "I~1", r2 = "II~1", r3 = "III~1", r = "A", p = "ETW";
        string s = "DER SCHNELLE BRAUNE FUCHS SPRINGT UBER DEN FAULEN HUND";
        if (args.Length != 6) {
            Console.WriteLine("Usage: enigma plugboard rotor1[~pos] rotor2[~pos] rotor3[~pos] reflector message");
            Console.WriteLine("Example: enigma C V~6 III~26 II~10 B");
            Console.WriteLine($"Default: enigma {p} {r1} {r2} {r3} {r} '{s}'");
            Console.WriteLine("You may instead specify the 26 character substitution for any component");
            if (args.Length != 0) return 1;
            Console.WriteLine();
        } else {
            p = args[0];
            r1 = args[1];
            r2 = args[2];
            r3 = args[3];
            r = args[4];
            s = args[5];
        }
        p = ParsePlugboard(p);
        (r1, int p1) = ParseRotor(r1);
        (r2, int p2) = ParseRotor(r2);
        (r3, int p3) = ParseRotor(r3);
        r = ParseReflector(r);

        Enigma enigma = new(new(p), new([new(r1, p1), new(r2, p2), new(r3, p3)]), new(r));
        Console.WriteLine($"Plugboard..: {ShowTranslate(p, enigma.Plugboard)}");
        Console.WriteLine($"Rotor 1 @{p1 + 1:D2}: {ShowTranslate(r1)}");
        Console.WriteLine($"Rotor 2 @{p2 + 1:D2}: {ShowTranslate(r2)}");
        Console.WriteLine($"Rotor 3 @{p3 + 1:D2}: {ShowTranslate(r3)}");
        Console.WriteLine($"Reflector..: {ShowTranslate(r, enigma.Reflector)}");
        Console.WriteLine($"Message....: {s} ({s.Count(char.IsLetter)} letters)");
        Console.WriteLine($"Repeating @: {CalcRepeating(s.Count(char.IsLetter))}");
        Console.WriteLine($"Encrypted..: {enigma.Translate(s)}");
        var pos = enigma.RotorSet.Positions.Select(p => (p + 1).ToString("D2"));
        Console.WriteLine($"Ending pos.: {string.Join(",", pos)}");

        string ShowTranslate(string x, SBox sbox = null) {
            string warn = sbox?.IsReflecting == false ? " Warning: not reflecting" : "";
            foreach (var (k, v) in Rotors)
                if (v == x)
                    return $"{x} ({k}){warn}";
            return $"{x}{warn}";
        }

        int CalcRepeating(int n) {
            int p = 26 * 26 * 26;
            for (; n % 2 == 0 && p % 2 == 0; n /= 2) p /= 2;
            for (; n % 13 == 0 && p % 13 == 0; n /= 13) p /= 13;
            return p;
        }

        enigma.Reset();
        HashSet<string> set = new();
        for (int rep = 0;; rep++) { // run until we find a duplicate
            var savePos = enigma.RotorSet.Positions;
            string x = enigma.Translate(s);
            if (!set.Add(x)) {
                Console.WriteLine("Dup found @: " + rep + " reps, 26^3/reps=" + ((26.0 * 26 * 26) / rep));
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
            if (s != n) Console.WriteLine("Error......: " + n + " <= Encryption/decryption round trip failed @ " + rep);
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

    public static (string, int) ParseRotor(string s) {
        var ss = s.Split('~');
        if (ss.Length > 2) throw new ArgumentException("Invalid rotor format");
        int pos = 1;
        if (ss.Length > 1 && !int.TryParse(ss[1], out pos)) throw new ArgumentException("Invalid rotor position");
        if (pos is < 1 or > 26) throw new ArgumentException("Rotor position must be between 1 and 26");
        return (ParseReflector(ss[0]), pos - 1);
    }

    public static string ParseReflector(string s) => s.Length == 26 ? s : Rotors[s];

    public static string ParsePlugboard(string s) => ParseReflector(s == "" ? "ETW" : s);
}