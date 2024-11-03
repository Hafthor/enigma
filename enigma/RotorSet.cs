namespace enigma;

public class RotorSet(Rotor[] rotors) {
    public Rotor[] Rotors => rotors;

    public char Translate(char c) {
        foreach (var rotor in rotors)
            c = rotor.Translate(c);
        return c;
    }

    public char ReverseTranslate(char c) {
        for (int i = rotors.Length - 1; i >= 0; i--)
            c = rotors[i].ReverseTranslate(c);
        return c;
    }

    public string Translate(string s) => new(s.Select(Translate).ToArray());
    
    public string ReverseTranslate(string s) => new(s.Select(ReverseTranslate).ToArray());
    
    public bool Advance() {
        foreach (var rotor in rotors)
            if (!rotor.Advance())
                return false;
        return true;
    }

    public int[] Positions {
        get => rotors.Select(rotor => rotor.Position).ToArray();
        set {
            for (int i = 0; i < rotors.Length; i++)
                rotors[i].Position = value[i];
        }
    }
    
    public void Reset() {
        foreach (var rotor in rotors)
            rotor.Reset();
    }
}