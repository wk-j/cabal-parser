#! "netcoreapp2.1"
#r "nuget:Sprache,2.1.2"

using Sprache;

class Item {

}

class KeyValue : Item {
    public string Key { set; get; }
    public string Value { set; get; }
}

class Comment : Item {
    public string Line { set; get; }
}

var comment =
    from line in new CommentParser { Single = "--" }.SingleLineComment
    from e in Parse.LineEnd.AtLeastOnce()
    select new Comment { Line = line };


var colon =
    from s1 in Parse.Optional(Parse.Char(' ').Many())
    from colon in Parse.Once(Parse.Char(':'))
    from s2 in Parse.Optional(Parse.Char(' ').Many())
    select $"{s1}{colon}{s2}";

var key =
    from key in Parse.Letter.XOr(Parse.Char('-')).Many()
    select new string(key.ToArray());

var value =
    from value in Parse.Except(Parse.AnyChar, Parse.LineEnd).Many()
    select new string(value.ToArray());

var lineEnd = Parse.LineEnd.AtLeastOnce();

var parser =
    from leading in Parse.WhiteSpace.Many()
    from key in key
    from colon in colon
    from value in value
    from lineEnd in lineEnd
    select new KeyValue { Key = key, Value = value };

var input = File.ReadAllText("resource/Hello.cabal");

var result =
    from kv in comment.Select(x => x).XOr<Item>(parser)
    select kv;

foreach (var item in result.Many().Parse(input)) {
    if (item is KeyValue item2) {
        Console.WriteLine($"{item2.Key}: {item2.Value}");
    } else if (item is Comment comment) {
        Console.WriteLine($"-- {comment.Line}");
    }
}