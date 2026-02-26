namespace Thunder.Md.PdfElements.Container;

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;
using Thunder.Md.PdfElements.Inline;

public class HeadlineElement: IPdfElement{
    private readonly ILogger _logger;
    public HeadlineElement(int layer, bool indexed, ITextElement text, ILogger logger){
        Layer = layer;
        Indexed = indexed;
        TextElement = text;
        _logger = logger;
    }
    public ITextElement TextElement { get; }
    public string Text => TextElement.Text;
    public int Layer{ get; }
    public bool Indexed{ get; }

    private ThunderIndexItem? _indexItem;
    public void Draw(ThunderConfig config, IThunderBuildState state, IContainer container){
        if(config.Project!.HeadlineSizes.Length == 0){
            config.Project!.HeadlineSizes = [2];
            _logger.LogWarning("Invalid headline sizes, using (text size)*2 as default");
        }
        
        container = container.SemanticSection();
        if(Layer > 0 && Layer <= 6){
            container = Layer switch{
                            1 => container.SemanticHeader1(),
                            2 => container.SemanticHeader2(),
                            3 => container.SemanticHeader3(),
                            4 => container.SemanticHeader4(),
                            5 => container.SemanticHeader5(),
                            6 => container.SemanticHeader6(),
                            _ => throw new UnreachableException()
                        };
        }
        
        float size = Layer < config.Project!.HeadlineSizes.Length
            ? config.Project!.HeadlineSizes[Layer]
            : config.Project!.HeadlineSizes[^1];

        bool bold = size <= 1.11;
        
        FontStyle fontStyle = new(SizeMultiplier: size, Bold: bold);
        TextWrapper textWrapper = new(fontStyle);
        if(Indexed && _indexItem is not null){
            textWrapper.Add(new PureTextElement(_indexItem.ReferenceText + " "));
            container = container.Section(_indexItem.SectionId);
        }
        textWrapper.Add(TextElement);
        /*if(Layer <= config.Project!.MaxLayerForOwnPage){
            container.PageBreak();
        }*/
        
        
        container.Text(t => {
            textWrapper.Draw(t, fontStyle, state, config);
        });
    }

    public void Prebuild(ThunderConfig config, IThunderBuildState state){
        if(Indexed){
            _indexItem = state.NextSectionId(Layer, Text);
        }
        TextElement.Prebuild(config, state);
    }
}