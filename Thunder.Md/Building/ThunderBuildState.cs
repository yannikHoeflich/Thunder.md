namespace Thunder.Md.Building;

using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Thunder.Md.Extensions.Cite;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;

public class ThunderBuildState: IThunderBuildState{
    public static readonly ReferenceGroup SectionGroup = new ReferenceGroup("SECTION");
    public static readonly ReferenceGroup FigureGroup = new ReferenceGroup("FIGURE");
    public static readonly ReferenceGroup TableGroup = new ReferenceGroup("TABLE");
    public static readonly ReferenceGroup MathGroup = new ReferenceGroup("MATH");
    public static readonly ReferenceGroup CitationGroup = new ReferenceGroup("CITATION");

    private bool _isInPrebuild = true;
    
    private ThunderConfig _config;
    private List<int> _sectionIds = [];
    private List<ThunderIndexItem> _sectionItems = [];
    private Dictionary<string, ThunderPublication> _publications = new(StringComparer.OrdinalIgnoreCase);
    private List<ThunderIndexItem> _elementReferences = new();
    private Dictionary<ReferenceGroup, int> _counters = [];

    public IReadOnlyList<(ThunderPublication Publication, ThunderIndexItem IndexItem)> Publications =>
        _publications.Where(x => _elementReferences.Any(y => y.Group == CitationGroup && x.Key.Equals(y.ReferenceId, StringComparison.OrdinalIgnoreCase)))
                     .Select(x => (x.Value, _elementReferences.First(y => x.Key.Equals(y.ReferenceId, StringComparison.OrdinalIgnoreCase))))
                     .ToImmutableList();

    public IReadOnlyList<ThunderIndexItem> SectionItems => _sectionItems.AsReadOnly();


    public ThunderBuildState(ThunderConfig config, IReadOnlyCollection<ThunderPublication> publications){
        _config = config;
        foreach(ThunderPublication thunderPublication in publications){
            _publications.Add(thunderPublication.Label, thunderPublication);
        }
    }


    public ThunderIndexItem GetNextItemName(ReferenceGroup group, string name, string? label){
        if(!_isInPrebuild){
            //TODO: Log error
            return null!;
        }

        if(label is not null && _elementReferences.Any(x => x.ReferenceId == label)){
            //TODO: Log error
            label = null;
        }
        
        string refText = "";
        if(!_config.Project!.Numberings.TryGetValue(group, out var numberingConfig)){
            refText = Random.Shared.Next().ToString();
        } else{
            if(!_counters.TryGetValue(group, out int counter)){
                counter = 1;
                _counters.Add(group, counter);
            }

            refText = GetNextFromCounter(numberingConfig, counter);
        }

        var indexItem = new ThunderIndexItem(group, label, name, refText, Guid.NewGuid().ToString());
        _elementReferences.Add(indexItem);
        return indexItem;
    }

    public ThunderIndexItem? RegisterCitation(string referenceId){
        referenceId = string.Concat(referenceId.Where(c => !char.IsWhiteSpace(c)).Select(char.ToLower));
        if(!_publications.TryGetValue(referenceId, out ThunderPublication? publication)){
            return null;
        }

        if(_elementReferences.Any(x => x.ReferenceId == referenceId)){
            return _elementReferences.FirstOrDefault(x => x.ReferenceId == referenceId);
        }

        string reference = GenerateReference(publication);
        ThunderIndexItem indexItem = new(CitationGroup, referenceId, "", reference, Guid.NewGuid().ToString());
        _elementReferences.Add(indexItem);
        return indexItem;

    }

    
    private string GenerateReference(ThunderPublication publication){
        int count = 0;
        string label;
        do{
            label = _config.Project!.Citation.Generate(publication, count);
            count++;
        } while(_elementReferences.Any(x => x.ReferenceText == label));

        return label;
    }
    
    public IReadOnlyList<ThunderIndexItem> OfGroup(ReferenceGroup group) => _elementReferences.Where(x => x.Group == group).ToList();

    public bool TryGetReference(string referenceId, [NotNullWhen(true)] out ThunderIndexItem? referenceItem){
        referenceItem = _elementReferences.FirstOrDefault(x => x.ReferenceId == referenceId);
        
        return referenceItem is not null;
    }


    private string GetNextFromCounter(NumberingConfig numberingConfig, int counter){
        StringBuilder sb = new();
        sb.Append(numberingConfig.Prefix);
        sb.Append(' ');
        if(numberingConfig.SectionLayer > 0){
            sb.Append(CurrentSectionId(numberingConfig.SectionLayer));
            sb.Append('.');
        }

        sb.Append(counter);
        
        return sb.ToString();
    }

    public ThunderIndexItem NextSectionId(int layer, string name){
        foreach(ReferenceGroup group in _counters.Keys){
            if(!_config.Project!.Numberings.TryGetValue(group, out var numberingConfig)){
                continue;
            }

            if(layer <= numberingConfig.SectionLayer){
                _counters[group]++;
            }
        }

        while(_sectionIds.Count <= layer){
            _sectionIds.Add(0);
        }

        while(_sectionIds.Count > layer){
            _sectionIds.RemoveAt(_sectionIds.Count - 1);
        }

        _sectionIds[^1]++;

        string sectionId = CurrentSectionId();

        ThunderIndexItem indexItem = new(SectionGroup, null, name, sectionId, sectionId);
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

    public IThunderBuildState Clone(){
        return new ThunderBuildState(_config, _publications.Values){
                                                                       _config = _config,
                                                                       _sectionIds = _sectionIds,
                                                                       _sectionItems = _sectionItems.ToList(),
                                                                       _publications
                                                                           = _publications.ToDictionary(k => k.Key,
                                                                               v => v.Value),
                                                                       _elementReferences
                                                                           = _elementReferences.ToList()
                                                                   };
    }

    public void EndOfPrebuild(){
        _isInPrebuild = false;
    }
}