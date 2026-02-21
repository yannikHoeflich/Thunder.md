namespace Thunder.Md.Readers;

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;
using YamlDotNet.Serialization;

public class ConfigLoader{
    private readonly ILogger _logger;

    public ConfigLoader(ILogger logger){
        _logger = logger;
    }

    public bool TryRead([NotNullWhen(true)] out ThunderProjectConfig? config){
        if(!File.Exists(ThunderPaths.ConfigPath)){
            _logger.LogError("No config file existent. Please check if you are in the right directory or create a proper config file.");
            config = null;
            return false;
        }

        IDeserializer deserializer = new DeserializerBuilder().Build();
        Dictionary<string, object> rawData
            = deserializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(ThunderPaths.ConfigPath));

        config = new ThunderProjectConfig();

        rawData = rawData.ToDictionary(x => x.Key, x => {
            if(x.Value is not string str){
                return x.Value;
            }

            if(long.TryParse(str, out long l)){
                return l;
            }

            if(double.TryParse(str, out double d)){
                return d;
            }

            return str;
        });

        foreach(KeyValuePair<string, object> kvp in rawData){
            switch(kvp.Key){
                //HeadlineSizes
                // Alignments
                // Fontfamily
                // Numbering
                // MaxLayerForOwnPage

                case "root-file":
                    config.RootFile = kvp.Value.ToString();
                break;
                case "extensions":
                    if(kvp.Value is not List<object> extensionList){
                        _logger.LogWarning("Invalid extensions list type: {ExtensionsList}", kvp.Value.ToString());
                        continue;
                    }

                    config.ImportedExtensions.AddRange(extensionList.Select(x => x.ToString()!));

                break;
                case "page-type":
                    if(kvp.Value is not string paperTypeStr){
                        _logger.LogWarning("Invalid paper type: {PaperTypeStr}", kvp.Value.ToString());
                        continue;
                    }

                    if(!PaperType.TryParseFromString(paperTypeStr, out PaperType paperType)){
                        _logger.LogWarning("Invalid paper type: {PaperTypeStr}. Using A4 as fallback",
                                           paperTypeStr);
                        paperType = PaperType.A4;
                    }

                    config.PageType = paperType;

                break;
                case "page-orientation":
                    if(kvp.Value is not string paperOrientationStr){
                        _logger.LogWarning("Invalid paper orientation type: {PaperOrientationStr}",
                                           kvp.Value.ToString());
                        continue;
                    }

                    if(!PaperOrientation.TryParseFromString(paperOrientationStr,
                                                            out PaperOrientation paperOrientation)){
                        _logger
                            .LogWarning("Invalid paper orientation: {PaperOrientationStr}. Using 'portrait' as default fallback.",
                                        paperOrientationStr);
                        paperOrientation = PaperOrientation.Portrait;
                    }

                    config.PageOrientation = paperOrientation;

                break;
                case "page-width":
                    if(kvp.Value is not long paperWidth){
                        if(kvp.Value is not double paperWidthDouble){
                            _logger.LogWarning("Invalid paper width: {PaperWidth}mm", kvp.Value.ToString());
                            continue;
                        }

                        paperWidth = (long)Math.Round(paperWidthDouble);
                        _logger.LogInformation("Paper width rounded to: {PaperWidth}mm", paperWidth);
                    }

                    if(paperWidth > int.MaxValue){
                        _logger.LogWarning("Invalid paper width: {PaperWidth}mm", paperWidth);
                        continue;
                    }

                    config.PaperWidth = (int)paperWidth;

                break;
                case "page-height":{
                    if(kvp.Value is not long paperHeight){
                        if(kvp.Value is not double paperHeightDouble){
                            _logger.LogWarning("Invalid paper height: {Paperheight}mm", kvp.Value.ToString());
                            continue;
                        }

                        paperHeight = (int)Math.Round(paperHeightDouble);
                        _logger.LogInformation("Paper height rounded to: {Paperheight}mm", paperHeight);
                    }

                    if(paperHeight > int.MaxValue){
                        _logger.LogWarning("Invalid paper height: {PaperWidth}mm", paperHeight);
                        continue;
                    }

                    config.PaperHeight = (int)paperHeight;

                    break;
                }
                case "colors":
                    if(kvp.Value is not List<object> colorList){
                        _logger.LogWarning("Invalid color list type: {ColorList}", kvp.Value.ToString());
                        continue;
                    }

                    List<ThunderColor> colors = new(colorList.Count);
                    foreach(string str in colorList.OfType<string>()){
                        if(!ThunderColor.TryFromHex(str, out ThunderColor color)){
                            continue;
                        }

                        colors.Add(color);
                    }

                    config.Colors = colors.ToArray();

                break;
                case "background-color":
                    if(kvp.Value is not string backgroundColorStr){
                        _logger.LogWarning("Invalid background color: {BackgroundColorStr}", kvp.Value.ToString());
                        continue;
                    }

                    if(!ThunderColor.TryFromHex(backgroundColorStr, out ThunderColor backgroundColor)){
                        _logger
                            .LogWarning("Invalid background color: {BackgroundColorStr}. Using default fallback 'white'",
                                        backgroundColorStr);
                        backgroundColor = ThunderColor.White;
                    }

                    config.BackgroundColor = backgroundColor;

                break;
                case "text-color":
                    if(kvp.Value is not string textColorStr){
                        _logger.LogWarning("Invalid text color: {BackgroundColorStr}", kvp.Value.ToString());
                        continue;
                    }

                    if(!ThunderColor.TryFromHex(textColorStr, out ThunderColor textColor)){
                        _logger.LogWarning("Invalid text color: {TextColorStr}. Using default fallback 'black'",
                                           textColorStr);
                        textColor = ThunderColor.Black;
                    }

                    config.TextColor = textColor;

                break;

                case "font-size":
                    if(kvp.Value is not double fontSize){
                        if(kvp.Value is not long fontSizeInt){
                            _logger.LogWarning("Invalid font size: {FontSize}", kvp.Value.ToString());
                            continue;
                        }

                        fontSize = fontSizeInt;
                    }

                    config.FontSize = (float)fontSize;
                break;

                case "page-margin":
                    if(kvp.Value is not long pageMargin){
                        if(kvp.Value is not double pageMarginDouble){
                            _logger.LogWarning("Invalid page margin: {Paperheight}mm", kvp.Value.ToString());
                            continue;
                        }

                        pageMargin = (int)Math.Round(pageMarginDouble);
                        _logger.LogInformation("Page margin rounded to: {Paperheight}mm", pageMargin);
                    }

                    if(pageMargin > int.MaxValue){
                        _logger.LogWarning("Invalid page margin: {PaperWidth}mm", pageMargin);
                        continue;
                    }

                    config.PageMargin = (int)pageMargin;

                break;

                case "references":
                    if(kvp.Value is not List<object> referencePaths){
                        _logger.LogWarning("Invalid reference paths: {ReferencePaths}", kvp.Value.ToString());
                        continue;
                    }

                    config.ReferencePaths.AddRange(referencePaths.OfType<string>());
                break;
                default:
                    config.ExtensionSettings[kvp.Key] = kvp.Value;
                break;
            }
        }

        return true;
    }
}