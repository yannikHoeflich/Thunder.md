namespace Thunder.Md.CodeExtensions;

using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;
using PdfColor = QuestPDF.Infrastructure.Color;

public static class ThunderColorExtensions{
    extension(ThunderColor color){
        public PdfColor ToPdfColor(){
            return PdfColor.FromARGB(color.A, color.R, color.G, color.B);
        }
    }
}