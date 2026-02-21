namespace Thunder.Md.PdfElements.Inline;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Cite;
using Thunder.Md.Extensions.Config;
using Thunder.Md.Extensions.PdfElements;

public class CitationElement: ITextElement{
    public CitationElement(string label){
        Label = label;
    }
    
    public string Label{ get; }
    public string Text => Label;
    
    public void Draw(TextDescriptor text, FontStyle fontStyle, ThunderBuildState state, ThunderConfig config){
        if(!state.TryGetCitation(Label, out Citation? citation)){
            text.Span($"[No reference for '{Label}']").Style(fontStyle.Combine(new FontStyle(Bold: true, Italic: true)).ToPdfStyle(config));
            return;
        }
        
        TextStyle style = fontStyle.Combine(config.Project!.Citation.Style).ToPdfStyle(config);

        style = config.Project!.Citation.Position switch{
                    CitationPosition.SubScript   => style.Subscript(),
                    CitationPosition.SuperScript => style.Superscript(),
                    _                            => style
                };
        
        text.SectionLink(citation.ReferenceString, citation.Label).Style(style);
    }
}