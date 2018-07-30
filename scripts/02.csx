#! "netcoreapp2.1"
#r "nuget:Sprache,2.1.2"
#r "nuget:Newtonsoft.Json,11.0.0"

using Sprache;
using Newtonsoft.Json;

class CabalItem { }

class Property : CabalItem {
    public string Key { set; get; }
    public string Value { set; get; }

    static Parser<IEnumerable<string>> lineEnd = Parse.LineEnd.Once();

    static Parser<string> colon =
        from s1 in Parse.Optional(Parse.Char(' ').Many())
        from colon in Parse.Once(Parse.Char(':'))
        from s2 in Parse.Optional(Parse.Char(' ').Many())
        select $"{s1}{colon}{s2}";

    static Parser<String> key =
        from key in Parse.Letter.XOr(Parse.Char('-')).Many()
        select new string(key.ToArray());

    static Parser<string> value =
        from value in Parse.Except(Parse.AnyChar, Parse.LineEnd).Many()
        select new string(value.ToArray());

    public static Parser<Property> Parser =
        from key in key
        from colon in colon
        from value in value
        from lineEnd in lineEnd
        select new Property { Key = key, Value = value };
}

class Comment : CabalItem {
    public string Line { set; get; }

    public static Parser<Comment> Parser =
        from line in new CommentParser { Single = "--" }.SingleLineComment
        from e in Parse.LineEnd.Once()
        select new Comment { Line = line };
}

class ExecutableItem {
}

class ExecutableProperty : ExecutableItem {
    public string Key { set; get; }
    public string Value { set; get; }
}

class BuildDepends : ExecutableItem {
    public IEnumerable<string> Packages { set; get; }
}

class Empty : CabalItem {
    public int Count { set; get; }
    public static Parser<Empty> Parser =
        from empty in Parse.LineEnd.AtLeastOnce()
        select new Empty { Count = empty.Count() };
}

class Executable : CabalItem {
    public string Name { set; get; }
    public IEnumerable<ExecutableProperty> Properties { set; get; }
    public BuildDepends BuildDepends { set; get; }

    public static Parser<Executable> Parser =
        from key in Parse.String("executable")
        from space in Parse.WhiteSpace.AtLeastOnce()
        from chars in Parse.Letter.Many()
        let name = new string(chars.ToArray())
        select new Executable { Name = name };
}

class Cabal {
    public Dictionary<string, string> Properties { set; get; }
    public IEnumerable<Executable> Executables { set; get; }
    public IEnumerable<Comment> Comments { set; get; }
    public IEnumerable<Empty> Empties { set; get; }
}

class Parser {

    public static Cabal ParseInput(string input) {
        var comment = Comment.Parser;
        var property = Property.Parser;
        var executable = Executable.Parser;
        var empty = Empty.Parser;
        var parser = comment
            .XOr<CabalItem>(empty)
            .XOr<CabalItem>(property)
            .XOr<CabalItem>(executable)
            .Many();

        var result = parser.Parse(input);
        return new Cabal {
            Comments = result.OfType<Comment>(),
            Properties = result.OfType<Property>().ToDictionary(kv => kv.Key, kv => kv.Value),
            Executables = result.OfType<Executable>(),
            Empties = result.OfType<Empty>()
        };
    }
}

var input = File.ReadAllText("resource/Hello.cabal");
var result = Parser.ParseInput(input);
var json = JsonConvert.SerializeObject(result, Formatting.Indented);
Console.WriteLine(json);