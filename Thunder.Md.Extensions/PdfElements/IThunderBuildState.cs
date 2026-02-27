namespace Thunder.Md.Extensions.PdfElements;

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Thunder.Md.Extensions.Cite;
using Thunder.Md.Extensions.Config;


public record ThunderIndexItem(ReferenceGroup Group, string? ReferenceId, string Name, string ReferenceText, string SectionId);
public record ReferenceGroup(string Id){
    public virtual bool Equals(ReferenceGroup? other){
        if(other is null){
            return false;
        }

        if(ReferenceEquals(this, other)){
            return true;
        }

        return string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Id);
}

public interface IThunderBuildState{
    
    public IReadOnlyList<(ThunderPublication Publication, ThunderIndexItem IndexItem)> Publications { get; }
    public ThunderIndexItem? RegisterCitation(string referenceId);
    public IReadOnlyList<ThunderIndexItem> SectionItems { get; }
    
    public ThunderIndexItem GetNextIndexItem(ReferenceGroup group, string name, string? label);
    
    public IReadOnlyList<ThunderIndexItem> OfGroup(ReferenceGroup group);
    
    public bool TryGetReference(string label, [NotNullWhen(true)] out ThunderIndexItem? referenceItem);

    public ThunderIndexItem NextSectionId(int layer, string name);
    public string CurrentSectionId();
    
    public IThunderBuildState Clone();

    void AddPageNumberAnchor(string sectionId, int deltaPage, NumberingStyle style);
    string GetPageNumber(int pageNumber, Func<string, int> anchorPageReader);
}