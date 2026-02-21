namespace Thunder.Md.Readers;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Logging;
using Thunder.Md.CodeExtensions;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Cite;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;
using Thunder.Md.PdfElements;
using Thunder.Md.PdfElements.Container;
using Thunder.Md.PdfElements.Inline;

public class MarkdownReader{
    private readonly FileReader _fileReader;
    private readonly ThunderConfig _config;
    private readonly ExtensionLoader _extensionLoader;
    private readonly ILogger _logger;
    private bool _shouldRun = true;

    public MarkdownReader(string path, ThunderConfig config, ExtensionLoader extensionLoader, ILogger logger){
        _config = config;
        _extensionLoader = extensionLoader;
        _logger = logger;
        _fileReader = new FileReader(path);
    }

    public IEnumerable<IPdfElement> Read(){
        if(!_fileReader.TryInit()){
            _logger.LogError("Cannot read file '{FilePath}'", _fileReader.Path);
            yield break;
        }

        _shouldRun = true;
        while(_fileReader.TryGetNext(out char c) && _shouldRun){
            _fileReader.Save();
            if(char.IsWhiteSpace(c)){
                continue;
            }

            if(c == '#'){
                if(TryReadHeadline(out IPdfElement? headline)){
                    yield return headline;
                    continue;
                }

                RecallOrError();
            }

            if(c == '!'){
                if(TryReadCanvas(out ICanvasElement? canvas)){
                    yield return canvas;
                    continue;
                }

                RecallOrError();
            }

            if(c == '$'){
                if(_fileReader.TryGetNext(out char tempC)
                && tempC == '$'
                && TryReadTextNotFormatted([new('$', 2)], EndLineManagement.Ignore, false, null, out string? mathStr)
                  ){
                    yield return new MathContainer(mathStr);
                    continue;
                }

                RecallOrError();
            }

            if(!TryReadText([new EndChar('\n', 2)], EndLineManagement.ToSpace, true, c, out TextWrapper? paragraph)){
                RecallOrError();
                _logger.LogError(_fileReader, "Can not read paragraph");
                continue;
            }

            yield return new ParagraphElement(paragraph);
        }

        if(!_shouldRun){
            Program.Error = true;
        }
    }

    private bool TryReadHeadline([NotNullWhen(true)] out IPdfElement? pdfElement){
        int layer = 1;
        bool stayedInFile;
        char c;
        while((stayedInFile = _fileReader.TryGetNext(out c)) && _shouldRun && c == '#'){
            layer++;
        }

        if(!stayedInFile){
            _logger.LogWarning(_fileReader, "Reached unexpected end of file. With start of a headline");
            pdfElement = null;
            return false;
        }

        bool indexed = c != '*';
        if(!indexed && !_fileReader.TryGetNext(out c)){
            _logger.LogWarning(_fileReader, "Reached unexpected end of file. With start of a headline");
            pdfElement = null;
            return false;
        }

        if(!char.IsWhiteSpace(c) || !TryReadText([new EndChar('\n', 1)], EndLineManagement.Ignore, true, null,
                                                 out TextWrapper? textElement)){
            pdfElement = null;
            return false;
        }

        pdfElement = new HeadlineElement(layer, indexed, textElement, Program.CreateLogger<HeadlineElement>());
        return true;
    }

    private record struct EndChar(char Char, int RequiredCount);

    private enum EndLineManagement{
        Ignore,
        ToSpace,
        Insert,
        Error
    }

