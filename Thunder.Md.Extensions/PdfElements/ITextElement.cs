namespace Thunder.Md.Extensions.PdfElements;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Thunder.Md.Extensions.Config;

public interface ITextElement{
    public string Text { get; }
    public void Draw(TextDescriptor text, FontStyle fontStyle, ThunderBuildState state, ThunderConfig config);
}