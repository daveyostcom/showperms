namespace DiffResults

open DiffPlex.DiffBuilder.Model
open DiffResults.DiffResults

module Chunked =

  let private getDiffLines (diff: DiffPaneModel)  : DiffLine seq = seq {
    for line in diff.Lines do
      match line.Type with
      | ChangeType.Unchanged -> yield UnchangedLine line.Text
      | ChangeType.Deleted   -> yield ActualLine    line.Text
      | ChangeType.Inserted  -> yield ExpectedLine  line.Text
      | ChangeType.Modified 
      | ChangeType.Imaginary -> failwith "ChangeType.Imaginary is never produced by InlineDiffBuilder, apparently. Right?"
      | x                    -> failwith $"unrecognized ChangeType: %A{x}" }
   

  let getDiffString (diff: DiffPaneModel)  : string * int =
    diff
    |> getDiffLines
    |> getDiffStringFromDiffLines

  let print (diff: DiffPaneModel)  : int =
    diff
    |> getDiffLines
    |> printDiffLines