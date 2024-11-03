namespace enigma;

public class Rotor : SBox {
    public Rotor(Random rand, int position) : base(rand, 13) => Position = InitialPosition = position;
    public Rotor(Random rand) : this(rand, 13) => Position = InitialPosition = rand.Next(26);
    public Rotor(string s, int position) : base(s) => Position = InitialPosition = position;
    private int InitialPosition { get; }
    public int Position { get; set; }
    public bool Advance() => (Position = (Position + 1) % 26) == 0;
    public void Reset() => Position = InitialPosition;
    public char Translate(char c) => base.Translate(c, Position);
    public char ReverseTranslate(char c) => base.ReverseTranslate(c, Position);
}