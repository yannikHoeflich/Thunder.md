namespace Thunder.Md.Readers.Markdown;

using System.Diagnostics.CodeAnalysis;
using Thunder.Md.PdfElements.Container;

public partial class MarkdownReader{
    private bool TryReadCodeBlock([NotNullWhen(true)]out CodeContainer? codeContainer){
        for(int i = 0; i < 2; i++){
            if(!_fileReader.TryGetNext(out char c) || c != '`'){
                codeContainer = null;
                return false;
            }
        }
        
        if(!TryReadTextNotFormatted([new EndChar('\n', 1)], EndLineManagement.Ignore, false, null, out string? languageId,
                                    out _) || !TryReadTextNotFormatted([new EndChar('`', 3)], EndLineManagement.Insert, false, null, out string? code,
                                                                       out _)){
            
            codeContainer = null;
            return false;
        }

        codeContainer = new CodeContainer(languageId, code);
        return true;
    }
}