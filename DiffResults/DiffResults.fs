namespace DiffResults

open System
open System.Text

module DiffResults =

  type DiffLine =
  | UnchangedLine of string
  | ExpectedLine  of string  // Line that was expected but not in actual (should be marked with √)
  | ActualLine    of string  // Line that was in actual but not expected (should be marked with x)

  let unchgd = "  "
  let expect = "√ "
  let actual = "x "
  
  let getDiffStringFromDiffLines diffLines  : string * int =
    let sb = StringBuilder()
    let mutable nChanges = 0
    for diffLine in diffLines do
      match diffLine with
      | UnchangedLine text -> sb.Append(unchgd + text + "\n") |> ignore
      | ExpectedLine  text -> sb.Append(expect + text + "\n") |> ignore ; nChanges <- nChanges + 1
      | ActualLine    text -> sb.Append(actual + text + "\n") |> ignore ; nChanges <- nChanges + 1
    sb.ToString(), nChanges

  let printDiffLines diffLines  : int =
    let savedColor = Console.ForegroundColor
    let mutable nChanges = 0
    for diffLine in diffLines do
      let (prefix , color , text , isAChange) =
        match diffLine with
        | UnchangedLine t -> (unchgd, savedColor        , t, false)
        | ExpectedLine  t -> (expect, ConsoleColor.Green, t, true )
        | ActualLine    t -> (actual, ConsoleColor.Red  , t, true )
      Console.ForegroundColor <- color
      Console.Write prefix
      Console.ForegroundColor <- savedColor
      Console.WriteLine text
      if isAChange then nChanges <- nChanges + 1
    nChanges