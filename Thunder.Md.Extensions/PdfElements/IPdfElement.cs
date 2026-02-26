namespace Thunder.Md.Extensions.PdfElements;

using QuestPDF.Infrastructure;
using Thunder.Md.Extensions.Config;

public interface IPdfElement{
    public void Draw(ThunderConfig config, IThunderBuildState state, IContainer container);
    public void Prebuild(ThunderConfig config, IThunderBuildState state);
}