namespace Thunder.Md.Writers;

using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Thunder.Md.Building;
using Thunder.Md.CodeExtensions;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Cite;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;
using Thunder.Md.InternalExtensions;
using Thunder.Md.PdfElements;
using Thunder.Md.Readers;

public class PdfBuilder{
    private readonly ILogger _logger;
    private readonly ThunderConfig _config;
    private readonly string _filePath;

    public PdfBuilder(ILogger logger, ThunderConfig config, string filePath){
        _logger = logger;
        _config = config;
        _filePath = filePath;
    }

    public void Write(IReadOnlyList<IPdfElement> elements){
        BibtexReader bibtexReader = new(_config);
        IReadOnlyCollection<ThunderPublication> publications = bibtexReader.Read();

        var setting = new DocumentSettings(){
                                                PDFUA_Conformance = PDFUA_Conformance.PDFUA_1
                                            };
                                var state = new ThunderBuildState(_config, publications);
        
                                foreach(IPdfElement element in elements){
                                    element.Prebuild(_config, state);
                                }

                                state.EndOfPrebuild();
                                
        Document.Create(container => {
                    container.Page(page => {
                        page.Size(_config.Project!.GetPdfSize());
                        page.Margin(_config.Project!.PageMargin, Unit.Millimetre);
                        page.PageColor(_config.Project!.BackgroundColor.ToPdfColor());
                        page.DefaultTextStyle(x => x.FontSize(_config.Project!.FontSize)
                                                    .FontColor(_config.Project!.TextColor.ToPdfColor())
                                                    .FontFamily(_config.Project!.FontFamily));

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(columnsHandler => {
                                columnsHandler.Spacing(20);
                                foreach(IPdfElement element in elements){
                                    element.Draw(_config, state, columnsHandler.Item());
                                }
                            });
                    });
                })
                .WithSettings(setting)
                .GeneratePdf(_filePath);
    }
}