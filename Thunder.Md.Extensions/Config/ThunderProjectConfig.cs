namespace Thunder.Md.Extensions.Config;

using System.Collections.Immutable;
using Thunder.Md.Extensions.PdfElements;

public class NumberingConfig{
    public Alignment Alignment{ get; set; } = Alignment.Bottom;
    public string Prefix{ get; set; } = "";
    public bool Used{ get; set; } = true;
    public int SectionLayer{ get; set; } = 0;
}

public class ThunderProjectConfig{
    public ThunderProjectConfig(){
        PageType = PaperType.A4;
        PageOrientation = PaperOrientation.Portrait;
    }

    public string? RootFile{ get; set; }

    public ThunderColor TextColor{ get; set; } = ThunderColor.Black;
    public ThunderColor BackgroundColor{ get; set; } = ThunderColor.White;
    public ThunderColor[] Colors{ get; set; } = [new(00, 0x4F, 0x7C), new(30, 30, 200), new(30, 200, 30)];

    public float FontSize{ get; set; } = 11;
    public int PaperHeight{ get; set; }
    public int PaperWidth{ get; set; }
    public int PageMargin{ get; set; } = 20;
    public float[] HeadlineSizes{ get; set; } = [2, 1.8f, 1.5f, 1.2f, 1.1f];
    public Alignment TextAlignment{ get; set; } = Alignment.Justify;
    public string FontFamily{ get; set; } = "Arial";

    public PaperType PageType{
        get => PaperSizes.GetPaperType(PaperWidth, PaperHeight);
        set{
            if(!PaperSizes.TryGetDimensions(PageOrientation, value, out (int Width, int Height) dimensions)){
                return;
            }

            PaperHeight = dimensions.Height;
            PaperWidth = dimensions.Width;
        }
    }

    public PaperOrientation PageOrientation{
        get => PaperHeight > PaperWidth ? PaperOrientation.Portrait : PaperOrientation.Landscape;
        set{
            int larger = int.Max(PaperHeight, PaperWidth);
            int smaller = int.Min(PaperHeight, PaperWidth);
            if(value == PaperOrientation.Landscape){
                PaperHeight = smaller;
                PaperWidth = larger;
            } else{
                PaperHeight = larger;
                PaperWidth = smaller;
            }
        }
    }

    public Dictionary<string, object> ExtensionSettings{ get; } = new();
    public List<string> ImportedExtensions{ get; } = [];

    public FontStyle NumberingStyle{ get; set; } = new(Italic: true);

    public int MaxLayerForOwnPage{ get; set; } = 1;

    public List<string> ReferencePaths { get; } = new();
    
    public CitationConfig Citation{ get; set; }  = new();

    public ImmutableDictionary<ReferenceGroup, NumberingConfig> Numberings{ get; set; }
        = new Dictionary<ReferenceGroup, NumberingConfig>(){
              { new ReferenceGroup("FIGURE"), new NumberingConfig(){SectionLayer = 1, Prefix = "Fig.", Alignment = Alignment.Bottom} },
              { new ReferenceGroup("TABLE"), new NumberingConfig(){SectionLayer = 1, Prefix = "Tab.", Alignment = Alignment.Bottom} },
              { new ReferenceGroup("MATH"), new NumberingConfig(){SectionLayer = 1, Prefix = "Eq.", Alignment = Alignment.Right} },
          }.ToImmutableDictionary();

    public ThunderColor? ContrastColorTo(ThunderColor color){
        float value = byte.Max(color.R, byte.Max(color.G, color.B)) / 255f;

        float backgroundValue = byte.Max(BackgroundColor.R, byte.Max(BackgroundColor.G, BackgroundColor.B)) / 255f;
        float textColorValue = byte.Max(TextColor.R, byte.Max(TextColor.G, TextColor.B)) / 255f;

        return Math.Abs(value - backgroundValue) > Math.Abs(value - textColorValue) 
            ? BackgroundColor 
            : TextColor;
    }
}

public enum Alignment{
    Left,
    Middle,
    Right,
    Justify,
    Top,
    Bottom
}