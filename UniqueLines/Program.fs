open System
open System.IO
open Newtonsoft.Json

type Conifg = { DefaultDirectory: string }

type String with
    member this.IsNullOrWhiteSpace() = String.IsNullOrWhiteSpace(this)

let loadConfig () =
    let configPath = "cnfig.json"

    if File.Exists(configPath) then
        let configJson = File.ReadAllText(configPath)
        JsonConvert.DeserializeObject<Conifg> configJson
    else
        { DefaultDirectory = Directory.GetCurrentDirectory() }

let getNewestFiles (dir: string) =
    let files =
        Directory.GetFiles(dir, "*.txt")
        |> Array.filter (fun file -> not (Path.GetFileName(file).StartsWith("AUTO")))
        |> Array.sortByDescending File.GetLastWriteTime
        |> Array.truncate 2

    if files.Length < 2 then
        printfn $"There are less than 2 .txt files in %s{dir}"
        None
    else
        Some files

let readLines (filePath: string) =
    File.ReadAllLines(filePath) |> List.ofArray

let writeLines (filePath: string) (lines: list<string>) =
    File.WriteAllLines(filePath, lines |> Array.ofList)

let processFiles (file1: string) (file2: string) (outputFile: string) =
    let content1 = readLines file1
    let content2 = readLines file2
    
    let set1 = Set.ofList content1
    let set2 = Set.ofList content2
    let intersection = Set.intersect set1 set2

    let uniqueContent =
        content1 @ content2
        |> List.filter (fun line -> not (intersection.Contains line))

    writeLines outputFile uniqueContent
    printfn $"File %s{outputFile} created with %d{uniqueContent.Length} unique lines"

let config = loadConfig ()

let rec processUserInput () =

    printf "Enter first file path or press enter to use default: "
    let file1 = Console.ReadLine()

    printf "Enter second file path or press enter to use default: "
    let file2 = Console.ReadLine()

    printf "Enter output file name or press enter to use default: "
    let output = Console.ReadLine()

    let defaultFiles = getNewestFiles config.DefaultDirectory

    let filePath1 =
        if file1.IsNullOrWhiteSpace() && defaultFiles.IsSome then
            defaultFiles.Value[0]
        else
            file1

    let filePath2 =
        if file2.IsNullOrWhiteSpace() && defaultFiles.IsSome then
            defaultFiles.Value[1]
        else
            file2

    let outputPath =
        if output.IsNullOrWhiteSpace() then
            $"AUTO_{Path.GetFileNameWithoutExtension(filePath1)}_{Path.GetFileNameWithoutExtension(filePath2)}.txt"
        else
            $"AUTO_{output}.txt"
               
    if not (filePath1.IsNullOrWhiteSpace()) && not (filePath2.IsNullOrWhiteSpace()) && 
       File.Exists(filePath1) && File.Exists(filePath2) then
        processFiles filePath1 filePath2 outputPath
    else
        printfn $"File %s{filePath1} or %s{filePath2} does not exist"
       
    printfn "Do you want to process another pair of files? (y/n)"
    let processFurther = Console.ReadLine()

    if processFurther.ToLower() = "y" || processFurther.IsNullOrWhiteSpace() then
        processUserInput()
    
processUserInput()