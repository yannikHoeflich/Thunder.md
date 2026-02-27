namespace Thunder.Md.PdfElements.Container;

using System.Diagnostics;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Thunder.Md.Building;
using Thunder.Md.CodeExtensions;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;
using Thunder.Md.InternalExtensions;
using Thunder.Md.PdfElements.Inline;
using Thunder.Md.ThunderMath;

public class MathContainer: IPdfElement{
    private readonly string? _referenceId;
    public string Text{ get; }
    private ThunderIndexItem? _indexItem;

    public MathContainer(string text, string? referenceId){
        _referenceId = referenceId;
        Text = text;
    }

    public void Draw(ThunderConfig config, IThunderBuildState state, IContainer container){
        if(_indexItem is null){
            throw new UnreachableException();
        }

        LatexMathString latexString = new(Text);
        string svg = latexString.ToSvg();

        TextWrapper nameWrapper = new(config.Project!.NumberingStyle);
        nameWrapper.Add(new PureTextElement(_indexItem.ReferenceText));

        container.SemanticFormula(Text).Section(_indexItem.SectionId).Height(config.Project!.FontSize * 1.2f)
                 .Layers(layer => {
                     if(config.Project!.MathNumbering is not null){
                         IContainer textContainer = config.Project.MathNumbering!.Alignment == Alignment.Left
                             ? layer.Layer().AlignLeft()
                             : layer.Layer().AlignRight();
                         textContainer.AlignMiddle().Text(text => {
                             nameWrapper.Draw(text, new FontStyle(), state, config);
                         });
                     }

                     layer.PrimaryLayer().AlignCenter().AlignMiddle().Height(config.Project!.FontSize * 1.1f).Svg(svg);
                 });
    }

    public void Prebuild(ThunderConfig config, IThunderBuildState state){
        _indexItem = state.GetNextIndexItem(ThunderBuildState.MathGroup, "", _referenceId);
    }
}