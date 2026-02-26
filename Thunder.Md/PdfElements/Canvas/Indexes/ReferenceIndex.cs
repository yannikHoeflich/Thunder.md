namespace Thunder.Md.PdfElements.Canvas.Indexes;

using System.Diagnostics;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Thunder.Md.Extensions.Cite;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;

public class ReferenceIndex: IndexCanvas{
    private readonly HashSet<string> _filter = new(StringComparer.OrdinalIgnoreCase);

    public ReferenceIndex(string? filter){
        if(string.IsNullOrWhiteSpace(filter)){
            return;
        }

        foreach(Range filterPart in filter.SplitAny(',', ';', ' ')){
            if(filterPart.End.Value - filterPart.Start.Value < 1){
                continue;
            }
            _filter.Add(filter[filterPart]);
        }
    }

    protected override IReadOnlyCollection<ThunderIndexItem> GetItems(ThunderConfig config, IThunderBuildState state,
                                                                      IContainer container){
        throw new UnreachableException();
    }

    public override void Draw(ThunderConfig config, IThunderBuildState state, IContainer container){
        container.Lazy(container => {
            container.Table(table => {
                var publications = state.Publications.Where(Filter).ToArray();
                table.ColumnsDefinition(columns => {
                    int maxIdLength = publications.Max(x => x.IndexItem.ReferenceText.Length);
                    float idWidth = maxIdLength * config.Project!.FontSize;
                    
                    columns.ConstantColumn(idWidth);
                    columns.RelativeColumn();
                });
                
                foreach((ThunderPublication publication, ThunderIndexItem indexItem) in publications){
                    table.Cell().Section(publication.Label).PaddingVertical(config.Project!.FontSize * 0.2f).Text(indexItem.ReferenceText);
                    table.Cell().PaddingVertical(config.Project!.FontSize * 0.2f).Text(publication.ToString());
                }
                
            });
            
        });
    }

    private bool Filter((ThunderPublication Publication, ThunderIndexItem Index) arg){
        if(_filter.Count == 0){
            return true;
        }
        return _filter.Contains(arg.Publication.Type);
    }
}