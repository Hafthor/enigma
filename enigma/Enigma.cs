namespace enigma;

public class Enigma(Plugboard plugboard, RotorSet rotorSet, Reflector reflector) {
    public Plugboard Plugboard => plugboard;
    public RotorSet RotorSet => rotorSet;
    public Reflector Reflector => reflector;

    public void Reset() => RotorSet.Reset();

    public char Translate(char c, bool advance = true) {
        if (advance && char.IsLetter(c)) _ = RotorSet.Advance(); // don't advance if the character is the same
        return Plugboard.ReverseTranslate(RotorSet.ReverseTranslate(Reflector.Translate(RotorSet.Translate(Plugboard.Translate(c)))));
    }

    public string Translate(string s) => new(s.Select(c => Translate(c)).ToArray());
}