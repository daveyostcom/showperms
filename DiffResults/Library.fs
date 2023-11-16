namespace DiffResults

open System
open DiffPlex.DiffBuilder
open DiffPlex.DiffBuilder.Model


// A program being tested outputs an actual result multiline string
// A program doing the testing
// • has an expected result multiline string
// • calls diff to get a multiline string showing the differences
//   between expected and actual
// In the diff output:
//   x marks a line that was output but shouldn't have been
//   √ marks a line that should have been output but wasn't
       (*
          x 0
            a
          √ b
          x x
            c
          √ d
          √ 
       *)
// Same return means expected = actual
  

type Result =
| Changed = 0
| Same    = 1

type Diff() =

  static let linePrefix lineType =
    match lineType with
    | ChangeType.Inserted -> "x " // bad,  an actual   line was inserted, is not in expected
    | ChangeType.Deleted  -> "√ " // good, an expected line was deleted,  is not in actual  
    | _                   -> "  "

  static let tallyChange lineType =
    match lineType with
    | ChangeType.Inserted -> 1
    | ChangeType.Deleted  -> 1
    | _                   -> 0

  static let getDiffString (diff: DiffPaneModel)  : string * int =
    let s =
      diff.Lines
      |> Seq.fold (fun acc line -> acc + (linePrefix line.Type) + line.Text + "\n") ""
    let n =
      diff.Lines
      |> Seq.fold (fun acc line -> acc + (tallyChange line.Type)) 0
    s, n

  static let print (diff: DiffPaneModel) =
    let mutable nChanges = 0
    diff.Lines
    |> Seq.iter (fun line ->
      let savedColor = Console.ForegroundColor
      match line.Type with
      | ChangeType.Unchanged ->                                                 Console.Write "  "
      | ChangeType.Inserted  -> Console.ForegroundColor <- ConsoleColor.Red   ; Console.Write "x "
                                Console.ForegroundColor <- savedColor
                                nChanges <- nChanges + 1
      | ChangeType.Deleted   -> Console.ForegroundColor <- ConsoleColor.Green ; Console.Write "√ "
                                Console.ForegroundColor <- savedColor
                                nChanges <- nChanges + 1
      | ChangeType.Modified 
      | ChangeType.Imaginary -> failwith "never produced by DiffPlex, apparently.  Right?"
      | _                    -> failwith "unrecognized ChangeType"
      Console.WriteLine line.Text )
    nChanges


  // for F# callers

  static member diffToString (expected: string) (ignoreWhiteSpace: bool) (actual: string)  : Result * string * int =
    match expected = actual, ignoreWhiteSpace with
    | true , _ -> Result.Same, "", 0
    | false, ignoreWhiteSpace ->
    let diff = InlineDiffBuilder.Diff(expected, actual, ignoreWhiteSpace)
    let s, nChanges = getDiffString diff
    match nChanges <> 0 with
    | false    -> Result.Same   , "", 0
    | true     -> Result.Changed, s , nChanges

  static member diffToConsole (expected: string) (ignoreWhiteSpace: bool) (actual: string)  : Result * int =
    match expected = actual, ignoreWhiteSpace with
    | true , _ -> Result.Same, 0
    | false, ignoreWhiteSpace ->
    let diff = InlineDiffBuilder.Diff(expected, actual, ignoreWhiteSpace)
    match diff.HasDifferences with
    | false    -> Result.Same, 0
    | true     -> let nChanges = print diff
                  Result.Changed, nChanges

  static member runToString (expected: string) (ignoreWhiteSpace: bool) (actual: string) =
    match Diff.diffToString expected ignoreWhiteSpace actual with
    | Result.Same   , _, _ -> printfn "Test passed"
    | Result.Changed, s, n -> printfn $"{s}––––––––––––––\nTest failed: {n} differences"
    | _ -> failwith "unreachable"
  static member runToStringNotIgnoring (expected: string) (actual: string) = Diff.runToString expected false actual
  static member runToStringIgnoring    (expected: string) (actual: string) = Diff.runToString expected true  actual
      
  static member runToConsole (expected: string) (ignoreWhiteSpace: bool) (actual: string) =
    match Diff.diffToConsole expected ignoreWhiteSpace actual with
    | Result.Same   , _ -> printfn "Test passed"
    | Result.Changed, n -> printfn $"––––––––––––––\nTest failed: {n} differences"
    | _ -> failwith "unreachable"
  static member runToConsoleNotIgnoring (expected: string) (actual: string) = Diff.runToConsole expected false actual
  static member runToConsoleIgnoring    (expected: string) (actual: string) = Diff.runToConsole expected true  actual


  // for C# callers
  
  static member Diff (expected: string, actual: string, ?ignoreWhiteSpace: bool)  : Result * string * int =
    Diff.diffToString expected (defaultArg ignoreWhiteSpace false) actual

  static member DiffToConsole (expected: string, actual: string, ?ignoreWhiteSpace: bool)  : Result * int =
    Diff.diffToConsole expected (defaultArg ignoreWhiteSpace false) actual
