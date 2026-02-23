namespace Thunder.Md.Readers.Markdown;

using System.Diagnostics.CodeAnalysis;
using Thunder.Md.PdfElements.Container;

public partial class MarkdownReader{
    private bool TryReadMathContainer([NotNullWhen(true)]out MathContainer? mathContainer){
        if(!_fileReader.TryGetNext(out char c)
        || c != '$'
        || !TryReadTextNotFormatted([new('$', 2)], EndLineManagement.Ignore, false, null, out string? mathStr,
                                    out _)){
            mathContainer = null;
            return false;
        }

        TryReadReference(out string? referenceId);
        
        mathContainer = new MathContainer(mathStr, referenceId);
        return true;
    }
}