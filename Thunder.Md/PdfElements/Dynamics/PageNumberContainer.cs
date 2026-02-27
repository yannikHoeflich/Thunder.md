namespace Thunder.Md.PdfElements.Dynamics;

using QuestPDF.Elements;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Thunder.Md.Building;

public class PageNumberContainer: IDynamicComponent{
    private readonly ThunderBuildState _state;

    public PageNumberContainer(ThunderBuildState state){
        _state = state;
    }

    public DynamicComponentComposeResult Compose(DynamicContext context){
        IDynamicElement content = context.CreateElement(element => {
            element
                .Element(x => context.PageNumber % 2 == 0
                             ? x.AlignRight()
                             : x.AlignLeft())
                .Text(text => {
                    text.Span("Page ");
                    string page = _state.GetPageNumber(context.PageNumber,
                                                       str => GetPage(str, context));
                    text.Span(page);
                });
        });

        return new DynamicComponentComposeResult{
                                                    Content = content,
                                                    HasMoreContent = false
                                                };
    }

    private int GetPage(string id, DynamicContext context){
        var position = context.GetContentCapturedPositions(id);
        if(position.Count < 1){
            return 0;
        }

        return position.First().PageNumber;
    }
}