namespace Thunder.Md.Readers.Markdown;

using System.Diagnostics.CodeAnalysis;

public partial class MarkdownReader{
    public bool TryReadReference([NotNullWhen(true)]out string? referenceId){
        _fileReader.Save();
        if(!_fileReader.TryGetNext(out char c) 
        || c != '{' 
        || !TryReadTextNotFormatted([new EndChar('}', 1)], EndLineManagement.Error, false, null, out referenceId,
               out _)){
            RecallOrErrorPop();
            referenceId = null;
            return false;
        }

        _fileReader.DeleteLastSave();
        return true;
    }
}