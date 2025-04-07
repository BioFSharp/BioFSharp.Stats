(**
# BioFSharp.XYZ

you can write markdown in these comment brackets `(** ... *)`
*)

// and actual F# content in a literate script file
let a = 42

(**
include evaluations like this:
*)

(***include-value: a***)

(**
Include the compiled binaries of your project to use in the docs like this. 
Make sure to load any external references via nuget.
*)
#r "nuget: BioFSharp, 2.0.0-preview.3"
#I @"..\src\BioFSharp.XYZ\bin\Release\net8.0"
#r "BioFSharp.XYZ.dll"

open BioFSharp.XYZ

let o = BioTalk.helloBio "Kev"

(***include-value: o***)

(**
Read more about documentation with fsdocs here: https://fsprojects.github.io/FSharp.Formatting/
*)