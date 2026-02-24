module DiffTest.ExpectedOutput

(*
    Input:
    expected  actual
    --------  ------
    yes$      no$
    yes$      no$
    what$     not$
    where$    what $
    why$      but$
    when$     why
*)

let expectedOutput = """SideBySide (default):
x no
√ yes
x no
√ yes
x not
√ what
x what 
√ where
x but
  why
√ when
√ 
––––––––––––––
Test failed: 11 differences

SideBySide ignoring whitespace:
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
√ 
––––––––––––––
Test failed: 9 differences

Inline:
x no
x no
x not
x what 
√ yes
√ yes
√ what
√ where
x but
  why
√ when
√ 
––––––––––––––
Test failed: 11 differences

Inline ignoring whitespace:
x no
x no
√ yes
√ yes
x not
  what
x but
√ where
  why
√ when
√ 
––––––––––––––
Test failed: 9 differences
"""
