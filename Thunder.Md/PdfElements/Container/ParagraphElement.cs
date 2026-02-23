namespace Thunder.Md.PdfElements.Container;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;

public class ParagraphElement: IPdfElement{
    private readonly ITextElement _textElement;
    
    public ParagraphElement(ITextElement textElement){
        _textElement = textElement;
    }

    public void Draw(ThunderConfig config, ThunderBuildState state, IContainer container){
        container.Lazy(container => {
            container.SemanticParagraph().Text(t => {
                switch(config.Project!.TextAlignment){
                    case Alignment.Left:
                        t.AlignLeft();
                    break;
                    case Alignment.Right:
                        t.AlignRight();
                    break;
                    case Alignment.Middle:
                        t.AlignCenter();
                    break;
                    case Alignment.Justify:
                        t.Justify();
                    break;
                }
                _textElement.Draw(t, new FontStyle(), state, config);
            });
        });
    }
}