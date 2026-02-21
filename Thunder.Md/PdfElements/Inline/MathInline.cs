namespace Thunder.Md.PdfElements.Inline;

using QuestPDF.Fluent;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;
using Thunder.Md.ThunderMath;

public class MathInline:ITextElement{
    public string Text{ get; }
    public MathInline(string text){
        Text = text;
    }
    public void Draw(TextDescriptor text, FontStyle fontStyle, ThunderBuildState state, ThunderConfig config){
        var latexString = new LatexMathString(Text);
        var svg = latexString.ToSvg();
        text.Element().PaddingBottom(config.Project!.FontSize * -0.2f).Height(config.Project!.FontSize * (fontStyle.SizeMultiplier??1) * 1.1f).Svg(svg);
    }
}