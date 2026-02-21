namespace Thunder.Md.CodeExtensions;

using QuestPDF.Infrastructure;

public class MockContainer: IContainer{
    public IElement? Child{ get; set; }
}