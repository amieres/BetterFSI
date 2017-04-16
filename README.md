# BetterFSI
A utility script for improving the F# Interactive experience, specially for multi-file scripts.

# Usage:
Place **BetterFSI.fsx** in the same directory as your script files.
Add the following comment somewhere in your script file(s), maybe at the top or the bottom:

```F#
(*          
(*keep*)#load "BetterFSI.fsx"  // <<<==== Execute first in F# Interactive
Do __SOURCE_FILE__ __LINE__ //
Do __SOURCE_FILE__ __LINE__ //   sprintf "Hello %s" System.Windows.Forms.SystemInformation.UserName |> BetterFSI.Copy 
Do __SOURCE_FILE__ __LINE__ //   BetterFSI.Paste()
Do __SOURCE_FILE__ __LINE__ //   BetterFSI.Paste().[..6]
Do __SOURCE_FILE__ __LINE__ //   let f = new System.Windows.Forms.Form() ;; f.Text <- BetterFSI.Paste() ;; f.ShowDialog()
Do __SOURCE_FILE__ __LINE__ //
*)
```

Execute the #load line once at the beginning.
Prefix your test commands with `Do __SOURCE_FILE__ __LINE__ //`
and execute them with Right-Click `Execute in Interactive`

When any of your files changes, they will be reloaded automaticaly just before executing your command.

## Caveats: 
- it uses SendKeys so it is better to use the mouse than the shortcut as they keystrokes may interfere.

Enjoy!
