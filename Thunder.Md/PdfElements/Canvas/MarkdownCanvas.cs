namespace Thunder.Md.PdfElements.Canvas;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Thunder.Md.CodeExtensions;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;
using Thunder.Md.Readers;
using Thunder.Md.Readers.Markdown;

public class MarkdownCanvas: ICanvasElement{
    private readonly string _filePath;
    private List<IPdfElement>? _elements;
    public MarkdownCanvas(string filePath){
        _filePath = filePath;
    }
    public void Draw(ThunderConfig config, IThunderBuildState state, IContainer container){
        if(_elements is null){
            throw new UnreachableException();
        }
        
        container.Column(columnsHandler => {
            columnsHandler.Spacing(20);
            foreach(IPdfElement element in _elements){
                element.Draw(config, state, columnsHandler.Item());
            }
        });
    }

    public void Prebuild(ThunderConfig config, IThunderBuildState state){
        MarkdownReader markdownReader = new(_filePath, config, Program.ExtensionLoader!, Program.CreateLogger<MarkdownReader>());
        _elements = markdownReader.Read().ToList();
        foreach(IPdfElement element in _elements){
            element.Prebuild(config, state);
        }
    }

    public static bool Create(ExtensionArgs args, string url, ITextElement? __altText, string? __label, Dictionary<string, string?> __parameters, [NotNullWhen(true)] out ICanvasElement? canvasElement){
        string fullPath = Path.Combine(ThunderPaths.Source, url);

        canvasElement = new MarkdownCanvas(fullPath);
        return true;
    }
}