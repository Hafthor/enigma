namespace enigma;

public class Enigma(Plugboard plugboard, RotorSet rotorSet, Reflector reflector) {
    public Plugboard Plugboard => plugboard;
    public RotorSet RotorSet => rotorSet;
    public Reflector Reflector => reflector;

    public void Reset() => RotorSet.Reset();

    public char Translate(char c, bool advance = true) {
        c = Plugboard.ReverseTranslate(RotorSet.ReverseTranslate(Reflector.Translate(RotorSet.Translate(Plugboard.Translate(c)))));
        if (advance && char.IsLetter(c)) _ = RotorSet.Advance(); // don't advance if the character is the same
        return c;
    }

    public string Translate(string s) => new(s.Select(c => Translate(c)).ToArray());
}