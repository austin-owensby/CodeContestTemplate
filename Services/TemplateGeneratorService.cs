using System.IO.Compression;
using System.Text;

namespace CodeContestTemplate.Services;

public class TemplateGeneratorService {
    private static Options OptionData { get; set; } = new();

    public static void GenerateTemplate(Options options) {
        OptionData = options;

        string workingFolder = OptionData.FormattedNameUpper!;

        // Clean up folder if it already exists
        if (Directory.Exists(workingFolder)) {
            Directory.Delete(workingFolder, true);
        }

        // Copy over top level folders
        CopyFile(".vscode/launch.json");
        CopyFile(".vscode/tasks.json");
        CopyFile(".gitignore");
        CopyFile("LICENSE");
        CopyFile("REPLACE.sln");
        
        // Generate project templates
        GenerateSharedTemplate();
        GenerateConsoleTemplate();
        GenerateWebAPITemplate();

        // Zip up results
        string zipOutput = $"{OptionData.FormattedNameUpper}.zip";
        File.Delete(zipOutput);
        ZipFile.CreateFromDirectory(workingFolder, zipOutput);

        // Clean up folder
        Directory.Delete(workingFolder, true);
    }

    private static void GenerateSharedTemplate() {
        CopyFile("Shared/Shared.csproj");
        CopyFile("Shared/Services/SolutionNotFoundException.cs");

        GenerateFileUtility();
        GenerateISolutionPuzzleService();
        GenerateSolutionService();
    }

