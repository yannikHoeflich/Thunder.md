namespace Thunder.Md.Extensions.Config;

using Thunder.Md.Extensions.PdfElements;

public interface IPdfCollectionElement: IPdfElement{
    public IReadOnlyList<IPdfElement> Elements { get; }
    public void Add(IPdfElement element);
}