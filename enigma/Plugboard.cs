namespace enigma;

public class Plugboard : SBox {
    public Plugboard(Random rand, int reflectingCount) : base(rand, reflectingCount) {}

    public Plugboard(string s) : base(s) {
        if (!IsReflecting) throw new ArgumentException("must be a reflecting plugboard");
    }

    public char Translate(char c) => base.Translate(c, 0);

    public char ReverseTranslate(char c) => base.ReverseTranslate(c, 0);
}