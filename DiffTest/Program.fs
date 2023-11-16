
let run() =
  let expected = "what\nwhere\nwhy\nwhen\n"
  let actual   = "not\nwhat \nbut\nwhy"
  DiffResults.Diff.runToConsoleNotIgnoring expected actual
  DiffResults.Diff.runToConsoleIgnoring    expected actual

[<EntryPoint>]
let main _ =
  run()
  0

(*
Output:
√ what
√ where
x not
x what
x but
  why
√ when
√ 
––––––––––––––
Test failed: 7 differences
Output:
x not
  what
√ where
x but
  why
√ when
√ 
––––––––––––––
Test failed: 5 differences
*)
