namespace Thunder.Md.Extensions.PdfElements;

using Thunder.Md.Extensions.Config;

public interface ICodeFormatter{
    public bool Supports(string languageId);
    public IEnumerable<ITextElement> Format(string code, ThunderConfig config);
}