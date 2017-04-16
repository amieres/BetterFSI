# BetterFSI
A utility script for improving the F# Interactive experience, specially when working with multi-file scripts. For Visual Studio.

# The Concept

Entering code directly in the FSI REPL is not usual practice because it is hard to keep track of what has been sent and in what order. Lack of syntax highliting and intellisense does not help either. Also as the code scrolls up it gets lost and it becomes difficult to fetch and maintain. Only the simplest tasks can be done this way.

It is then a good idea to maintain the code in FSX file(s) and from there send it to the REPL. This involves using CTRL-A a lot to select the whole file and then sending it to the FSI. This is not difficult but as the code grows and is done repeatedly it becomes less comfortable because of all the scrolling back. When several files are involved the task becomes rather cumbersome.

The purpose of this script is to facilitate and automate this task. **BetterFSI** keeps track of file dependencies and automatically refreshes the code in the REPL as needed.

## Separating Code and Invocation

Working with scripts sometimes involves creating actual code and then calling it with specific parameters. As you make changes to the code and send them to the REPL you may not want it to execute all the calls every time because they may be slow, have side effects or there could be many which are innecessary. One way of dealing with this is to select only the part of code that you want to send but this is even harder than selecting all with CTRL-A.

A good idea then is to keep the calls commented out between `(*  *)`, that way they can be sent together with the code without actually executing them. Then when they need to be invoked it is easy to send just the ones needed with a couple of keystrokes or mouse clicks.

That is the way **BetterFSI** is intended to be used.

# Usage:
- Place **BetterFSI.fsx** among your script files.
- Add the following comment somewhere in your own script file(s):

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

- Execute the #load line once to activate the script.
- Put your calls after the `Do __SOURCE_FILE__ __LINE__ //`
- Execute one (or several) whole line with Right-Click *Execute in Interactive*

When any of your files changes, they will be reloaded automaticaly just before executing your command.

## Caveats: 
- It uses SendKeys so it is better to use the mouse than the shortcut as they keystrokes sometimes may interfere and produce undesirable results.
- It works only for Visual Studio

Enjoy!
