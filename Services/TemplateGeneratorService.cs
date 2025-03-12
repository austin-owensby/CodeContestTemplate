using System.IO.Compression;

namespace CodeContestTemplate.Services;

public class TemplateGeneratorService {
    public static void GenerateTemplate(Options options) {
        // Create temporary folder to work in
        Directory.CreateDirectory(options.FormattedName!);

        // Copy over top level folders
        Directory.CreateDirectory($"{options.FormattedName}/.vscode");
        File.Copy("TemplateFiles/.vscode/launch.json", $"{options.FormattedName}/.vscode/launch.json");
        File.Copy("TemplateFiles/.vscode/tasks.json", $"{options.FormattedName}/.vscode/tasks.json");
        File.Copy("TemplateFiles/.gitignore", $"{options.FormattedName}/.gitignore");
        File.Copy("TemplateFiles/LICENSE", $"{options.FormattedName}/LICENSE");
        File.Copy("TemplateFiles/REPLACE.sln", $"{options.FormattedName}/{options.FormattedName}.sln");

        Directory.CreateDirectory($"{options.FormattedName}/Console");
        File.Copy("TemplateFiles/Console/Console.csproj", $"{options.FormattedName}/Console/Console.csproj");
        Directory.CreateDirectory($"{options.FormattedName}/WebAPI");
        File.Copy("TemplateFiles/WebAPI/WebAPI.csproj", $"{options.FormattedName}/WebAPI/WebAPI.csproj");
        Directory.CreateDirectory($"{options.FormattedName}/Shared");
        File.Copy("TemplateFiles/Shared/Shared.csproj", $"{options.FormattedName}/Shared/Shared.csproj");

        // Zip up results
        ZipFile.CreateFromDirectory(options.FormattedName!, $"{options.FormattedName}.zip");

        // Clean up folder
        Directory.Delete(options.FormattedName!, true);
    }
}