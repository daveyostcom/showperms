

open DiffResults
open DiffTest
open ExpectedOutput

let runToConsole (expected: string) (actual: string) = Diff.RunToConsole(expected, actual, ignoreWhiteSpace = false, mode = DiffMode.LineByLine)

let diffTest runDiff = 
  let expected = "yes\nyes\nwhat\nwhere\nwhy\nwhen\n"
  let actual = "no\nno\nnot\nwhat \nbut\nwhy"
  let actualOutput = 
    let sb = System.Text.StringBuilder()
    sb.AppendLine("SideBySide (default):") |> ignore
    let diff1 = Diff.RunToString(expected, actual, ignoreWhiteSpace = false, mode = DiffMode.LineByLine)
    sb.Append(diff1) |> ignore
    sb.AppendLine("\nSideBySide ignoring whitespace:") |> ignore
    let diff2 = Diff.RunToString(expected, actual, ignoreWhiteSpace = true, mode = DiffMode.LineByLine)
    sb.Append(diff2) |> ignore
    sb.AppendLine("\nInline:") |> ignore
    let diff3 = Diff.RunToString(expected, actual, ignoreWhiteSpace = false, mode = DiffMode.Chunked)
    sb.Append(diff3) |> ignore
    sb.AppendLine("\nInline ignoring whitespace:") |> ignore
    let diff4 = Diff.RunToString(expected, actual, ignoreWhiteSpace = true, mode = DiffMode.Chunked)
    sb.Append(diff4) |> ignore
    sb.ToString()
  runDiff ExpectedOutput.expectedOutput actualOutput

let run() =
  let expected = "yes\nyes\nwhat\nwhere\nwhy\nwhen\n"
  let actual   = "no\nno\nnot\nwhat \nbut\nwhy"
  printfn "\nSideBySide (default):"            ; Diff.RunToConsole (expected, actual, ignoreWhiteSpace = false, mode = DiffMode.LineByLine)
  printfn "\nSideBySide ignoring whitespace:" ; Diff.RunToConsole (expected, actual, ignoreWhiteSpace = true , mode = DiffMode.LineByLine)
  printfn "\nInline:"                          ; Diff.RunToConsole (expected, actual, ignoreWhiteSpace = false, mode = DiffMode.Chunked)
  printfn "\nInline ignoring whitespace:"     ; Diff.RunToConsole (expected, actual, ignoreWhiteSpace = true , mode = DiffMode.Chunked)

[<EntryPoint>]
let main argv =
  diffTest runToConsole        
  0
