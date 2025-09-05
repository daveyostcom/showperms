namespace DiffResults

open DiffPlex.DiffBuilder.Model
open DiffResults.DiffResults

module LineByLine =
    
  let private getDiffLines (diff: SideBySideDiffModel)  : DiffLine seq = seq {
    for i in 0 .. diff.OldText.Lines.Count - 1 do
      let oldLine = diff.OldText.Lines.[i]
      let newLine = diff.NewText.Lines.[i]
      match oldLine.Type, newLine.Type with
      | ChangeType.Unchanged, ChangeType.Unchanged ->  yield UnchangedLine oldLine.Text
      | ChangeType.Deleted  , ChangeType.Imaginary ->  yield ActualLine    oldLine.Text
      | ChangeType.Imaginary, ChangeType.Inserted  ->  yield ExpectedLine  newLine.Text
      | ChangeType.Deleted  , ChangeType.Inserted  ->  yield ActualLine    oldLine.Text
                                                       yield ExpectedLine  newLine.Text
      | ChangeType.Modified , ChangeType.Modified  ->  yield ActualLine    oldLine.Text
                                                       yield ExpectedLine  newLine.Text
      | ChangeType.Imaginary, ChangeType.Imaginary ->  () // This is just padding in the side-by-side model, ignore.
      | combo -> failwith $"unrecognized ChangeType combination: %A{combo}" }

  let getDiffString (diff: SideBySideDiffModel)  : string * int =
    diff
    |> getDiffLines
    |> getDiffStringFromDiffLines

  let print (diff: SideBySideDiffModel)  : int =
    diff
    |> getDiffLines
    |> printDiffLines
