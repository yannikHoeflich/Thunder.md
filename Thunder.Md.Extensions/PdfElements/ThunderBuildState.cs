namespace Thunder.Md.Extensions.PdfElements;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Thunder.Md.Extensions.Cite;
using Thunder.Md.Extensions.Config;

public record ThunderIndexItem(string Id, string Name, string LabelId);
public record ReferenceItem(string ReferenceText, string Link);
public class ThunderBuildState{
    private ThunderConfig _config;
    private List<int> _sectionIds = [];
    private int _graphicsCounter = 1;
    private int _mathCounter = 1;
    private int _tableCounter = 1;
    private List<ThunderIndexItem> _sectionItems = [];
    private List<ThunderIndexItem> _graphicsItems = [];
    private List<ThunderIndexItem> _mathItems = [];
    private List<ThunderIndexItem> _tableItems = [];
    private Dictionary<string, ThunderPublication> _publications = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string> _citationReferences = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, ReferenceItem> _elementReferences = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<(ThunderPublication Publication, string Label)> Publications =>
        _publications.Where(x => _citationReferences.ContainsKey(x.Key)).Select(x => (x.Value, _citationReferences[x.Key])).ToImmutableList();
    public IReadOnlyList<ThunderIndexItem> SectionItems => _sectionItems.AsReadOnly();
    public IReadOnlyList<ThunderIndexItem> GraphicsItems => _graphicsItems.AsReadOnly();
    public IReadOnlyList<ThunderIndexItem> MathItems => _mathItems.AsReadOnly();
    public IReadOnlyList<ThunderIndexItem> TableItems => _tableItems.AsReadOnly();
    
    public ThunderBuildState(ThunderConfig config, IReadOnlyCollection<ThunderPublication> publications){
        _config = config;
        foreach(ThunderPublication thunderPublication in publications){
            _publications.Add(thunderPublication.Label, thunderPublication);
        }
    }

    public ThunderIndexItem GetNextGraphicsName(string name, string? label){
        var numberingConfig = _config.Project!.GraphicsNumbering;
        return GetNextFromCounter(numberingConfig, _graphicsCounter++, name, label, "GRAPH", _graphicsItems);
    }
    public ThunderIndexItem GetNextMathName(string? label){
        var numberingConfig = _config.Project!.MathNumbering;
        return GetNextFromCounter(numberingConfig, _mathCounter++, "", label, "MATH", _mathItems);
    }
    public ThunderIndexItem GetNextTableName(string name, string? label){
        var numberingConfig = _config.Project!.TableNumbering;
        return GetNextFromCounter(numberingConfig, _tableCounter++, name, label, "TABLE", _tableItems);
    }
    private ThunderIndexItem GetNextFromCounter(NumberingConfig numberingConfig, int counter, string name,
                                                string? reference,
                                                string labelPrefix, List<ThunderIndexItem> indexList){
        StringBuilder sb = new();
        sb.Append(numberingConfig.Prefix);
        sb.Append(' ');
        if(numberingConfig.SectionLayer > 0){
            sb.Append(CurrentSectionId(numberingConfig.SectionLayer));
            sb.Append('.');
        }
        sb.Append(counter);
        string id = sb.ToString();
        ThunderIndexItem indexItem = new ThunderIndexItem(id, name, $"{labelPrefix}{id}");
        indexList.Add(indexItem);
        if(reference is not null){
            _elementReferences.Add(reference, new ReferenceItem(indexItem.Id, indexItem.LabelId));
            if(reference.Any(c => !char.IsLetterOrDigit(c) && c != '-' && c != '_')){
                //TODO: Log warning 'invalid name, can't be referenced'
            }
        }
        return indexItem;
    }

    public ThunderIndexItem NextSectionId(int layer, string name){
        if(layer <= _config.Project!.GraphicsNumbering.SectionLayer){
            _graphicsCounter = 1;
        }
        if(layer <= _config.Project!.MathNumbering.SectionLayer){
            _mathCounter = 1;
        }
        if(layer <= _config.Project!.TableNumbering.SectionLayer){
            _tableCounter = 1;
        }

        while(_sectionIds.Count <= layer){
            _sectionIds.Add(0);
        }
        while(_sectionIds.Count > layer){
            _sectionIds.RemoveAt(_sectionIds.Count - 1);
        }

        _sectionIds[^1]++;
        
        string id = CurrentSectionId();
        ThunderIndexItem indexItem = new(id, name, $"SECTION_{id}");
        _sectionItems.Add(indexItem);
        return indexItem;
    }

    public string CurrentSectionId(){
        return CurrentSectionId(_sectionIds.Count);
    }
    
    private string CurrentSectionId(int maxLayer){
        if(maxLayer == 0){
            return "";
        }
        StringBuilder sb = new StringBuilder();
        int counter = 0;
        foreach(int id in _sectionIds){
            counter++;
            if(counter > maxLayer){
                break;
            }
            sb.Append(id);
            sb.Append('.');
        }

        while(counter < maxLayer){
            sb.Append("0.");
            counter++;
        }
        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }

    public ThunderBuildState Clone(){
        return new ThunderBuildState(_config, _publications.Values){
                                          _config = _config,
                                          _sectionIds = _sectionIds,
                                          _graphicsCounter = _graphicsCounter,
                                          _mathCounter = _mathCounter,
                                          _tableCounter = _tableCounter,
                                          _graphicsItems =  _graphicsItems.ToList(),
                                          _mathItems =   _mathItems.ToList(),
                                          _sectionItems =   _sectionItems.ToList(),
                                          _tableItems =   _tableItems.ToList(),
                                          _publications = _publications.ToDictionary(k => k.Key, v => v.Value),
                                          _citationReferences =  _citationReferences.ToDictionary(k => k.Key, v => v.Value),
                                      };
    }

    public bool TryGetReference(string label, [NotNullWhen(true)] out ReferenceItem? referenceItem){
        label = string.Concat(label.Where(c => !char.IsWhiteSpace(c)).Select(char.ToLower));
        if(!_publications.TryGetValue(label, out ThunderPublication? publication)){
            return TryGetElementReference(label, out referenceItem);
        }
        if(!_citationReferences.TryGetValue(label, out string? reference)){
            reference = GenerateReference(publication);
            _citationReferences.Add(label, reference);
        }
        
        
        referenceItem = new ReferenceItem(reference, label);
        return true;
    }

    private bool TryGetElementReference(string label, [NotNullWhen(true)] out ReferenceItem? referenceItem){
        if(_elementReferences.TryGetValue(label, out referenceItem)){
            return true;
        }

        referenceItem = null;
        return false;
    }

    private string GenerateReference(ThunderPublication publication){
        int count = 0;
        string label;
        do{
            label = _config.Project!.Citation.Generate(publication, count);
            count++;
        } while(_citationReferences.ContainsValue(label));

        return label;
    }
}