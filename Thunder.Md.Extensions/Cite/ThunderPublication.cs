namespace Thunder.Md.Extensions.Cite;

using System.Text;

public class ThunderPublication{
    public ThunderPublication(string label, string type){
        Label = label;
        Type = type;
    }

    public string Type{ get; set; }
    public string Label{ get; }
    public string? Title{ get; set; } = null;
    public ThunderAuthor[]? Authors{ get; set; } = null;
    public string? Organisation{ get; set; } = null;
    public string? Publisher{ get; set; } = null;
    public int? Year{ get; set; } = null;

    public override string ToString(){
        StringBuilder  builder = new StringBuilder();
        if(Title is not null){
            builder.Append(Title);
        }

        if(Year is not null){
            builder.Append('(').Append(Year).Append(')');
        }

        if(Authors is not null){
            builder.Append(" by ");
            builder.Append(string.Join(" and ",  Authors.Select(x => x.ToDisplayName())));
        }

        if(Organisation is not null){
            builder.Append(", ");
            builder.Append(Organisation);
        }

        if(Publisher is not null){
            builder.Append(" Published by ");
            builder.Append(Publisher);
        }
        
        return builder.ToString();
    }
}

public record ThunderAuthor(string? Firstnames, string? Lastname, string? Suffix){
    public string ToDisplayName(){
        IEnumerable<string?> combinedNames = Enumerable.Empty<string>().Append(Firstnames).Append(Suffix).Append(Lastname)
                                                       .Where(x => !string.IsNullOrWhiteSpace(x));
        return string.Join(' ', combinedNames);
    }
}