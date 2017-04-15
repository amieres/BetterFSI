# BetterFSI
A utility script for improving the F# Interactive experience

# Usage:
Place **BetterFSI.fsx** in the same directory as your script files.
Add this somewhere in your script file(s), maybe at the top or the bottom:

```F#
(*          
(*keep*)#load "BetterFSI.fsx"  // <<<==== Execute first in F# Interactive

Do __SOURCE_FILE__ __LINE__ //   HPLView.Query |> BetterFSI.Copy 
Do __SOURCE_FILE__ __LINE__ //   CARRIER_PROCEDURE_SRC
Do __SOURCE_FILE__ __LINE__ //
*)
```

Prefix your test commands with `Do __SOURCE_FILE__ __LINE__ //`
and execute them with Right-Click `Execute in Interactive`

If some of your files changed, they will be reloaded automaticaly before executing your command.
Enjoy!
