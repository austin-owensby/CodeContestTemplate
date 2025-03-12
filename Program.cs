using CodeContestTemplate.Services;

Options options = InputService.GetOptionsFromInputs();
TemplateGeneratorService.GenerateTemplate(options);

Console.WriteLine("Project creation complete! Please review the code and update any TODOs in the code.");