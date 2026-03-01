namespace Thunder.Md.Extensions.PdfElements;

public interface ICodeFormatter{
    public bool Supports(string languageId);
    public IEnumerable<ITextElement> Format(string code);
}