    private bool TryReadText(EndChar[] endChars, EndLineManagement endLineManagement, bool reduceSpaces,
                             char? firstChar,
                             [NotNullWhen(true)] out TextWrapper? result){
        if(endChars.Length == 0){
            throw new ArgumentException("endChars must not be empty", nameof(endChars));
        }

        int endCharCount = 0;
        char currentEndChar = endChars[0].Char;
        StringBuilder text = new();
        result = new TextWrapper(new FontStyle());
        bool isInSpace = false;
        char c = '\0';
        while(endCharCount < endChars.Where(x => x.Char == currentEndChar).Min(x => x.RequiredCount)
           && (firstChar.HasValue || _fileReader.TryGetNext(out c))
           && _shouldRun
             ){
            if(firstChar.HasValue){
                c = firstChar.Value;
                firstChar = null;
            }

            if(c == '\0'){
                _logger.LogError(_fileReader, "Invalid '\0' character in file");
                return false;
            }

            if(endChars.Any(x => x.Char == c)){
                if(endCharCount == 0){
                    _fileReader.Save(-1);
                }

                if(currentEndChar != c){
                    currentEndChar = c;
                    endCharCount = 0;
                }

                endCharCount++;
                continue;
            }

            if(char.IsWhiteSpace(c) && endCharCount > 0){
                continue;
            }

            if(endCharCount > 0){
                endCharCount = 0;
                RecallOrErrorPop();
                _fileReader.TryGetNext(out c);
            }

            if(c == '\n'){
                switch(endLineManagement){
                    case EndLineManagement.Ignore:
                        // Ignore
                    break;
                    case EndLineManagement.Insert:
                        text.Append('\n');
                    break;
                    case EndLineManagement.ToSpace:
                        if(!isInSpace || !reduceSpaces){
                            text.Append(' ');
                        }

                        isInSpace = true;
                    break;
                    case EndLineManagement.Error:
                        return false;
                    default:
                        throw new UnreachableException();
                }

                continue;
            }

            if(char.IsWhiteSpace(c)){
                if(!isInSpace || !reduceSpaces){
                    text.Append(' ');
                }

                isInSpace = true;
                continue;
            }

            isInSpace = false;

            if(c == '*'){
                PureTextElement textElement = new(text.ToString());
                result.Add(textElement);
                text.Clear();
                _fileReader.Save();
                bool bold = false;
                if(!_fileReader.TryGetNext(out char tempC)){
                    text.Append(c);
                    continue;
                }

                bold = tempC == '*';
                if(!bold){
                    RecallOrErrorPop();
                }

                EndChar[] newEndChars = [..endChars, new('*', bold ? 2 : 1)];
                if(!TryReadText(newEndChars, endLineManagement, reduceSpaces, null, out TextWrapper? innerTextWrapper)){
                    result = null;
                    return false;
                }

                if(bold){
                    innerTextWrapper.FontStyle = innerTextWrapper.FontStyle with{ Bold = true };
                } else{
                    innerTextWrapper.FontStyle = innerTextWrapper.FontStyle with{ Italic = true };
                }

                result.Add(innerTextWrapper);
                continue;
            }

            if(c == '_'){
                PureTextElement textElement = new(text.ToString());
                result.Add(textElement);
                text.Clear();

                _fileReader.Save();
                if(!_fileReader.TryGetNext(out char tempC) || tempC != '_'){
                    RecallOrErrorPop();
                    text.Append(c);
                    continue;
                }


                EndChar[] newEndChars = [..endChars, new('_', 2)];
                if(!TryReadText(newEndChars, endLineManagement, reduceSpaces, null, out TextWrapper? innerTextWrapper)){
                    result = null;
                    return false;
                }

                innerTextWrapper.FontStyle = innerTextWrapper.FontStyle with{ Underline = true };

                result.Add(innerTextWrapper);
                continue;
            }

            if(c == '-'){
                PureTextElement textElement = new(text.ToString());
                result.Add(textElement);
                text.Clear();

                _fileReader.Save();
                if(!_fileReader.TryGetNext(out char tempC) || tempC != '-'){
                    RecallOrErrorPop();
                    text.Append(c);
                    continue;
                }


                EndChar[] newEndChars = [..endChars, new('-', 2)];
                if(!TryReadText(newEndChars, endLineManagement, reduceSpaces, null, out TextWrapper? innerTextWrapper)){
                    result = null;
                    return false;
                }

                innerTextWrapper.FontStyle = innerTextWrapper.FontStyle with{ Striketrough = true };

                result.Add(innerTextWrapper);
                continue;
            }

            if(c == '$'){
                PureTextElement textElement = new(text.ToString());
                result.Add(textElement);
                text.Clear();

                _fileReader.Save();
                if(!_fileReader.TryGetNext(out char tempC) || tempC != '$'){
                    RecallOrErrorPop();
                    text.Append(c);
                    continue;
                }


                EndChar[] newEndChars = [..endChars, new('$', 2)];
                if(!TryReadTextNotFormatted(newEndChars, endLineManagement, reduceSpaces, null, out string? mathStr)){
                    result = null;
                    return false;
                }


                result.Add(new MathInline(mathStr));
                continue;
            }

            if(c == '@'){
                PureTextElement textElement = new(text.ToString());
                result.Add(textElement);
                text.Clear();
                
                if(TryReadCitation(out CitationElement? citation)){
                    result.Add(citation);
                    continue;
                }
                
                RecallOrErrorPop();
                text.Append(c);
            }


            text.Append(c);
        }

        string remainderText = text.ToString();
        if(!string.IsNullOrWhiteSpace(remainderText)){
            PureTextElement remainderTextElement = new(remainderText);
            result.Add(remainderTextElement);
        }

        _fileReader.DeleteLastSave();

        return true;
    }

