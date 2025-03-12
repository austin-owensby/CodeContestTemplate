namespace CodeContestTemplate.Services;

public class Options {
    public string? HumanReadableName { get; set; }
    public string? BaseURL { get; set; }
    public bool? OneOff { get; set; }
    public bool? ScheduledReleases { get; set; }
    public int? TotalPuzzles { get; set; }
    public int? PartsPerPuzzle { get; set; }
    public bool? SeparateInputs { get; set; }
    public bool? PrivateInputs { get; set; }
    public int? UtcOffset { get; set; }
    public int? Month { get; set; }
    public bool? SpecificDate { get; set; }
    public bool? OnWeekends { get; set; }
    public int? Date { get; set; }
    public int? Year { get; set; }
    public string? FormattedName => HumanReadableName?.Replace(" ", string.Empty);
}

/*
DEBUG
public class Options {
    public string? HumanReadableName { get; set; } = "Test Project";
    public string? BaseURL { get; set; } = "google.com";
    public bool? OneOff { get; set; } = false;
    public bool? ScheduledReleases { get; set; } = true;
    public int? TotalPuzzles { get; set; } = 25;
    public int? PartsPerPuzzle { get; set; } = 2;
    public bool? SeparateInputs { get; set; } = false;
    public bool? PrivateInputs { get; set; } = true;
    public int? UtcOffset { get; set; } = 7;
    public int? Month { get; set; } = 12;
    public bool? SpecificDate { get; set; } = true;
    public bool? OnWeekends { get; set; } = true;
    public int? Date { get; set; } = 1;
    public int? Year { get; set; } = 2015;
    public string? FormattedName => HumanReadableName?.Replace(" ", string.Empty);
}
*/

public class InputService {
    public static Options GetOptionsFromInputs() {
        Options options = new();

        options.HumanReadableName = GetStringInput("What is the human readable name of the project?", options.HumanReadableName);

        options.BaseURL = GetStringInput("What is the base url of the project?", options.BaseURL);

        options.OneOff = GetBoolOption("Is this a one off event?", options.OneOff);

        if (options.OneOff == false) {
            options.ScheduledReleases = GetBoolOption("Will this event release puzzles periodically starting at a set date and time instead of all at once?", options.ScheduledReleases);

            options.UtcOffset = GetHoursOption("Compared to midnight UTC, what is the positive offset in hours from when events release?", options.UtcOffset);

            if (options.ScheduledReleases == true) {
                options.Month = GetMonthOption("What month does the event start in?", options.Month);

                options.SpecificDate = GetBoolOption("Does the event start on a specific date?", options.SpecificDate);

                if (options.SpecificDate == true) {
                    options.Date = GetDateOption("What day of the month does the event start on?", options.Month!.Value, options.Date);
                    options.Year = GetIntOption("What year did the first event come out?", options.Year);
                }

                // TODO ask more questions to handle if specific date is false

                options.OnWeekends = GetBoolOption("Do puzzles come out on weekends?", options.OnWeekends);
            }
        }
        
        options.TotalPuzzles = GetIntOption($"How many total puzzles are there {(options.OneOff == true ? "per event" : string.Empty)}?", options.TotalPuzzles);

        options.PartsPerPuzzle = GetIntOption("How many parts will each puzzle have?", options.PartsPerPuzzle);

        if (options.PartsPerPuzzle > 1) {
            options.SeparateInputs = GetBoolOption("Will each part have a separate input?", options.SeparateInputs);
        }
        else {
            options.SeparateInputs = false;
        }

        options.PrivateInputs = GetBoolOption("Should the inputs be kept private and not checked into source code?", options.PrivateInputs);
        return options;
    }

    private static bool GetBoolOption(string prompt, bool? value) {
        while (value == null) {
            Console.WriteLine(prompt);
            Console.WriteLine("1) Yes");
            Console.WriteLine("2) No");
            Console.WriteLine();

            string? valueString = Console.ReadLine();
            Console.Clear();

            if (int.TryParse(valueString, out int parsedValue) && parsedValue > 0 && parsedValue <= 2) {
                value = parsedValue == 1;
            }
            else {
                Console.WriteLine($"Invalid value, expected an integer 1 or 2.");
            }
        }

        return value.Value;
    }

    private static int GetIntOption(string prompt, int? value) {
        while (value == null) {
            Console.WriteLine(prompt);
            Console.WriteLine();

            string? valueString = Console.ReadLine();
            Console.Clear();

            if (int.TryParse(valueString, out int parsedValue) && parsedValue > 0) {
                value = parsedValue;
            }
            else {
                Console.WriteLine($"Invalid value, expected a positive integer.");
            }
        }

        return value.Value;
    }

    private static int GetHoursOption(string prompt, int? value) {
        while (value == null) {
            Console.WriteLine(prompt);
            Console.WriteLine();

            string? valueString = Console.ReadLine();
            Console.Clear();

            if (int.TryParse(valueString, out int parsedValue) && parsedValue >= 0 && parsedValue <= 23) {
                value = parsedValue;
            }
            else {
                Console.WriteLine("Invalid value, expected a positive integer between 0 and 23 inclusive.");
            }
        }

        return value.Value;
    }

    private static int GetMonthOption(string prompt, int? value) {
        while (value == null) {
            Console.WriteLine(prompt);
            Console.WriteLine();

            string? valueString = Console.ReadLine();
            Console.Clear();

            if (int.TryParse(valueString, out int parsedValue) && parsedValue >= 1 && parsedValue <= 12) {
                value = parsedValue;
            }
            else {
                Console.WriteLine("Invalid value, expected a positive integer between 1 and 12 inclusive.");
            }
        }

        return value.Value;
    }

    private static int GetDateOption(string prompt, int month, int? value) {
        while (value == null) {
            Console.WriteLine(prompt);
            Console.WriteLine();

            string? valueString = Console.ReadLine();
            Console.Clear();

            if (int.TryParse(valueString, out int parsedValue) && DateTime.TryParse($"{DateTime.Now.Year}-{month}-{parsedValue}", out DateTime date)) {
                value = parsedValue;
            }
            else {
                Console.WriteLine($"Invalid value, expected a positive integer that is a day in month {month}.");
            }
        }

        return value.Value;
    }

    private static string GetStringInput(string prompt, string? value) {
        while (value == null) {
            Console.WriteLine(prompt);
            Console.WriteLine();

            string? valueString = Console.ReadLine();
            Console.Clear();

            if (!string.IsNullOrEmpty(valueString)) {
                value = valueString;
            }
            else {
                Console.WriteLine($"Invalid value, expected a non-empty string.");
            }
        }

        return value;
    }
}
