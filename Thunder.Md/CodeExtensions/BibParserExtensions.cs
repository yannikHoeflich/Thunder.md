namespace Thunder.Md.CodeExtensions;

using System.Diagnostics.CodeAnalysis;
using BibSharp;
using Thunder.Md.Extensions.Cite;

public static class BibParserExtensions{
    extension(BibParser bibParser){
        public bool TryParseFile(string path, [NotNullWhen(true)] out IList<BibEntry>? entries){
            try{
                entries = bibParser.ParseFile(path);
            } catch{
                entries = null;
                return false;
            }

            return true;
        }
    }

    extension(BibEntry entry){
        public ThunderPublication ToThunderPublication(){
            ThunderPublication publication = new(entry.Key, entry.EntryType.Value){
                                                               Authors = entry.GetAuthors(),
                                                               Title = entry.Title,
                                                               Organisation = entry.Organization,
                                                               Year = entry.Year,
                                                               Publisher = entry.Publisher
                                                           };

            return publication;
        }

        private ThunderAuthor[]? GetAuthors(){
            return entry.Authors.Count > 0
                ? entry.Authors
                       .Select(x =>
                                   new
                                       ThunderAuthor(JoinNames(x.FirstName,
                                                               x.MiddleName),
                                                     x.LastName,
                                                     x.Suffix))
                       .ToArray()
                : null;
        }
    }

    private static string? JoinNames(string? firstname, string? middlename){
        if(firstname is null && middlename is null){
            return null;
        }

        if(middlename is null){
            return firstname;
        }

        if(firstname is null){
            return middlename;
        }

        return $"{firstname} {middlename}";
    }
}