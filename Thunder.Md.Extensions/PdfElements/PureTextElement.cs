namespace Thunder.Md.Extensions.PdfElements;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Thunder.Md.Extensions.Config;

public class PureTextElement: ITextElement{
    public PureTextElement(string text){
        Text = text;
    }

    public string Text{ get; }

    public void Draw(TextDescriptor text, FontStyle fontStyle, ThunderBuildState state, ThunderConfig config){
        if(string.IsNullOrEmpty(Text)){
            return;
        }

        TextStyle textStyle = fontStyle.ToPdfStyle(config);
        text.Span(Text).Style(textStyle);
    }
}