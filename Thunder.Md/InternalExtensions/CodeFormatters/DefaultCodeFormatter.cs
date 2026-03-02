namespace Thunder.Md.InternalExtensions.CodeFormatters;

using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;

public class DefaultCodeFormatter: ICodeFormatter{
    public bool Supports(string languageId) => true;

    public IEnumerable<ITextElement> Format(string code, ThunderConfig config){
        yield return new PureTextElement(code);
    }
}