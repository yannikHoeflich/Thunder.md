namespace Thunder.Md.PdfElements.Canvas.Indexes;

using QuestPDF.Infrastructure;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;

public class TableIndex: IndexCanvas{
    protected override IReadOnlyCollection<ThunderIndexItem> GetItems(ThunderConfig config, ThunderBuildState state, IContainer container) => state.TableItems;
}