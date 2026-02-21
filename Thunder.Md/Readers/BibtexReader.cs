namespace Thunder.Md.Readers;

using System.Collections.Immutable;
using BibSharp;
using Thunder.Md.CodeExtensions;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Cite;
using Thunder.Md.Extensions.Config;

public class BibtexReader{
    private ThunderConfig _config;

    public BibtexReader(ThunderConfig config){
        _config = config;
    }

    public IReadOnlyCollection<ThunderPublication> Read(){
        var parser = new BibParser();

        List<string> bibFiles = [];
        foreach(string fullPath in _config.Project!.ReferencePaths
                                          .Select(path => Path.Combine(ThunderPaths.Source, path))
                                          .Where(Directory.Exists)){
            GetBibFiles(fullPath, bibFiles);
        }

        List<BibEntry> entries = [];
        foreach(var bibFile in bibFiles){
            if(!parser.TryParseFile(bibFile, out IList<BibEntry>? bibEntries)){
                continue;
            }

            entries.AddRange(bibEntries);
        }

        return entries.Select(x => x.ToThunderPublication()).ToImmutableList();
    }

    private void GetBibFiles(string path, List<string> bibFiles){
        bibFiles.AddRange(Directory.GetFiles(path, "*.bib"));

        foreach(string directory in Directory.EnumerateDirectories(path)){
            GetBibFiles(directory, bibFiles);
        }
    }
}