    private static void GenerateFileUtility() {
        var sb = new StringBuilder();
        sb.AppendLine($"namespace {OptionData.FormattedNameUpper}.Services");
        sb.AppendLine("{");
        sb.AppendLine("\tpublic static class FileUtility");
        sb.AppendLine("\t{");
        sb.AppendLine("\t\t/// <summary>");
        sb.AppendLine("\t\t/// Get the input file line by line");
        sb.AppendLine("\t\t/// </summary>");

        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.AppendLine("\t\t/// <param name=\"year\"></param>");
            } else {
                sb.AppendLine("\t\t/// <param name=\"event\"></param>");
            }
        }

        sb.AppendLine("\t\t/// <param name=\"puzzle\"></param>");

        if (OptionData.SeparateInputs == true) {
            sb.AppendLine("\t\t/// <param name=\"part\"></param>");
        }

        sb.AppendLine("\t\t/// <param name=\"example\">Defaults to false, if true will pull an example file.</param>");
        sb.Append("\t\t/// <remarks>Ex. GetInputLines(");

        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.Append($"{DateTime.Now.Year}, ");
            } else {
                sb.Append($"\"{OptionData.EventDictionary.First().Key}\", ");
            }
        }

        sb.Append('1');

        if (OptionData.SeparateInputs == true) {
            sb.Append(", 1");
        }
        
        sb.Append(") reads the data from \"/Inputs/");

        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.Append($"{DateTime.Now.Year}/");
            } else {
                sb.Append($"\"{OptionData.EventDictionary.First().Key}\"/");
            }
        }

        sb.Append("1".PadLeft((int)(1 + Math.Floor(Math.Log10(OptionData.TotalPuzzles!.Value))), '0'));

        if (OptionData.SeparateInputs == true) {
            sb.Append("/1");
        }

        sb.Append(".txt\". GetInputLines(");

        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.Append($"{DateTime.Now.Year}, ");
            } else {
                sb.Append($"\"{OptionData.EventDictionary.First().Key}\", ");
            }
        }

        sb.Append('1');

        if (OptionData.SeparateInputs == true) {
            sb.Append(", 1");
        }

        sb.Append(", true) reads the data from \"/Inputs/");

        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.Append($"{DateTime.Now.Year}/");
            } else {
                sb.Append($"\"{OptionData.EventDictionary.First().Key}\"/");
            }
        }

        sb.Append("1".PadLeft((int)(1 + Math.Floor(Math.Log10(OptionData.TotalPuzzles!.Value))), '0'));

        if (OptionData.SeparateInputs == true) {
            sb.Append("/1");
        }

        sb.AppendLine("_example.txt\"</remarks>");
        sb.AppendLine("\t\t/// <returns></returns>");
        sb.Append("\t\tpublic static List<string> GetInputLines(");

        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.Append("int year, ");
            } else {
                sb.Append("string event, ");
            }
        }

        sb.Append("int puzzle, ");

        if (OptionData.SeparateInputs == true) {
            sb.Append("int part, ");
        }

        sb.AppendLine("bool example = false)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tstring directoryPath = Directory.GetParent(Environment.CurrentDirectory)!.FullName;");
        sb.Append("\t\t\tstring filePath = Path.Combine(directoryPath, \"Inputs\", ");

        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.Append("year.ToString(), ");
            } else {
                sb.Append("event, ");
            }
        }

        sb.Append($"$\"{{puzzle:D{1 + Math.Floor(Math.Log10(OptionData.TotalPuzzles!.Value))}}}");

        if (OptionData.SeparateInputs == true) {
            sb.Append("\", $\"{part.ToString()}");
        }

        sb.AppendLine("{(example ? \"_example\" : string.Empty)}.txt\");");
        sb.AppendLine("\t\t\treturn File.ReadAllLines(filePath).ToList();");
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");
        sb.AppendLine("}");

        string path = $"{OptionData.FormattedNameUpper}/Shared/Services/FileUtility.cs";
        File.WriteAllText(path, sb.ToString());
        Console.WriteLine($"Wrote file: {path}");
    }

    private static void GenerateISolutionPuzzleService() {
        var sb = new StringBuilder();
        sb.AppendLine($"namespace {OptionData.FormattedNameUpper}.Services");
        sb.AppendLine("{");
        sb.AppendLine("\tpublic interface ISolutionPuzzleService");
        sb.AppendLine("\t{");

        if (OptionData.PartsPerPuzzle == 1) {
            sb.AppendLine("\t\t/// <summary>");
            sb.AppendLine("\t\t/// Execute's the puzzle's solution");
            sb.AppendLine("\t\t/// </summary>");
            sb.AppendLine("\t\t/// <param name=\"example\"></param>");
            sb.AppendLine("\t\t/// <returns></returns>");
            sb.AppendLine("\t\tstring RunSolution(bool example);");
        }
        else {
            for (int i = 1; i <= OptionData.PartsPerPuzzle; i++) {
                sb.AppendLine("\t\t/// <summary>");
                sb.AppendLine($"\t\t/// Execute's the puzzle's part {i} solution");
                sb.AppendLine("\t\t/// </summary>");
                sb.AppendLine("\t\t/// <param name=\"example\"></param>");
                sb.AppendLine("\t\t/// <returns></returns>");
                sb.AppendLine($"\t\tstring RunPart{i}Solution(bool example);");

                if (i != OptionData.PartsPerPuzzle) {
                    sb.AppendLine();
                }
            }
        }
            
        sb.AppendLine("\t}");
        sb.AppendLine("}");

        string path = $"{OptionData.FormattedNameUpper}/Shared/Services/ISolutionPuzzleService.cs";
        File.WriteAllText(path, sb.ToString());
        Console.WriteLine($"Wrote file: {path}");
    }

    private static void GenerateSolutionService() {
        var sb = new StringBuilder();
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection; // This is needed for the Console App");
        sb.AppendLine("using System.Diagnostics;");
        sb.AppendLine($"using {OptionData.FormattedNameUpper}.Gateways;");
        sb.AppendLine();
        sb.AppendLine($"namespace {OptionData.FormattedNameUpper}.Services");
        sb.AppendLine("{");
        sb.AppendLine($"\tpublic class SolutionService(IServiceProvider serviceProvider, {OptionData.FormattedNameUpper}Gateway {OptionData.FormattedNameLower}Gateway)");
        sb.AppendLine("\t{");
        sb.AppendLine("\t\tprivate readonly IServiceProvider serviceProvider = serviceProvider;");
        sb.AppendLine($"\t\tprivate readonly {OptionData.FormattedNameUpper}Gateway {OptionData.FormattedNameLower}Gateway = {OptionData.FormattedNameLower}Gateway;");
        sb.AppendLine();
            
        sb.AppendLine("\t}");
        sb.AppendLine("}");

        string path = $"{OptionData.FormattedNameUpper}/Shared/Services/SolutionService.cs";
        File.WriteAllText(path, sb.ToString());
        Console.WriteLine($"Wrote file: {path}");
    }

    private static void GenerateConsoleTemplate() {
        CopyFile("Console/Console.csproj");
    }

    private static void GenerateWebAPITemplate() {
        CopyFile("WebAPI/WebAPI.csproj");
    }

    private static void CopyFile(string path) {
        try {
            string source = $"TemplateFiles/{path}";
            string destination = $"{OptionData.FormattedNameUpper}/{path.Replace("REPLACE", OptionData.FormattedNameUpper)}";
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(source, destination);
            ReplaceTextInFile(destination);
            Console.WriteLine($"Wrote file: {path}");
        } catch (Exception) {
            Console.WriteLine($"Failed to copy file {path}");
            throw;
        }
    }

    private static void ReplaceTextInFile(string path) {
        string text = File.ReadAllText(path);
        text = text.Replace("REPLACE", OptionData.FormattedNameUpper);
        File.WriteAllText(path, text);
    }
}