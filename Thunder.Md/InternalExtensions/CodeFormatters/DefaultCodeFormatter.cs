namespace Thunder.Md.InternalExtensions.CodeFormatters;

using Thunder.Md.Extensions.PdfElements;

public class DefaultCodeFormatter: ICodeFormatter{
    public bool Supports(string languageId) => true;

    public IEnumerable<ITextElement> Format(string code){
        yield return new PureTextElement(code);
    }
}