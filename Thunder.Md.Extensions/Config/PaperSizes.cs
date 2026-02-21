namespace Thunder.Md.Extensions.Config;

internal static class PaperSizes{
    private record struct PaperTypeDimension(PaperType PaperType, int Width, int Height);

    private static readonly PaperTypeDimension[] _paperTypeDimensions =[
                                                                           new(PaperType.A3, 297, 420),
                                                                           new(PaperType.A4, 210, 297),
                                                                           new(PaperType.A5, 148, 210),
                                                                           new(PaperType.UsLetter, 216, 279),
                                                                       ];

    public static bool TryGetDimensions(PaperOrientation orientation, PaperType paperType,
                                        out (int Width, int Height) result){
        PaperTypeDimension match = _paperTypeDimensions.FirstOrDefault(dimension => dimension.PaperType == paperType);
        if(match == default){
            result = default;
            return false;
        }

        result = orientation switch{
                     PaperOrientation.Portrait => (match.Width, match.Height),
                     PaperOrientation.Landscape => (match.Height, match.Width),
                     _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
                 };
        return true;
    }

    public static PaperType GetPaperType(int width, int height){
        PaperTypeDimension match
            = _paperTypeDimensions.FirstOrDefault(dimension => dimension.Width == width && dimension.Height == height);
        return match == default ? PaperType.Other : match.PaperType;
    }
}

public enum PaperType{
    Other,
    A5,
    A4,
    A3,
    UsLetter
}

public enum PaperOrientation{
    Portrait,
    Landscape
}

public static class PaperExtensions{
    private static readonly char[] _charsToRemove = ['_', '-', ' ', '\t', '\n', '\r'];

    extension(PaperType paperType){
        public static bool TryParseFromString(string paperTypeStr, out PaperType result){
            string simplified = string.Concat(paperTypeStr.Where(c => !_charsToRemove.Contains(c)).Select(char.ToLower));
            result = simplified switch{
                         "din" or "dina4" or "a4"       => PaperType.A4,
                         "dina3" or "a3"                => PaperType.A3,
                         "dina5" or "a5"                => PaperType.A5,
                         "us" or "letter" or "usletter" => PaperType.UsLetter,
                         _                              => PaperType.Other
                     };
            return result != PaperType.Other;
        }
    }


    extension(PaperOrientation paperType){
        public static bool TryParseFromString(string orientationStr, out PaperOrientation result){
            string simplified
                = string.Concat(orientationStr.Where(c => _charsToRemove.Contains(c)).Select(char.ToLower));
            result = simplified switch{
                         "landscape" or "horizontal"          => PaperOrientation.Landscape,
                         "portrait" or "vertical" or "normal" => PaperOrientation.Portrait,
                         _                                    => (PaperOrientation)(-1)
                     };
            return (int)result >= 0;
        }
    }
}