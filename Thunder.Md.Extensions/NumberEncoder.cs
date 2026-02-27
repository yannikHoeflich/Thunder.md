namespace Thunder.Md.Extensions;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Thunder.Md.Extensions.Config;

public static class NumberEncoder{
    private const string AlphabeticUppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string AlphabeticLowercase = "abcdefghijklmnopqrstuvwxyz";

    public static string Encode(int number, NumberingStyle style){
        if(style == NumberingStyle.NotShow){
            return "";
        }
        
        if(number == 0){
            return string.Empty;
        }

        switch(style){
            case NumberingStyle.Numeric:
                return number.ToString();
            case NumberingStyle.AlphabeticUppercase or NumberingStyle.AlphabeticLowercase:{
                string charset = style == NumberingStyle.AlphabeticUppercase ? AlphabeticUppercase : AlphabeticLowercase;
                return BaseN(number, charset);
            }
            case NumberingStyle.RomanLowercase or NumberingStyle.RomanUppercase:{
                string romanNumber = ToRoman(number);
                return style == NumberingStyle.RomanLowercase ? romanNumber.ToLower() : romanNumber.ToUpper();
            }
        }
        
        return number.ToString();
    }

    private static string BaseN(int number, string charSet){
        StringBuilder result = new StringBuilder();
        if(number < 0){
            result.Append('-');
            number = -number;
        }

        int baseN = charSet.Length;

        if(number == 0)
            return charSet[0].ToString();

        var chars = new Stack<char>();

        while(number > 0){
            int remainder = number % baseN;
            chars.Push(charSet[remainder]);
            number /= baseN;
        }

        result.Append(new string(chars.ToArray()));
        
        return result.ToString();
    }

    private static readonly ReadOnlyDictionary<int, string> _romanMap = new(new Dictionary<int, string>{
                                                                          {1000, "M"},
                                                                          {900, "CM"},
                                                                          {500, "D"},
                                                                          {400, "CD"},
                                                                          {100, "C"},
                                                                          {90, "XC"},
                                                                          {50, "L"},
                                                                          {40, "XL"},
                                                                          {10, "X"},
                                                                          {9, "IX"},
                                                                          {5, "V"},
                                                                          {4, "IV"},
                                                                          {1, "I"},
                                                                      });

    private static string ToRoman(int number){
        if(number <= 0 || number > 3999)
            return "-";


        var result = new System.Text.StringBuilder();

        foreach((var value, var symbol) in _romanMap){
            while(number >= value){
                result.Append(symbol);
                number -= value;
            }
        }

        return result.ToString();
    }
}