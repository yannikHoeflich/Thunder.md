namespace Thunder.Md.PdfElements.Inline;

using QuestPDF.Fluent;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;

public class TextWrapper: ITextElement{
    private readonly List<ITextElement> _textElements = [];
    public string Text => string.Concat(_textElements.Select(x => x.Text));
    public FontStyle FontStyle{ get; set; }
    
    public TextWrapper(FontStyle fontStyle){
        FontStyle = fontStyle;
    }
    
    public void Draw(TextDescriptor text, FontStyle fontStyle, ThunderBuildState state, ThunderConfig config){
        fontStyle = fontStyle.Combine(FontStyle);
        foreach(var textElement in _textElements){
            textElement.Draw(text, fontStyle, state, config);
        }
    }

    public void Add(ITextElement textElement){
        _textElements.Add(textElement);
    }
}