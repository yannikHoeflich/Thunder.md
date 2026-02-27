namespace Thunder.Md.PdfElements.Canvas;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Thunder.Md.Building;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;

public class PageNumberingCanvas: ICanvasElement{
    private int _pageNumber;
    private ThunderIndexItem? _pageAnchor;
    
    public PageNumberingCanvas(int pageNumber){
        _pageNumber = pageNumber;
    }

    public void Draw(ThunderConfig config, IThunderBuildState state, IContainer container){
        if(_pageAnchor is null){
            throw new UnreachableException();
        }

        container.CaptureContentPosition(_pageAnchor.SectionId).Section(_pageAnchor.SectionId).SemanticIgnore();
    }
    
    public void Prebuild(ThunderConfig config, IThunderBuildState state){
        _pageAnchor = state.GetNextIndexItem(ThunderBuildState.NumberingGroup, "", null);
        NumberingStyle style = NumberingStyle.Numeric;
        state.AddPageNumberAnchor(_pageAnchor.SectionId, _pageNumber, style);
    }
    
    public static bool Create(ExtensionArgs args, string url, ITextElement? altText, string? __label, Dictionary<string, string?> parameters, [NotNullWhen(true)] out ICanvasElement? canvasElement){
        if(altText is null || !int.TryParse(altText.Text, out int pagenumber)){
            pagenumber = 1;
        }
        
        canvasElement = new PageNumberingCanvas(pagenumber);
        return true;
    }
}