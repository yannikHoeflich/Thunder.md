namespace Thunder.Md.PdfElements.Canvas.Indexes;

using QuestPDF.Infrastructure;
using Thunder.Md.Building;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;
using Thunder.Md.InternalExtensions;

public class TableIndex: IndexCanvas{
    protected override IReadOnlyCollection<ThunderIndexItem> GetItems(ThunderConfig config, IThunderBuildState state, IContainer container) => state.OfGroup(ThunderBuildState.TableGroup);
}