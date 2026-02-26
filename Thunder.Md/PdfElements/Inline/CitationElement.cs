namespace Thunder.Md.PdfElements.Inline;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Cite;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;

public class CitationElement: ITextElement{
    private ThunderIndexItem? _reference;
    
    public CitationElement(string label){
        Label = label;
    }
    
    public string Label{ get; }
    public string Text => Label;
    
    public void Draw(TextDescriptor text, FontStyle fontStyle, IThunderBuildState state, ThunderConfig config){
        ThunderIndexItem? reference =  _reference;
        if(reference is not null || !state.TryGetReference(Label, out reference)){
            text.Span($"[No reference for '{Label}']").Style(fontStyle.Combine(new FontStyle(Bold: true, Italic: true)).ToPdfStyle(config));
            //TODO: Log warning
            return;
        }
        
        TextStyle style = fontStyle.Combine(config.Project!.Citation.Style).ToPdfStyle(config);

        style = config.Project!.Citation.Position switch{
                    CitationPosition.SubScript   => style.Subscript(),
                    CitationPosition.SuperScript => style.Superscript(),
                    _                            => style
                };
        
        text.SectionLink(reference.ReferenceText, reference.SectionId).Style(style);
    }

    public void Prebuild(ThunderConfig config, IThunderBuildState state){
        state.RegisterCitation(Label);
    }
}