namespace DiffResults

open DiffPlex.DiffBuilder


// A program being tested outputs a multiline-string actual result
// A program doing the testing
// • has a multiline-string expected result
// • calls diff to get a multiline string showing the differences
//   between expected and actual
// In the diff output:
//   x marks a line that was output but shouldn't have been
//   √ marks a line that should have been output but wasn't
(*
        expected  actual
        --------  ------
        yes       no
        yes       no
        what      not
        where     what
        why       but
        when      why
        
        Output:
        x no
        √ yes
        x no
        √ yes
        x not
          what
        x but
        √ where
          why
        √ when
*)

// Same means expected = actual
type Result =
| Changed = 0
| Same    = 1

// Specify which diff mode to use
type DiffMode =
| LineByLine = 0 // SideBySideDiffBuilder
| Chunked    = 1 // InlineDiffBuilder


type Diff() =

  static member DiffToString (expected: string, actual: string, ?ignoreWhiteSpace: bool, ?mode: DiffMode)  : Result * string * int =
    let ignoreWS = defaultArg ignoreWhiteSpace false
    let diffMode = defaultArg mode DiffMode.LineByLine
    match expected = actual with
    | true  -> Result.Same, "", 0
    | false ->
    match diffMode with
    | DiffMode.LineByLine ->  let diffBuilder = SideBySideDiffBuilder()
                              let diff = diffBuilder.BuildDiffModel(actual, expected, ignoreWS)
                              let s, nChanges = LineByLine.getDiffString diff
                              match nChanges = 0 with
                              | true  -> Result.Same   , "", 0
                              | false -> Result.Changed, s , nChanges
    | DiffMode.Chunked    ->  let diff = InlineDiffBuilder.Diff(actual, expected, ignoreWS)
                              let s, nChanges = Chunked.getDiffString diff
                              match nChanges = 0 with
                              | true  -> Result.Same   , "", 0
                              | false -> Result.Changed, s , nChanges
    | _ -> failwith "Unknown DiffMode"

  static member DiffToConsole (expected: string, actual: string, ?ignoreWhiteSpace: bool, ?mode: DiffMode)  : Result * int =
    let ignoreWS = defaultArg ignoreWhiteSpace false
    let diffMode = defaultArg mode DiffMode.LineByLine
    match expected = actual with
    | true  -> Result.Same, 0
    | false ->
    match diffMode with
    | DiffMode.LineByLine ->  let diffBuilder = SideBySideDiffBuilder()
                              let diff = diffBuilder.BuildDiffModel(actual, expected, ignoreWS)
                              let nChanges = LineByLine.print diff
                              match nChanges = 0 with
                              | true  -> Result.Same   , 0
                              | false -> Result.Changed, nChanges
    | DiffMode.Chunked    ->  let diff = InlineDiffBuilder.Diff(actual, expected, ignoreWS)
                              let nChanges = Chunked.print diff
                              match nChanges = 0 with
                              | true  -> Result.Same   , 0
                              | false -> Result.Changed, nChanges
    | _ -> failwith "Unknown DiffMode"
    
  static member RunToString (expected: string, actual: string, ?ignoreWhiteSpace: bool, ?mode: DiffMode)  : string =
    let ignoreWS = defaultArg ignoreWhiteSpace false
    let diffMode = defaultArg mode DiffMode.LineByLine
    match Diff.DiffToString(expected, actual, ignoreWS, diffMode) with
    | Result.Same   , _, _ ->          "Test passed\n"
    | Result.Changed, s, n -> sprintf $"%s{s}––––––––––––––\nTest failed: %d{n} differences\n"
    | _ -> failwith "unreachable"
      
  static member RunToConsole (expected: string, actual: string, ?ignoreWhiteSpace: bool, ?mode: DiffMode)  : unit =
    let ignoreWS = defaultArg ignoreWhiteSpace false
    let diffMode = defaultArg mode DiffMode.LineByLine
    match Diff.DiffToConsole(expected, actual, ignoreWS, diffMode) with
    | Result.Same   , _ -> printfn "Test passed"
    | Result.Changed, n -> printfn $"––––––––––––––\nTest failed: %d{n} differences"
    | _ -> failwith "unreachable"