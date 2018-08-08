#! "netcoreapp2.1"
#r "nuget:Sprache,2.1.2"
#r "nuget:Newtonsoft.Json,11.0.0"

using Sprache;
using Newtonsoft.Json;
using System.Collections.Generic;

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
        from key in Parse.Letter.XOr(Parse.Char('-')).Many().Text()
        select key;

    static Parser<string> value =
        from value in Parse.Except(Parse.AnyChar, Parse.LineEnd).Many().Text()
        select value;

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
    public static Parser<IEnumerable<char>> NotWhite { get => notWhite; set => notWhite = value; }

    static Parser<string> colon =
        from s1 in Parse.Optional(Parse.Char(' ').Many())
        from colon in Parse.Once(Parse.Char(':'))
        from s2 in Parse.Optional(Parse.Char(' ').Many())
        select $"{s1}{colon}{s2}";

    static Parser<String> key =
        from space in Parse.WhiteSpace.Many()
        from key in Parse.Letter.XOr(Parse.Char('-')).Many().Text()
        select key;

    static Parser<string> value =
        from value in Parse.Except(Parse.AnyChar, Parse.LineEnd).Many().Text()
        select value;

    public static Parser<ExecutableProperty> property =
        from key in key
        from colon in colon
        from value in value
        from lineEnd in lineEnd
        select new ExecutableProperty { Key = key, Value = value };

    static Parser<IEnumerable<string>> lineEnd = Parse.LineEnd.Once();

    static Parser<IEnumerable<char>> notWhite = Parse.Except(Parse.AnyChar, Parse.WhiteSpace).Many();

    public static Parser<Executable> Parser =
        from key in Parse.String("executable").Once()
        from space in Parse.WhiteSpace.AtLeastOnce()
        from name in NotWhite.Text()
        from property in property.Many()
        select new Executable {
            Name = name.ToString(),
            Properties = property.Select(x => new ExecutableProperty {
                Key = x.Key,
                Value = x.Value
            })
        };
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
        var parser =
            comment
            .Or<CabalItem>(executable)
            .Or<CabalItem>(empty)
            .Or<CabalItem>(property)
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
// var input = File.ReadAllText("resource/Executable.cabal");
var result = Parser.ParseInput(input);
var json = JsonConvert.SerializeObject(result, Formatting.Indented);
Console.WriteLine(json);