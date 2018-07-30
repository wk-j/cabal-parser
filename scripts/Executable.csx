#! "netcoreapp2.1"
#r "nuget:Sprache,2.1.2"
#r "nuget:Newtonsoft.Json,11.0.0"

using Sprache;
using Newtonsoft.Json;

var empty =
    from empty in Parse.LineEnd.Once()
    select empty;

var value =
    from value in Parse.Except(Parse.AnyChar, Parse.LineEnd).Many()
    select new string(value.ToArray());

var property =
    from key in Parse.Letter.XOr(Parse.Char('-')).Many()
    from colon in Parse.String(":")
    from _ in Parse.WhiteSpace.AtLeastOnce()
    from value in value
    select new { Name = new string(key.ToArray()), Value = new string(value.ToArray()) };

var executable =
        from key in Parse.String("executable").Once()
        from space in Parse.WhiteSpace.Many()
        from name in Parse.Letter.Many()
        select new { Executable = new string(name.ToArray()) };

/*
hs-source-dirs:      src
*/

var input = @"
homepage:            https://github.com/githubuser/Hello#readme
license:             BSD3
license-file:        LICENSE
executable hello
  hs-source-dirs:      src
  main-is:             Main.hs
  default-language:    Haskell2010
  build-depends:
      base >= 4.7 && < 5
    , http-conduit
    , bytestring
";

var result =
    executable
    .XOr<object>(property)
    .XOr<object>(empty)
    .Many()
    .Parse(input);

var json = JsonConvert.SerializeObject(result, Formatting.Indented);
Console.WriteLine(json);