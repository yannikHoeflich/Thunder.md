namespace Thunder.Md.Extensions.Config;

using System.Globalization;

public record struct ThunderColor(byte R, byte G, byte B, byte A = 255){
    
    public string ToHex(){
        char[] str = A == 255 ? new char[6] : new char[8];
        
        str[0] = ToChar(R / 16);
        str[1] = ToChar(R % 16);
        
        str[2] = ToChar(G / 16);
        str[3] = ToChar(G % 16);
        
        str[4] = ToChar(B / 16);
        str[5] = ToChar(B % 16);

        if(A == 255){
            return new string(str);
        }

        str[6] = ToChar(A / 16);
        str[7] = ToChar(A % 16);

        return new string(str);
    }

    private char ToChar(int i){
        if(i < 10){
            return (char)(i + '0');
        }
        return (char)(i - 10 + 'A');
    }

    public static bool TryFromHex(string hex, out ThunderColor thunderColor){
        thunderColor = default;
        ReadOnlySpan<char> hexSpan = hex.AsSpan();
        if(hexSpan.StartsWith("0x", StringComparison.OrdinalIgnoreCase)){
            hexSpan = hexSpan[2..].Trim();
        }

        if(hexSpan.Length != 6 && hexSpan.Length != 8){
            return false;
        }

        byte red = 0;
        byte green = 0;
        byte blue = 0;
        byte alpha = byte.MaxValue;

        ReadOnlySpan<char> redStr = hexSpan[..2];
        if(!TryParseHexByte(redStr, out red)){
            return false;
        }

        ReadOnlySpan<char> greenStr = hexSpan[2..4];
        if(!TryParseHexByte(greenStr, out green)){
            return false;
        }

        ReadOnlySpan<char> blueStr = hexSpan[4..6];
        if(!TryParseHexByte(blueStr, out blue)){
            return false;
        }

        if(hexSpan.Length == 8){
            ReadOnlySpan<char> alphaStr = hexSpan[6..8];
            if(!TryParseHexByte(alphaStr, out alpha)){
                return false;
            }
        }

        thunderColor = new ThunderColor(red, green, blue, alpha);
        return true;
    }

    private static bool TryParseHexByte(ReadOnlySpan<char> str, out byte result) =>
        byte.TryParse(str, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out result);


    public static ThunderColor White => new(255, 255, 255);
    public static ThunderColor Black => new(0, 0, 0);
}