namespace enigma;

public class RotorSet(Rotor[] rotors) {
    public Rotor[] Rotors => rotors;

    public char Translate(char c) {
        for (int i = rotors.Length - 1; i >= 0; i--)
            c = rotors[i].Translate(c);
        return c;
    }

    public char ReverseTranslate(char c) {
        foreach (var rotor in rotors)
            c = rotor.ReverseTranslate(c);
        return c;
    }
    
    public bool Advance() {
        for (int i = rotors.Length - 1; i >= 0; i--)
            if (!rotors[i].Advance())
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