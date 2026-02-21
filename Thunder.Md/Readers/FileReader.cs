namespace Thunder.Md.Readers;

using Microsoft.Extensions.Logging;

public class FileReader{
    private record struct FilePoint(int Index, int Line, int Column );
    public string Path{ get; }
    public int Line{ get; private set; } = 1;
    public int Column{ get; private set; } = 1;

    private int _index = 0;
    private string? _content = null;
    
    private readonly Stack<FilePoint> _savePoints = new();

    public FileReader(string path){
        Path = path;
    }

    public bool TryInit(){
        if(!File.Exists(Path)){
            return false;
        }

        try{
            _content = File.ReadAllText(Path);
        } catch{
            return false;
        }

        return true;
    }

    public bool TryGetNext(out char value){
        if(_content is null || _index >= _content.Length){
            value ='\0';
            return false;
        }

        value = _content[_index];
        _index++;
        Column++;
        if(value == '\n'){
            Line++;
            Column = 1;
        }
        
        return true;
    }

    public void Save(int delta = 0){
        _savePoints.Push(new FilePoint(_index + delta, Line, Column));
    }
    
    public bool TryRecall(){
        if(_savePoints.Count < 1){
            return false;
        }

        FilePoint savePoint = _savePoints.Pop();
        _index = savePoint.Index;
        Line  = savePoint.Line;
        Column = savePoint.Column;
        return true;
    }

    public void Rebase(){
        _savePoints.Clear();
    }

    public void DeleteLastSave(){
        _savePoints.TryPop(out _);
    }
}