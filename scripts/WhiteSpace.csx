#! "netcoreapp2.1"
#r "nuget:Sprache,2.1.2"


using Sprache;

class KeyValue {
    public string Key { set; get; }
    public string Value { set; get; }
}


var comment = new CommentParser {
    Single = "--"
};

var parser =
    from leading in Parse.WhiteSpace.Many()
    from key in Parse.Letter.Many()
    from s1 in Parse.Optional(Parse.Char(' ').Many())
    from colon in Parse.Once(Parse.Char(':'))
    from s2 in Parse.Optional(Parse.Char(' ').Many())
    from value in Parse.Except(Parse.AnyChar, Parse.WhiteSpace).Many()
    from _ in Parse.LineEnd.AtLeastOnce()
    select new KeyValue { Key = new string(key.ToArray()), Value = new string(value.ToArray()) };


var input = File.ReadAllText("resource/Hello.cabal");
var result = Parse.Many(parser).Parse(input);

foreach (var item in result) {
    Console.WriteLine(item.Key);
    Console.WriteLine(item.Value);
}