open System

let mutable ExecuteInteractiveKeys = "^%f"
let mutable TestFile = __SOURCE_DIRECTORY__ + (System.IO.Path.DirectorySeparatorChar.ToString()) + "__Test__.fsx"

let mutable Verbose = true

let userTimerWithCallback ms = 
    let event = new System.Threading.AutoResetEvent(false)
    let timer = new System.Timers.Timer(ms)
    timer.Elapsed.Add (fun _ -> event.Set() |> ignore )
    //printfn "Waiting for timer at %O" DateTime.Now.TimeOfDay
    timer.Start()
    //printfn "Doing something useful while waiting for event"
    event.WaitOne() |> ignore
    timer.Stop()
    //printfn "Timer ticked at %O" DateTime.Now.TimeOfDay

let Copy        txt = System.Windows.Forms.Clipboard.SetText txt
                      if Verbose then printf "Copied to clipboard: %s...\n\n" <| txt.[..min (txt.Length - 1) 100]
let Paste ()        = System.Windows.Forms.Clipboard.GetText()
let SendKeys        = System.Windows.Forms.SendKeys.SendWait
let keysRaw: string -> string =
    fun      txt    -> txt.Split '\n'
                       |> Array.map (String.collect (fun c -> if c = ' ' then " " else sprintf "{%c}" c))
                       |> String.concat "{ENTER}"

let sendToFsi: string -> unit =
    fun        text   -> if text <> "" then
                            //printfn "---->\n%s\n<------" text
                            SendKeys ExecuteInteractiveKeys
                            SendKeys <| keysRaw text
                            SendKeys "{ENTER};;{ENTER}"
                            SendKeys "^{TAB}"
                            userTimerWithCallback 200.

type State = {
        loadedFiles: Map<string, LoadedFile>
        fsiCommands: string[]
    }
with
    static member New()  = { loadedFiles = Map [] ; fsiCommands = [||] }
    member this.LoadedFiles                = this.loadedFiles
    member this.GetFileO       name        = this.loadedFiles |> Map.tryFind name
    member this.AddFile:       LoadedFile -> State =
                fun            file       -> { this with loadedFiles = this.loadedFiles |> Map.add file.Name file }
    member this.AddFsiCommands:string[]   -> State =
                fun            commands   -> { this with fsiCommands = Array.append this.fsiCommands commands}
    member this.AddFsiCommand  command     = this.AddFsiCommands [| command |]
    member this.DoCommands   : string     -> State =
                fun         cmd           -> [  
                                                if this.fsiCommands |> Array.isEmpty |> not then
                                                    this.fsiCommands 
                                                    |> Array.partition (fun l -> l.StartsWith "#r " || l.StartsWith "#I ")
                                                    |> fun (fs, sd) -> System.IO.File.WriteAllLines(TestFile, Array.append fs sd)
                                                    yield  "#load @" + "\"" + TestFile +  "\" "
                                                    yield  "open __Test__"
                                                    yield! this.fsiCommands |> Array.filter (fun l -> l.StartsWith "open ")
                                             ]
                                             |> String.concat "\n"
                                             |> sendToFsi
                                             { this with fsiCommands = [||] }
    member this.NeedReload:    string     -> bool  =
                fun            file       ->
                    match this.GetFileO file with
                    | None       -> true
                    | Some lFile -> lFile.NeedReload this

and LoadedFile = 
    LoadedFile of 
        name        : string
      * modified    : DateTime
      * dependencies: string Set
      * content     : string[]
