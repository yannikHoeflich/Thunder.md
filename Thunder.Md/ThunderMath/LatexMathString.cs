namespace Thunder.Md.ThunderMath;

using System.Text;
using CSharpMath.VectSharp;
using VectSharp;
using VectSharp.SVG;

public class LatexMathString{
    private string _latexStr;
    public LatexMathString(string latexStr){
        _latexStr = $"${latexStr}$";
    }

    public string ToSvg(){
        var painter = new TextPainter {
                                          LaTeX = _latexStr
                                      };
        var page = painter.DrawToPage(400f); // adjust width here

        var stream = new MemoryStream();
        page.SaveAsSVG(stream);

        var buffer = stream.ToArray();
        var svg = Encoding.UTF8.GetString(buffer);

        File.WriteAllText("test.svg", svg);
        return svg;
    }
}