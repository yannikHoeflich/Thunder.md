namespace Thunder.Md.PdfElements.Canvas.Indexes;

using QuestPDF.Infrastructure;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;

public class SectionIndex: IndexCanvas{
    protected override IReadOnlyCollection<ThunderIndexItem> GetItems(ThunderConfig config, IThunderBuildState state, IContainer container) => state.SectionItems;
    protected override bool IsTableOfContent => true;
}