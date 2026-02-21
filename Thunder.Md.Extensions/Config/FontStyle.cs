namespace Thunder.Md.Extensions.Config;

using System.Diagnostics;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public record struct FontStyle(
    float? SizeMultiplier = null,
    bool? Bold = null,
    bool? Italic = null,
    bool? Underline = null,
    bool? Striketrough = null,
    ThunderColor? Color = null){
    public FontStyle Combine(FontStyle addedStyle){
        var style = this;
        if(addedStyle.SizeMultiplier.HasValue){
            style.SizeMultiplier = addedStyle.SizeMultiplier.Value;
        }

        if(addedStyle.Bold.HasValue){
            style.Bold = addedStyle.Bold.Value;
        }

        if(addedStyle.Italic.HasValue){
            style.Italic = addedStyle.Italic.Value;
        }

        if(addedStyle.Underline.HasValue){
            style.Underline = addedStyle.Underline.Value;
        }

        if(addedStyle.Striketrough.HasValue){
            style.Striketrough = addedStyle.Striketrough.Value;
        }

        if(addedStyle.Color.HasValue){
            style.Color = addedStyle.Color.Value;
        }

        return style;
    }

    public static FontStyle Combine(IEnumerable<FontStyle> styles){
        return styles.Aggregate((current, styleItem) => current.Combine(styleItem));
    }

    public TextStyle ToPdfStyle(ThunderConfig config){
        if(config.Project is null){
            throw new UnreachableException();
        }

        TextStyle style = new TextStyle().FontColor(ToPdfColor(Color ?? config.Project.TextColor))
                                         .FontSize(config.Project.FontSize * (SizeMultiplier ?? 1));
        
        if(Bold??false){
            style = style.Bold();
        }

        if(Italic ?? false){
            style = style.Italic();
        }

        if(Underline ?? false){
            style = style.Underline();
        }

        if(Striketrough ?? false){
            style = style.Strikethrough();
        }

        return style;
    }

    private Color ToPdfColor(ThunderColor color){
        return QuestPDF.Infrastructure.Color.FromARGB(color.A, color.R, color.G, color.B);
    }
}