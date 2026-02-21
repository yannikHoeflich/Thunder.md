namespace Thunder.Md.Extensions.PdfElements;

using System.Diagnostics.CodeAnalysis;

public delegate bool CanvasCreatorMethod(ExtensionArgs args, string url, ITextElement? altText, [NotNullWhen(true)] out ICanvasElement? canvasElement);
public record CanvasCreator(CanvasCreatorMethod Creator, string? Protocol = null, params string[] FileExtension);