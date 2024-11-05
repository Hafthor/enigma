namespace enigma;

public class Reflector : SBox {
    public Reflector(Random rand) : base(rand, Size/2) {}

    public Reflector(string s) : base(s) {
        if (!IsReflecting) throw new ArgumentException("must be a reflecting reflector");
    }
    public char Translate(char c) => base.Translate(c, 0);
}