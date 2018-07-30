#! "netcoreapp2.1"
#r "nuget:Sprache,2.1.2"
#r "nuget:Newtonsoft.Json,11.0.0"

using Sprache;
using Newtonsoft.Json;

var empty =
    from empty in Parse.LineEnd.Once()
    select empty;

var property =
    from key in Parse.Letter.XOr(Parse.Char('-')).Many()
    from colon in Parse.String(":")
    from _ in Parse.WhiteSpace.AtLeastOnce()
    from value in Parse.Letter.Many()
    from line in Parse.LineEnd.Once()
    select new { name = new string(key.ToArray()), value = new string(value.ToArray()) };

var executable =
    from key in Parse.String("executable")
    from space in Parse.WhiteSpace.AtLeastOnce()
    from name in Parse.Letter.Many()
    let n = new string(name.ToArray())
    select new { Name = n };

var input = @"
hs-source-dirs:      src
executable hello
  hs-source-dirs:      src
  main-is:             Main.hs
  default-language:    Haskell2010
  build-depends:
      base >= 4.7 && < 5
    , http-conduit
    , bytestring
";

var result = property
    .XOr<object>(executable)
    .XOr<object>(empty)
    .Many()
    .Parse(input);

var json = JsonConvert.SerializeObject(result, Formatting.Indented);
Console.WriteLine(json);