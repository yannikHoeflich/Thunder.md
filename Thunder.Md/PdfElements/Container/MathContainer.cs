namespace Thunder.Md.PdfElements.Container;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;
using Thunder.Md.PdfElements.Inline;
using Thunder.Md.ThunderMath;

public class MathContainer: IPdfElement{
    public string Text{ get; }
    public MathContainer(string text){
        Text = text;
    }
    
    public void Draw(ThunderConfig config, ThunderBuildState state, IContainer container){
        LatexMathString latexString = new(Text);
        string svg = latexString.ToSvg();

        var indexItem = state.GetNextMathName();
        TextWrapper nameWrapper = new(config.Project!.NumberingStyle);
        nameWrapper.Add(new PureTextElement(indexItem.Id + " "));

        container.SemanticFormula(Text).Section(indexItem.LabelId).Height(config.Project!.FontSize * 1.2f).Layers(layer => {
            if(config.Project.MathNumbering.Used){
                IContainer textContainer = config.Project.MathNumbering.Alignment == Alignment.Left 
                    ? layer.Layer().AlignLeft() 
                    : layer.Layer().AlignRight();
                textContainer.AlignMiddle().Text(text => {
                    nameWrapper.Draw(text, new FontStyle(), state, config);
                });
            }
            layer.PrimaryLayer().AlignCenter().AlignMiddle().Height(config.Project!.FontSize * 1.1f).Svg(svg);
        });
    }
}