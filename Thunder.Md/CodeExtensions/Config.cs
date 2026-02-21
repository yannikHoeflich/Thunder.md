namespace Thunder.Md.CodeExtensions;

using QuestPDF.Infrastructure;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;
using PdfSize = QuestPDF.Helpers.PageSize;

public static class Config{
    extension(ThunderProjectConfig config){
        public PdfSize GetPdfSize(){
            return new PdfSize(config.PaperWidth, config.PaperHeight, Unit.Millimetre);
        }
    }
}