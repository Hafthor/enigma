namespace enigma;

public abstract class SBox {
    public const int Size = 26;
    public const int LetterOffset = 'A', LowerLetterOffset = 'a';

    private readonly int[,] translate, reverseTranslate;

    private readonly string xlat;
    private string rxlat;
    
    protected SBox(string s) => (translate, reverseTranslate) = New(xlat = s);

    // call with -1 to make non-reflecting random sbox
    protected SBox(Random rand = null, int reflectingCount = -1) {
        if (reflectingCount * 2 > Size)
            throw new ArgumentException($"{nameof(reflectingCount)} is too large (max {Size / 2})");
        List<int> range = new(Enumerable.Range(0, Size));
        var ca = range.Select(c => (char)(c + LetterOffset)).ToArray();
        if (rand == null) {
            ; // do nothing
        } else if (reflectingCount >= 0) {
            for (int i = 0; i < reflectingCount; i++) {
                int ai = rand.Next(range.Count), a = range[ai];
                range.RemoveAt(ai);
                int bi = rand.Next(range.Count), b = range[bi];
                range.RemoveAt(bi);
                (ca[a], ca[b]) = (ca[b], ca[a]);
            }
        } else {
            // Fisher-Yates shuffle
            for (int i = Size - 1; i > 0; i--) {
                int j = rand.Next(i + 1);
                (ca[i], ca[j]) = (ca[j], ca[i]);
            }
        }
        (translate, reverseTranslate) = New(xlat = new(ca));
    }

    private (int[,], int[,]) New(string s) {
        if (string.IsNullOrEmpty(s))
            s = new(Enumerable.Range(0, Size).Select(i => (char)(i + LetterOffset)).ToArray());
        if (s.Length != Size)
            throw new ArgumentException($"s must be {Size} characters long");
        for (int i = 0; i < Size; i++)
            if (!char.IsUpper(s[i]))
                throw new ArgumentException("s must contain only uppercase letters");
        
        // invert s to make rxlat
        var ca = new char[Size];
        for (int i = 0; i < Size; i++) {
            int j= s[i] - LetterOffset;
            ca[j] = (char)(i + LetterOffset);
        }
        rxlat = new(ca);
        
        int[,] ft = new int[Size, Size], rt = new int[Size, Size];
        for (int offset = 0; offset < Size; offset++) {
            string x = s[offset..] + s[..offset];
            for (int i = 0; i < Size; i++) {
                ft[offset, i] = x[i] - LetterOffset;
                rt[offset, x[i] - LetterOffset] = i;
            }
        }
        return (ft, rt);
    }

    public bool IsReflecting => xlat == rxlat;
    
    public char Translate(char c, int offset) => Translate(translate, c, offset);

    public char ReverseTranslate(char c, int offset) => Translate(reverseTranslate, c, offset);

    private char Translate(int[,] translateMatrix, char c, int offset) {
        if (translateMatrix == translate) return Translate2(c, offset);
        return RTranslate2(c, offset);
        
        // return c switch {
        //     _ when char.IsUpper(c) => (char)(translateMatrix[offset, c - LetterOffset] + LetterOffset),
        //     _ when char.IsLower(c) => (char)(translateMatrix[offset, c - LowerLetterOffset] + LowerLetterOffset),
        //     _ => c
        // };
    }

    private char Translate2(char c, int position) {
        if (!char.IsLetter(c)) return c;
        int i = c - (char.IsUpper(c) ? LetterOffset : LowerLetterOffset);
        i = (i + position) % Size;
        var x = xlat.Select(c => c - LetterOffset).ToArray();
        int j = (x[i] + Size - position) % Size;
        return (char)(j + (char.IsUpper(c) ? LetterOffset : LowerLetterOffset));
    }
    
    //  3 2 1 | 1 2 3           abcde fghij klmno pqrst uvw xyz
    // D I X R B W M M      III BDFHJ LCPRT XVZNY EIWGA KMU SQO
    // E A A E Q H L L      II  AJDKS IRUXB LHWCT MNYPQ OZV GFE
    // R H U A Y O Y X      I   EKMFL GDQVZ NTOWY HXUSP AIB RCJ
    //                      B   YRUHQ SLDPX NGOKM IEBFZ CWV JAT

    private char RTranslate2(char c, int position) {
        if (!char.IsLetter(c)) return c;
        int i = c - (char.IsUpper(c) ? LetterOffset : LowerLetterOffset);
        i = (i + position) % Size;
        var x = rxlat.Select(c => c - LetterOffset).ToList();
        int j = (x[i] + Size - position) % Size;
        return (char)(j + (char.IsUpper(c) ? LetterOffset : LowerLetterOffset));
    }
    
    // Example:
    // A B C   B C A   C A B
    // |  x     x  |   < | >
    // A C B   C B A   B A C
    // A->A    A->B    A->C
}