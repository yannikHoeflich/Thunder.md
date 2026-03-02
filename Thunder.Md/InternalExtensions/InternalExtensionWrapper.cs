namespace Thunder.Md.InternalExtensions;

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;
using Thunder.Md.InternalExtensions.Commands;
using Thunder.Md.PdfElements.Canvas;
using Thunder.Md.PdfElements.Inline;

public class InternalExtensionWrapper(string srcPath, ILogger logger): ThunderExtension(srcPath, logger){
    public override string Id => "internal";

    public override IEnumerable<ICommand> GetCommands(){
        yield return new Build();
    }

    public override IEnumerable<CanvasCreator> GetCanvasCreators(){
        yield return new CanvasCreator(ThunderImage.Create, null, "jpg", "jpeg", "png", "gif", "webp");
        yield return new CanvasCreator(MarkdownCanvas.Create, null, "md");
        yield return new CanvasCreator(IndexCanvas.Create, null, "toc", "sections", "content", "figures", "graphics", "tables", "references", "citations", "math", "equations", "formulas");
        yield return new CanvasCreator(PageNumberingCanvas.Create, null, "page");
        
    }

    public override IEnumerable<InlineCanvasCreator> GetInlineCanvasCreators(){        
        yield return new InlineCanvasCreator(CreateUrl, "http");
        yield return new InlineCanvasCreator(CreateUrl, "https");

        
    }

    private bool CreateUrl(ExtensionArgs args, string url, ITextElement? altText, string? label, Dictionary<string, string?> parameters, [NotNullWhen(true)] out IInlineCanvasElement? inlineCanvas){
        if(!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)){
            inlineCanvas = null;
            return false;
        }

        if(altText is null){
            altText = new PureTextElement(url);
        }

        inlineCanvas = new WrapperInlineCanvas(new WebLink(altText, uri.AbsoluteUri));

        return true;
    }


    public override void PreCompile(ExtensionArgs args){
        
    }

    public override void PostCompile(ExtensionArgs args){
        
    }
}