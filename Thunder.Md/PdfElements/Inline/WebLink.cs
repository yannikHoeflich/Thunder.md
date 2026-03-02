namespace Thunder.Md.PdfElements.Inline;

using QuestPDF.Fluent;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;

public class WebLink: ITextElement{
    public string Text => _text.Text;

    private readonly ITextElement _text;

    private readonly string _url;
    
    public WebLink(ITextElement text, string url){
        _text = text;
        _url = url;
    }
    
    public void Draw(TextDescriptor text, FontStyle fontStyle, IThunderBuildState state, ThunderConfig config){
        fontStyle = fontStyle.Combine(new FontStyle(Color: config.Project!.Colors[0]));
        text.Hyperlink(Text, _url).Style(fontStyle.ToPdfStyle(config));
    }
    
    public void Prebuild(ThunderConfig config, IThunderBuildState state){ }
}