with
    member this.FullName      = match this with LoadedFile(name, modified, dependencies, content) -> name
    member this.Modified      = match this with LoadedFile(name, modified, dependencies, content) -> modified
    member this.Dependencies  = match this with LoadedFile(name, modified, dependencies, content) -> dependencies
    member this.Content       = match this with LoadedFile(name, modified, dependencies, content) -> content
    member this.IsOld         = System.IO.File.GetLastWriteTime this.FullName <> this.Modified
    member this.Name          = System.IO.Path.GetFileName      this.FullName
    member this.ContentForFsi: bool    -> string [] = 
                fun             indent ->
                    this.Content
                    |> Array.map (fun l -> 
                        if l.StartsWith "#load " then @"//" + l 
                        else if l.StartsWith "#" then l 
                        else if indent then "    " + l
                        else l) 
                    |> Array.append 
                         [| 
                            if indent then 
                                yield sprintf "module %s = " (System.IO.Path.GetFileNameWithoutExtension this.FullName) 
                            yield     sprintf "#line 1 \"%s\"" this.Name
                         |]
    member this.NeedReload: State -> bool = 
                fun         state -> this.IsOld || this.Dependencies |> Seq.exists state.NeedReload
    member this.GetLineO:   int   -> string option =
                fun         line  -> this.Content |> Array.tryItem line

let getDependencies: string[] -> string Set = 
    fun              content  ->
        content
        |> Seq.filter (fun l -> l.StartsWith "#load ")
        |> Seq.map    (fun l -> l.Split('"').[1])
        |> Seq.map    System.IO.Path.GetFileName
        |> Set.ofSeq

let fixPath: string -> string =
    fun      file   -> if System.IO.Path.IsPathRooted file then file
                       else __SOURCE_DIRECTORY__ + (System.IO.Path.DirectorySeparatorChar.ToString()) + file
                       //|> fun s -> printfn "%s" s ; s

let rec loadFile_: State -> bool   -> string -> State =
        fun        state1   indent    file   ->
            let fileName = fixPath file 
            let content  = System.IO.File.ReadAllLines fileName
            let deps     = getDependencies content
            let newFile  = LoadedFile(fileName, System.IO.File.GetLastWriteTime fileName, deps, content)
            let state2   = newFile |> state1.AddFile
            let stateNp1 = deps |> Seq.fold (fun (stateN:State) file -> if stateN.NeedReload file then loadFile_ stateN true file else stateN) state2
            stateNp1.AddFsiCommands <| newFile.ContentForFsi indent

let mutable globalState: State = State.New()

let loadFile:   bool   -> string -> unit =
    fun         indent    file   -> let state2 = loadFile_ globalState indent file
                                    globalState <- state2.DoCommands ""

let doThis:     bool   -> string -> string -> unit =
    fun         indent    file      line   -> 
        SendKeys "^+S"
        userTimerWithCallback 400.
        if globalState.NeedReload  file then
            loadFile indent file 
        globalState.GetFileO file
            |> Option.bind (fun lf -> lf.GetLineO (int line - 1))
            |> Option.bind (fun ln -> let n = ln.IndexOf @"//" 
                                      if n < 0 then None else ln.Substring (n+2) |> Some)
            |> Option.iter (fun cmd -> if cmd.Trim() <> "" then cmd.Trim() |> sendToFsi)


(*
(* don't remove *)   #load "BetterFSI.fsx"
BetterFSI.loadFile      "EMS_MSB_VW_FIN_TRANS.fsx"
__SOURCE_FILE__
BetterFSI.loadFile      "SlowlyChangingDimensionPlus.fsx"
BetterFSI.loadFile      "FieldDecl.fsx"
BetterFSI.loadFile      "FieldDefault.fsx"
BetterFSI.loadFile      "Fields.fsx"
BetterFSI.loadFile      "BetterFSI.fsx"
BetterFSI.globalState.GetFileO "HPL_VW_FIN_TRANS.fsx"  |> Option.bind (fun lf -> lf.GetLineO (int "3"))
BetterFSI.globalState.GetFile "SlowlyChangingDimensionPlus.fsx" |> Option.get |> BetterFSI.needReload BetterFSI.state 
BetterFSI.state.GetFile "EMS_MSB_VW_FIN_TRANS.fsx"        |> Option.get |> fun f -> f.IsOld
BetterFSI.checkModified "BetterFSI.fsx"

sendToFsi """printf "hello\n\n\n";; """
*)

let path = __SOURCE_DIRECTORY__
let file = __SOURCE_FILE__
let line = __LINE__

#if INTERACTIVE
let msg = "Interactive"
sendToFsi "let Do f l = BetterFSI.doThis false f l"
#else
let msg = "Not Interactive"
#endif

printfn "%s" msg

