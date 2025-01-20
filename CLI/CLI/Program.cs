using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Security;
//bundle
static List<string> endFiles(string files)
{
    string[] arr = files.Split(',');
    List<string> result = new List<string>();
    Dictionary<string, string> endsFiles = new Dictionary<string, string>()
    {
        {"C#" ,".cs"},
        {"JAVASCRIPT",".js" },
        {"HTML",".html" },
        {"CSS",".css" },
        {"PYTHON",".py" },
        {"C++",".cpp" },
        {"TYPESCRIPT",".ts" },
        {"C",".c" }
    };
    if (files == "all")
        return endsFiles.Values.ToList();
    foreach (string file in arr)
    {
        if (endsFiles.ContainsKey(file))
            result.Add(endsFiles[file]);
        else
            Console.WriteLine($"EROR::the language {file} does not exist");
        if (file == "C++") result.Add(".h");

    }
    return result;
}
static string CreateDestinationFile(FileInfo output)
{
    FileStream file = null;
    try
    {
        if (output != null)
            using (file = File.Create(output.FullName)) { }
        else
            using (file = File.Create("bundle.txt")) { }
        Console.WriteLine("file was created");
    }
    catch (DirectoryNotFoundException)
    {
        Console.WriteLine("Error: file path is invalid");
        return null;
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine("Error: file path is invalid");
        return null;
    }
    return file.Name;
}
static void getFiles(string directory, List<string> files, List<string> ending, List<string>ignore)
{
    if (!ignore.Contains(directory))
    {
        string[] file = Directory.GetFiles(directory);
        foreach (string f in file)
        {
            if (ending.Contains(Path.GetExtension(f)))
                files.Add(f);
        }
        string[] directorys = Directory.GetDirectories(directory);
        foreach (string item in directorys)
        {
            getFiles(item, files, ending, ignore);
        }
    }
}
static void sorting(List<string> files, bool sort)
{
    if (!sort)
    {
        files.Sort((x, y) =>
        {
            string extX = Path.GetFileName(x);
            string extY = Path.GetFileName(y);
            return extX.CompareTo(extY);
        });
    }
    else
    {
        files.Sort((x, y) =>
        {
            string extX = Path.GetExtension(x);
            string extY = Path.GetExtension(y);
            return extX.CompareTo(extY);
        });
    }
}
static void mainFunc(string src, List<string> files, bool note, bool sort, bool remove, string author)
{
    if (files == null || files.Count == 0 || string.IsNullOrEmpty(src))
    {
        Console.WriteLine("Invalid input parameters.");
        return;
    }
    sorting(files, sort);
    using (StreamWriter fs = new StreamWriter(src))
    {
        if (author != null) fs.WriteLine(author);
        foreach (string f in files)
        {
            if (note) fs.WriteLine($"// {f}");
            using (StreamReader r = new StreamReader(f))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    if (remove)
                    {
                        if (!String.IsNullOrWhiteSpace(line))
                            fs.WriteLine(line);
                    }
                    else
                    {
                        fs.WriteLine(line);
                    }
                }
            }
        }
    }
}
//create-rsp
static void makeAndInput()
{
    string str = " bundle";
    Console.WriteLine("enter a programing languages that you want to bundle");
    str += " -l "; str += Console.ReadLine();
    Console.WriteLine("if you want enter name or location");
    if (Console.ReadLine() != "")
    { str += " -o "; str += Console.ReadLine(); }
    Console.WriteLine("enter y/n if you want note");
    if (Console.ReadLine() == "y")
        str += " -n";
    Console.WriteLine("enter y/n if you want sort by the ending");
    if (Console.ReadLine() == "y")
        str += " -s";
    Console.WriteLine("enter y/n if you want remove empty line");
    if (Console.ReadLine() == "y")
        str += " -r";
    Console.WriteLine("if you want enter name of the author");
    if (Console.ReadLine() != "")
    { str += " -a "; str += Console.ReadLine(); }
    File.WriteAllText("response.rsp", str);

}
//main
var bundleCommand = new Command("bundle", "a commad that bundle a code files to single file");
var langOption = new Option<string>(new[] { "--languages", "-l" }, "list of language") { IsRequired = true };
var outputOption = new Option<FileInfo>(new[] { "--output", "-o" }, "new location");
var noteOption = new Option<bool>(new[] { "--note", "-n" }, "current location");
var sortOption = new Option<bool>(new[] { "--sort", "-s" }, "type of sort");
var removeLinesOption = new Option<bool>(new[] { "--remove-empty-lines", "-r" }, "remove empty lines");
var authorOption = new Option<string>(new[] { "--author", "-a" }, "author");

bundleCommand.AddOption(langOption);
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeLinesOption);
bundleCommand.AddOption(authorOption);
bundleCommand.SetHandler((languages, output, note, sort, remove, author) =>
{
    List<string> ignores = new List<string>()
        {
             "bin", "Debug", "Release", "build", "node_modules", "dist",
             "__pycache__", "*.py[cod]", "venv", "env", "out"
        };

    string bundle = CreateDestinationFile(output);
    Console.WriteLine(bundle);
    List<string> ending = endFiles(languages);
    List<string> rightFiles = new List<string>();
    getFiles(Directory.GetCurrentDirectory(), rightFiles, ending, ignores);
    mainFunc(bundle, rightFiles, note, sort, remove, author);
}, langOption, outputOption, noteOption, sortOption, removeLinesOption, authorOption);
var rspCommand = new Command("create-rsp", "a command that help to user");
rspCommand.SetHandler(() =>
{
    makeAndInput();
});
var rootCommand = new RootCommand("root command CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(rspCommand);
rootCommand.InvokeAsync(args);

