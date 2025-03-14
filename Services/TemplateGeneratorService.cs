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
        GenerateGateway();
    }

    private static void GenerateFileUtility() {
        StringBuilder sb = new();
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
        StringBuilder sb = new();
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
        StringBuilder sb = new();
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
        sb.AppendLine("\t\t/// <summary>");
        sb.AppendLine("\t\t/// Execute the specific solution based on the passed in parameters");
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

        sb.AppendLine("\t\t/// <returns></returns>");
        sb.AppendLine("\t\t/// <exception cref=\"SolutionNotFoundException\"></exception>");
        sb.Append("\t\tpublic async Task<string> GetSolution(");

        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.Append("int year, ");
            } else {
                sb.Append("string event, ");
            }
        }

        sb.Append("int puzzle, ");

        if (OptionData.PartsPerPuzzle > 1) {
            sb.Append("int part, ");
        }

        sb.AppendLine("bool send, bool example)");
        sb.AppendLine("\t\t{");
        sb.Append("\t\t\tConsole.WriteLine($\"Running solution for ");

        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.Append("year {year}, ");
            } else {
                sb.Append("event {event}, ");
            }
        }

        sb.Append("puzzle: {puzzle}, ");

        if (OptionData.SeparateInputs == true) {
            sb.Append("part: {part}, ");
        }

        sb.AppendLine("example: {(example ? \"yes\" : \"no\")}, submit: {(send ? \"yes\" : \"no\")}\");");
        sb.Append("\t\t\tISolutionPuzzleService service = FindSolutionService(");

        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.Append("year, ");
            } else {
                sb.Append("event, ");
            }
        }

        sb.AppendLine("puzzle);");
        sb.AppendLine();
        sb.AppendLine("\t\t\tStopwatch sw = Stopwatch.StartNew();");
        sb.AppendLine("\t\t\t// Run the specific solution");

        if (OptionData.PartsPerPuzzle == 1) {
            sb.AppendLine("\t\t\tstring answer = service.RunSolution(example);");
        }
        else {
            sb.AppendLine("\t\t\tstring answer;");

            for (int i = 0; i < OptionData.PartsPerPuzzle; i++) {
                if (i == 0) {
                    sb.AppendLine("\t\t\tif(part == 1){");
                    sb.AppendLine($"\t\t\t\tanswer = service.RunPart{i + 1}Solution(example);");
                    sb.AppendLine("\t\t\t}");
                }
                else if (i != OptionData.PartsPerPuzzle - 1) {
                    sb.AppendLine($"\t\t\telse if(part == {i + 1}){{");
                    sb.AppendLine($"\t\t\t\tanswer = service.RunPart{i + 1}Solution(example);");
                    sb.AppendLine("\t\t\t}");
                }
                else {
                    sb.AppendLine("\t\t\telse {");
                    sb.AppendLine($"\t\t\t\tanswer = service.RunPart{i + 1}Solution(example);");
                    sb.AppendLine("\t\t\t}");
                }
            }
        }

        sb.AppendLine("\t\t\tsw.Stop();");
        sb.AppendLine("\t\t\tConsole.WriteLine($\"Elapsed time: {sw.Elapsed}\");");
        sb.AppendLine();
        sb.AppendLine($"\t\t\t// Optionally submit the answer to {OptionData.HumanReadableName}");
        sb.AppendLine("\t\t\tif (send)");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\ttry");
        sb.AppendLine("\t\t\t\t{");
        sb.Append($"\t\t\t\t\tstring response = await {OptionData.FormattedNameLower}Gateway.SubmitAnswer(");

        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.Append("year, ");
            } else {
                sb.Append("event, ");
            }
        }

        sb.Append("puzzle, ");

        if (OptionData.PartsPerPuzzle > 1) {
            sb.Append("part, ");
        }

        sb.AppendLine("answer);");
        sb.AppendLine($"\t\t\t\t\tanswer = $\"Submitted answer: {{answer}}.\\n{OptionData.HumanReadableName} response: {{response}}\";");
        sb.AppendLine("\t\t\t\t}");
        sb.AppendLine("\t\t\t\tcatch (Exception e)");
        sb.AppendLine("\t\t\t\t{");
        sb.AppendLine($"\t\t\t\t\tConsole.WriteLine(\"An error occurred while submitting the answer to {OptionData.HumanReadableName}\");");
        sb.AppendLine($"\t\t\t\t\tanswer = $\"Submitted answer: {{answer}}.\\n{OptionData.HumanReadableName} response: {{e.Message}}\";");
        sb.AppendLine("\t\t\t\t}");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\treturn answer;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        sb.AppendLine("\t\t /// <summary>");
        sb.AppendLine("\t\t /// Fetch the specific service for the specified puzzle");
        sb.AppendLine("\t\t /// </summary>");
        
        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.AppendLine("\t\t/// <param name=\"year\"></param>");
            } else {
                sb.AppendLine("\t\t/// <param name=\"event\"></param>");
            }
        }
        
        sb.AppendLine("\t\t/// <param name=\"puzzle\"></param>");
        sb.AppendLine("\t\t/// <returns></returns>");
        sb.Append("\t\tprivate ISolutionPuzzleService FindSolutionService(");
        
        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.Append("int year, ");
            } else {
                sb.Append("string event, ");
            }
        }

        sb.AppendLine("int puzzle)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tIEnumerable<ISolutionPuzzleService> services = serviceProvider.GetServices<ISolutionPuzzleService>();");
        sb.AppendLine();
        sb.AppendLine($"\t\t\t// Use ':D{1 + Math.Floor(Math.Log10(OptionData.TotalPuzzles!.Value))}' to front pad 0s to less digit puzzles to match the formatting");
        sb.AppendLine($"\t\t\tstring serviceName = $\"{OptionData.FormattedNameUpper}.Services.Solution{{puzzle:D{1 + Math.Floor(Math.Log10(OptionData.TotalPuzzles!.Value))}}}Service\";");
        sb.AppendLine("\t\t\tISolutionPuzzleService? service = services.FirstOrDefault(s => s.GetType().ToString() == serviceName);");
        sb.AppendLine();
        sb.AppendLine("\t\t\t// If the service was not found, throw an exception");
        sb.AppendLine("\t\t\tif (service == null)");
        sb.AppendLine("\t\t\t{");
        sb.Append("\t\t\t\tthrow new SolutionNotFoundException($\"No solutions found for ");

        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.Append("year {year}, ");
            } else {
                sb.Append("event {event}, ");
            }
        }

        sb.AppendLine("puzzle: {puzzle}/\");");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\treturn service;");
        sb.AppendLine("\t\t}");

        sb.AppendLine("\t}");
        sb.AppendLine("}");

        string path = $"{OptionData.FormattedNameUpper}/Shared/Services/SolutionService.cs";
        File.WriteAllText(path, sb.ToString());
        Console.WriteLine($"Wrote file: {path}");
    }

    private static void GenerateGateway() {
        StringBuilder sb = new();
        sb.AppendLine("using System.Net;");
        sb.AppendLine("using HtmlAgilityPack;");
        sb.AppendLine();
        sb.AppendLine($"namespace {OptionData.FormattedNameUpper}.Gateways");
        sb.AppendLine("{");
        sb.AppendLine($"\tpublic class {OptionData.FormattedNameUpper}Gateway");
        sb.AppendLine("\t{");
        sb.AppendLine("\t\tprivate HttpClient? client;");
        sb.AppendLine("\t\tprivate readonly int throttleInMinutes = 3;");
        sb.AppendLine("\t\tprivate DateTimeOffset? lastCall = null;");
        sb.AppendLine();
        sb.AppendLine("\t\t/// <summary>");
        sb.AppendLine("\t\t/// For a given puzzle, get the user's puzzle input");
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

        sb.AppendLine("\t\t/// <returns></returns>");
        sb.Append("\t\tpublic async Task<string> ImportInput(");
        
        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.Append("int year, ");
            } else {
                sb.Append("string event, ");
            }
        }
        
        sb.Append("int puzzle");
        
        if (OptionData.SeparateInputs == true) {
            sb.Append(", int part");   
        }

        sb.AppendLine(")");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tThrottleCall();");
        sb.AppendLine();
        sb.AppendLine("\t\t\tHttpRequestMessage message = new(HttpMethod.Get, $\"TODO REPLACE\");");
        sb.AppendLine();
        sb.AppendLine("\t\t\tif (client == null)");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\ttry");
        sb.AppendLine("\t\t\t\t{");
        sb.AppendLine("\t\t\t\t\tInitializeClient();");
        sb.AppendLine("\t\t\t\t}");
        sb.AppendLine("\t\t\t\tcatch");
        sb.AppendLine("\t\t\t\t{");
        sb.AppendLine("\t\t\t\t\tthrow new Exception(\"Unable to read Cookie.txt. Make sure that it exists in the PuzzleHelper folder. See the ReadMe for more.\");");
        sb.AppendLine("\t\t\t\t}");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\tHttpResponseMessage result = await client!.SendAsync(message);");
        sb.AppendLine("\t\t\tstring response = await GetSuccessfulResponseContent(result);");
        sb.AppendLine();
        sb.AppendLine("\t\t\treturn response;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t/// <summary>");
        sb.AppendLine("\t\t/// For a given puzzle, get the user's example input");
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

        sb.AppendLine("\t\t/// <returns></returns>");
        sb.Append("\t\tpublic async Task<string> ImportInputExample(");
        
        if (OptionData.OneOff == false) {
            if (OptionData.ScheduledReleases == true) {
                sb.Append("int year, ");
            } else {
                sb.Append("string event, ");
            }
        }
        
        sb.Append("int puzzle");
        
        if (OptionData.SeparateInputs == true) {
            sb.Append(", int part");   
        }

        sb.AppendLine(")");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tThrottleCall();");
        sb.AppendLine();
        sb.AppendLine("\t\t\tHttpRequestMessage message = new(HttpMethod.Get, $\"TODO REPLACE\");");
        sb.AppendLine();
        sb.AppendLine("\t\t\tif (client == null)");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\ttry");
        sb.AppendLine("\t\t\t\t{");
        sb.AppendLine("\t\t\t\t\tInitializeClient();");
        sb.AppendLine("\t\t\t\t}");
        sb.AppendLine("\t\t\t\tcatch");
        sb.AppendLine("\t\t\t\t{");
        sb.AppendLine("\t\t\t\t\tthrow new Exception(\"Unable to read Cookie.txt. Make sure that it exists in the PuzzleHelper folder. See the ReadMe for more.\");");
        sb.AppendLine("\t\t\t\t}");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\tHttpResponseMessage result = await client!.SendAsync(message);");
        sb.AppendLine("\t\t\tstring response = await GetSuccessfulResponseContent(result);");
        sb.AppendLine();
        sb.AppendLine("\t\t\treturn response;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t/// <summary>");
        sb.AppendLine("\t\t/// Send the user's answer to the specific puzzle");
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

        sb.AppendLine("\t\t/// <returns></returns>");
        sb.Append("\t\tpublic async Task<string> SubmitAnswer(");
        
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

        sb.AppendLine("string answer)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tThrottleCall();");
        sb.AppendLine();
        sb.AppendLine("\t\t\t// TODO REPLACE");
        sb.AppendLine("\t\t\tDictionary<string, string> data = new()");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\t{ \"answer\", answer }");
        sb.AppendLine("\t\t\t};");
        sb.AppendLine();
        sb.AppendLine("\t\t\tHttpContent request = new FormUrlEncodedContent(data);");
        sb.AppendLine();
        sb.AppendLine("\t\t\tif (client == null)");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\ttry");
        sb.AppendLine("\t\t\t\t{");
        sb.AppendLine("\t\t\t\t\tInitializeClient();");
        sb.AppendLine("\t\t\t\t}");
        sb.AppendLine("\t\t\t\tcatch");
        sb.AppendLine("\t\t\t\t{");
        sb.AppendLine("\t\t\t\t\treturn \"Unable to read Cookie.txt. Make sure that it exists in the PuzzleHelper folder. See the ReadMe for more.\";");
        sb.AppendLine("\t\t\t\t}");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\tHttpResponseMessage result = await client!.PostAsync($\"TODO REPLACE\", request);");
        sb.AppendLine();
        sb.AppendLine("\t\t\tstring response = await GetSuccessfulResponseContent(result);");
        sb.AppendLine();
        sb.AppendLine("\t\t\ttry");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\t// Display the response");
        sb.AppendLine("\t\t\t\tHtmlDocument doc = new();");
        sb.AppendLine("\t\t\t\tdoc.LoadHtml(response);");
        sb.AppendLine("\t\t\t\tHtmlNode htmlElement = doc.DocumentNode.SelectSingleNode(\"TODO REPLACE\");");
        sb.AppendLine("\t\t\t\tresponse = htmlElement.InnerHtml;");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine("\t\t\tcatch (Exception)");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tConsole.WriteLine(\"Error parsing html response.\");");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\treturn response;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t/// <summary>");
        sb.AppendLine("\t\t/// Ensure that the response was successful and return the parsed response if it was");
        sb.AppendLine("\t\t/// </summary>");
        sb.AppendLine("\t\t/// <param name=\"result\"></param>");
        sb.AppendLine("\t\t/// <returns></returns>");
        sb.AppendLine("\t\tprivate static async Task<string> GetSuccessfulResponseContent(HttpResponseMessage result)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (result.StatusCode == HttpStatusCode.Unauthorized)");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tthrow new Exception(\"Your Cookie has expired, please update it. See the ReadMe for more info.\");");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\tresult.EnsureSuccessStatusCode();");
        sb.AppendLine("\t\t\treturn await result.Content.ReadAsStringAsync();");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t/// <summary>");
        sb.AppendLine("\t\t/// Tracks the last API call and prevents another call from being made until after the configured limit");
        sb.AppendLine("\t\t/// </summary>");
        sb.AppendLine("\t\tprivate void ThrottleCall()");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (lastCall != null && DateTimeOffset.Now < lastCall.Value.AddMinutes(throttleInMinutes))");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine($"\t\t\t\tthrow new Exception($\"Unable to make another API call to {OptionData.HumanReadableName} Server because we are attempting to throttle calls according to their specifications (See more in the ReadMe). Please try again after {{lastCall.Value.AddMinutes(throttleInMinutes)}}.\");");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\telse");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tlastCall = DateTimeOffset.Now;");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t/// <summary>");
        sb.AppendLine("\t\t/// Initialize the Http Client using the user's cookie");
        sb.AppendLine("\t\t/// </summary>");
        sb.AppendLine("\t\tprivate void InitializeClient()");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\t// We're waiting to do this until the last moment in case someone want's to use the code base without setting up the cookie");
        sb.AppendLine("\t\t\tclient = new HttpClient");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine($"\t\t\t\tBaseAddress = new Uri(\"{OptionData.BaseURL}/\")");
        sb.AppendLine("\t\t\t};");
        sb.AppendLine();
        sb.AppendLine($"\t\t\tclient.DefaultRequestHeaders.UserAgent.ParseAdd(\".NET 8.0 (+via https://github.com/austin-owensby/{OptionData.FormattedNameUpper} by austin_owensby@hotmail.com)\");");
        sb.AppendLine();
        sb.AppendLine("\t\t\tstring[] fileData;");
        sb.AppendLine();
        sb.AppendLine("\t\t\ttry");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tstring directoryPath = Directory.GetParent(Environment.CurrentDirectory)!.FullName;");
        sb.AppendLine("\t\t\t\tstring filePath = Path.Combine(directoryPath, \"Shared\", \"PuzzleHelper\", \"Cookie.txt\");");
        sb.AppendLine("\t\t\t\tfileData = File.ReadAllLines(filePath);");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine("\t\t\tcatch (Exception)");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tthrow new Exception(\"Unable to read Cookie.txt. Make sure that it exists in the PuzzleHelper folder. See the ReadMe for more.\");");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\tif (fileData.Length == 0 || string.IsNullOrWhiteSpace(fileData[0]))");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tthrow new Exception(\"Cookie.txt is empty. Please ensure it is properly populated and saved. See the ReadMe for more.\");");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\tif (fileData.Length > 1)");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tthrow new Exception(\"Detected multiple lines in Cookie.txt, ensure that the whole cookie is on 1 line.\");");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\tstring cookie = fileData[0];");
        sb.AppendLine("\t\t\tclient.DefaultRequestHeaders.Add(\"Cookie\", cookie);");

        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");
        sb.AppendLine("}");

        string path = $"{OptionData.FormattedNameUpper}/Shared/Gateways/{OptionData.FormattedNameUpper}Gateway.cs";
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
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