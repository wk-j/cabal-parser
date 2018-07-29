#! "netcoreapp2.1"
#r "nuget:Sprache,2.1.2"


using Sprache;

class KeyValue {
    public string Key { set; get; }
    public string Value { set; get; }
}


var input = @"
     packageId: 100

";

var parser =
    from leading in Parse.WhiteSpace.Many()
    from key in Parse.Letter.Many()
    from s1 in Parse.Optional(Parse.Char(' ').Many())
    from colon in Parse.Once(Parse.Char(':'))
    from s2 in Parse.Optional(Parse.Char(' ').Many())
    from value in Parse.LetterOrDigit.Many()
    select new KeyValue { Key = new string(key.ToArray()), Value = new string(value.ToArray()) };


var result = parser.Parse(input);
Console.WriteLine(result.Key);
Console.WriteLine(result.Value);