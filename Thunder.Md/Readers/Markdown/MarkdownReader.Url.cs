namespace Thunder.Md.Readers.Markdown;

using System.Diagnostics.CodeAnalysis;
using Thunder.Md.CodeExtensions;
using Thunder.Md.Extensions.PdfElements;
using Thunder.Md.PdfElements.Inline;

public partial class MarkdownReader{
    private bool TryReadLink([NotNullWhen(true)]out WebLink? webLink){
        _fileReader.Save();
        if(!_fileReader.TryGetNext(out char c)){
            webLink = null;
        }

        ITextElement? text = null;
        bool isDirect = c == '[';
        
        if(!isDirect){
            if(!TryReadText([new EndChar(']', 1)], EndLineManagement.Error, true, c,
                            out TextWrapper? innerTextWrapper, out _)){
                webLink = null;
                return false;
            }

            text = innerTextWrapper;


            if(!_fileReader.TryGetNext(out c) || c != '('){
                webLink = null;
                return false;
            }
        }

        if(!TryReadTextNotFormatted([new EndChar(isDirect ? ']' : ')', isDirect ? 2 : 1)], EndLineManagement.Error,
                                    true, null,
                                    out var url, out _) || !Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)){
            webLink = null;
            return false;
        }

        text ??= new PureTextElement(url);
        
        webLink = new WebLink(text, uri.AbsoluteUri);

        return true;
    }
}