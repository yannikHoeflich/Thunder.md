namespace Thunder.Md.Extensions.PdfElements;

using QuestPDF.Infrastructure;
using Thunder.Md.Extensions.Config;

public class WrapperCanvas: ICanvasElement{
    private readonly IPdfElement _wrapper;
    public WrapperCanvas(IPdfElement wrapper){
        this._wrapper = wrapper;
    }

    public void Draw(ThunderConfig config, IThunderBuildState state, IContainer container){
        _wrapper.Draw(config, state, container);
    }
    public void Prebuild(ThunderConfig config, IThunderBuildState state){
        _wrapper.Prebuild(config, state);
    }
}