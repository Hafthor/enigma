namespace enigma;

public class Rotor : SBox {
    public Rotor(Random rand, int position = -1, int ringPosition = -1) : base(rand, Size/2) {
        Position = InitialPosition = position >= 0 ? position : rand.Next(Size);
        RingPosition = ringPosition >= 0 ? ringPosition : rand.Next(Size);
    }
    public Rotor(string s, int position, int ringPosition) : base(s) {
        Position = InitialPosition = position;
        RingPosition = ringPosition;
    }

    public int InitialPosition { get; }
    public int Position { get; set; }
    public int RingPosition { get; }
    public bool Advance() => (Position = (Position + 1) % Size) == 0;
    public void Reset() => Position = InitialPosition;
    public char Translate(char c) => base.Translate(c, (RingPosition + Position) % Size);
    public char ReverseTranslate(char c) => base.ReverseTranslate(c, (RingPosition + Position) % Size);
}