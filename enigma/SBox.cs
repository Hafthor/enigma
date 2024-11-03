namespace enigma;

public abstract class SBox {
    const int Size = 26;
    private const int LetterOffset = 'A', LowerLetterOffset = 'a';

    private readonly int[,] translate, reverseTranslate;
    protected SBox() {
        reverseTranslate = translate = new int[Size, Size];
        for (int i = 0; i < Size; i++)
            for (int j = 0; j < Size; j++)
                translate[i, j] = j;
    }

    protected SBox(string s) => (translate, reverseTranslate) = New(s);

    private (int[,], int[,]) New(string s) {
        if (s.Length != Size)
            throw new ArgumentException($"s must be {Size} characters long");
        for (int i = 0; i < Size; i++)
            if (!char.IsUpper(s[i]))
                throw new ArgumentException("s must contain only uppercase letters");
        int[,] translate = new int[Size, Size], reverseTranslate = new int[Size, Size];
        for (int offset = 0; offset < Size; offset++) {
            string x = s[offset..] + s[..offset];
            for (int i = 0; i < Size; i++) {
                translate[offset, i] = x[i] - LetterOffset;
                reverseTranslate[offset, x[i] - LetterOffset] = i;
            }
        }
        return (translate, reverseTranslate);
    }
    
    public bool IsReflecting {
        get {
            for (int i = 0; i < Size; i++)
                if(translate[0, translate[0, i]] != i)
                    return false;
            return true;
        }
    }
    
    protected SBox(Random rand, int count) {
        if (count * 2 > Size)
            throw new ArgumentException($"count is too large (max {Size / 2})");
        List<int> range = new(Enumerable.Range(0, Size));
        char[] ca = range.Select(c => (char)(c + LetterOffset)).ToArray();
        for (int i = 0; i < count; i++) {
            int ai = rand.Next(range.Count), a = range[ai];
            range.RemoveAt(ai);
            int bi = rand.Next(range.Count), b = range[bi];
            range.RemoveAt(bi);
            (ca[a], ca[b]) = (ca[b], ca[a]);
        }
        (translate, reverseTranslate) = New(new(ca));
    }

    public char Translate(char c, int offset) {
        return c switch {
            _ when char.IsUpper(c) => (char)(translate[offset, c - LetterOffset] + LetterOffset),
            _ when char.IsLower(c) => (char)(translate[offset, c - LowerLetterOffset] + LowerLetterOffset),
            _ => c
        };
    }
    
    public char ReverseTranslate(char c, int offset) {
        return c switch {
            _ when char.IsUpper(c) => (char)(reverseTranslate[offset, c - LetterOffset] + LetterOffset),
            _ when char.IsLower(c) => (char)(reverseTranslate[offset, c - LowerLetterOffset] + LowerLetterOffset),
            _ => c
        };
    }
}