    private bool TryReadCitation([NotNullWhen(true)]out CitationElement? citationElement){
        StringBuilder label = new();
        _fileReader.Save();
        while(_fileReader.TryGetNext(out char c) && (char.IsLetterOrDigit(c) || c == '-' || c == '_')){
            _fileReader.DeleteLastSave();
            _fileReader.Save();
            label.Append(c);
        }
        
        RecallOrErrorPop();

        if(label.Length == 0){
            citationElement = null;
            return false;
        }
        
        citationElement = new CitationElement(label.ToString());
        return true;
    }

    private bool TryReadCanvas([NotNullWhen(true)] out ICanvasElement? canvasElement){
        if(!_fileReader.TryGetNext(out char c) || c != '['){
            canvasElement = null;
            return false;
        }

        bool isDirect = false;
        if(!_fileReader.TryGetNext(out c)){
            canvasElement = null;
            return false;
        }

        ITextElement? altTextElement = null;
        isDirect = c == '[';
        if(!isDirect){
            if(!TryReadText([new EndChar(']', 1)], EndLineManagement.Error, true, c,
                            out TextWrapper? innerTextWrapper)){
                canvasElement = null;
                return false;
            }

            altTextElement = innerTextWrapper;


            if(!_fileReader.TryGetNext(out c) || c != '('){
                canvasElement = null;
                return false;
            }
        }

        if(!TryReadTextNotFormatted([new EndChar(isDirect ? ']' : ')', isDirect ? 2 : 1)], EndLineManagement.Error,
                                    true, null,
                                    out string? file)){
            canvasElement = null;
            return false;
        }

        if(!_extensionLoader.TryGetCanvasElement(file, altTextElement, _config, out canvasElement)){
            _logger.LogWarning("Cannot generate canvas for file '{file}'. Are you missing to import an extension?",
                               file);
            canvasElement = null;
            return false;
        }

        return true;
    }

    private bool TryReadTextNotFormatted(EndChar[] endChars, EndLineManagement endLineManagement, bool reduceSpaces,
                                         char? firstChar, [NotNullWhen(true)] out string? value){
        if(endChars.Length == 0){
            throw new ArgumentException("endChars must not be empty", nameof(endChars));
        }

        int endCharCount = 0;
        char currentEndChar = endChars[0].Char;
        StringBuilder text = new();
        bool isInSpace = false;
        char c = '\0';
        while(endCharCount < endChars.Where(x => x.Char == currentEndChar).Min(x => x.RequiredCount)
           && (firstChar.HasValue || _fileReader.TryGetNext(out c))
           && _shouldRun
             ){
            if(firstChar.HasValue){
                c = firstChar.Value;
                firstChar = null;
            }

            if(c == '\0'){
                _logger.LogError(_fileReader, "Invalid '\0' character in file");
                value = null;
                return false;
            }

            if(endChars.Any(x => x.Char == c)){
                if(endCharCount == 0){
                    _fileReader.Save(-1);
                }

                if(currentEndChar != c){
                    currentEndChar = c;
                    endCharCount = 0;
                }

                endCharCount++;
                continue;
            }

            if(char.IsWhiteSpace(c) && endCharCount > 0){
                continue;
            }

            if(endCharCount > 0){
                endCharCount = 0;
                RecallOrErrorPop();
                _fileReader.TryGetNext(out c);
            }

            if(c == '\n'){
                switch(endLineManagement){
                    case EndLineManagement.Ignore:
                        // Ignore
                    break;
                    case EndLineManagement.Insert:
                        text.Append('\n');
                    break;
                    case EndLineManagement.ToSpace:
                        if(!isInSpace || !reduceSpaces){
                            text.Append(' ');
                        }

                        isInSpace = true;
                    break;
                    case EndLineManagement.Error:
                        value = null;
                        return false;
                    default:
                        throw new UnreachableException();
                }

                continue;
            }

            if(char.IsWhiteSpace(c)){
                if(!isInSpace || !reduceSpaces){
                    text.Append(' ');
                }

                isInSpace = true;
                continue;
            }

            isInSpace = false;

            text.Append(c);
        }

        value = text.ToString();

        _fileReader.DeleteLastSave();

        return true;
    }

    private void RecallOrError(){
        RecallOrErrorPop();
        _fileReader.Save();
    }

    private void RecallOrErrorPop(){
        bool value = _fileReader.TryRecall();
        _shouldRun = value;
        if(value){
            return;
        }

        _logger.LogError("Cannot recall file point '{FilePath}'", _fileReader.Path);
    }
}