namespace Thunder.Md.Extensions.PdfElements;

using QuestPDF.Fluent;
using Thunder.Md.Extensions.Config;

public class WrapperInlineCanvas: IInlineCanvasElement{
    private readonly ITextElement _wrapper;
    
    public WrapperInlineCanvas(ITextElement wrapper){
        _wrapper = wrapper;
    }
    public string Text => _wrapper.Text;

    public void Draw(TextDescriptor text, FontStyle fontStyle, IThunderBuildState state, ThunderConfig config){
        _wrapper.Draw(text, fontStyle, state, config);
    }

    public void Prebuild(ThunderConfig config, IThunderBuildState state){
        _wrapper.Prebuild(config, state);
    }
}