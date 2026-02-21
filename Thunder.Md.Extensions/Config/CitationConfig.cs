namespace Thunder.Md.Extensions.Config;

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Thunder.Md.Extensions.Cite;
using Thunder.Md.Extensions.PdfElements;

public class CitationConfig{
    public CitationPosition Position{ get; set; } = CitationPosition.Normal;
    public string Prefix{ get; set; } = "[";
    public string Suffix{ get; set; } = "]";
    public CitationGroup[] Groups{ get; set; } = [new(4, [new CitationValue("author"), new CitationValue("organisation"), new CitationValue("title")], StringFormat.Uppercase),
                                                 new(2, [new CitationValue("year")], StringFormat.Uppercase)];
    public NumberingStyle NumberingStyle{ get; set; } = NumberingStyle.Numeric;
    
    public FontStyle Style{ get; set; }
    

    public string Generate(ThunderPublication publication, int count){
        StringBuilder label = new StringBuilder();
        label.Append(Prefix);
        foreach(CitationGroup group in Groups){
            AddGroup(label, group, publication);
        }

        if(Groups.Length == 0){
            count++;
        }

        label.Append(NumberEncoder.Encode(count, NumberingStyle));

        label.Append(Suffix);
        return label.ToString();
    }

    private void AddGroup(StringBuilder label, CitationGroup group, ThunderPublication publication){
        foreach(CitationValue value in group.Values){
            if(!TryGetValue(value, publication, out string? fullString)){
                continue;
            }

            string labelPart = GetLabelPart(fullString, group.Chars, CitationValue.RightToLeftValues.Contains(value.Id));

            labelPart = group.Format switch{
                            StringFormat.Uppercase => labelPart.ToUpper(),
                            StringFormat.Lowercase => labelPart.ToLower(),
                            _                      => labelPart
                        };
            label.Append(labelPart);
            return;
        }
    }

    private bool TryGetValue(CitationValue value, ThunderPublication publication, [NotNullWhen(true)] out string? result){
        result = value.Id.ToLower() switch{
                    "year"         => publication.Year?.ToString(),
                    "author"       => publication.Authors?[0].Lastname,
                    "title"        => publication.Title,
                    "organisation" => publication.Organisation,
                    "publisher"    => publication.Publisher,
                     _             => null
                 };

        return result is not null;
    }

    private string GetLabelPart(string fullString, int charCount, bool rightToLeft){
        fullString = string.Concat(fullString.Where(char.IsLetterOrDigit));
        
        if(fullString.Length <= charCount){
            return fullString;
        }
        
        return rightToLeft ? fullString[^charCount..] : fullString[..charCount];
    }
}

public record CitationGroup(int Chars, CitationValue[] Values, StringFormat Format);

public record CitationValue(string Id){
    public static ReadOnlySet<string> RightToLeftValues{ get; } = new(new HashSet<string>(StringComparer.OrdinalIgnoreCase){
                                                                       "year"
                                                                   });
}
public enum StringFormat{
    None,
    Uppercase,
    Lowercase
}
public enum CitationPosition{
    Normal,
    SubScript,
    SuperScript
}