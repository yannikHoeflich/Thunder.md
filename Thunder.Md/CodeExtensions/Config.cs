namespace Thunder.Md.CodeExtensions;

using QuestPDF.Infrastructure;
using Thunder.Md.Building;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;
using Thunder.Md.InternalExtensions;
using PdfSize = QuestPDF.Helpers.PageSize;

public static class Config{
    extension(ThunderProjectConfig config){
        public PdfSize GetPdfSize(){
            return new PdfSize(config.PaperWidth, config.PaperHeight, Unit.Millimetre);
        }

        public NumberingConfig? MathNumbering => config.Numberings.GetValueOrDefault(ThunderBuildState.MathGroup);
        public NumberingConfig? TableNumbering => config.Numberings.GetValueOrDefault(ThunderBuildState.TableGroup);
        public NumberingConfig? GraphicsNumbering => config.Numberings.GetValueOrDefault(ThunderBuildState.FigureGroup);
    }
}