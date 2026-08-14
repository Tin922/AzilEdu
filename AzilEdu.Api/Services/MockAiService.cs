using System.Text.Json;
using AzilEdu.Shared.DTOs;
 
namespace AzilEdu.Api.Services;
 
public class MockAiService : IAiService
{
    public string ProviderName => "Mock";
    public string ModelName => "Lokalni predvidljivi odgovori";
    public bool UsesExternalService => false;
 
    public Task<string> GenerateTextAsync(string purpose, string input)
    {
        var text = purpose switch
        {
            "animal-adoption" =>
                $"Upoznajte našeg štićenika! {input} Tražimo odgovoran i topao dom u kojem će dobiti sigurnost i pažnju.",
            "donor-thank-you" =>
                $"Hvala vam na podršci azilu. Vaša donacija izravno pomaže kvalitetnijoj brizi za životinje. {input}",
            "social-post" =>
                $"🐾 Traži se dom! {input} Podijelite objavu i pomozite nam pronaći pravu obitelj.",
            "daily-summary" =>
                BuildDailySummary(input),
            "volunteer-summary" =>
                BuildVolunteerSummary(input),
            _ =>
                $"AI prijedlog za svrhu '{purpose}': {input}"
        };
 
        return Task.FromResult(text);
    }
 
    public Task<T?> GenerateStructuredAsync<T>(string purpose, string input)
    {
        object? result = purpose switch
        {
            "animal-intake" => BuildAnimalIntake(input),
            "animal-data-check" => BuildAnimalDataCheck(input),
            _ => null
        };
 
        return Task.FromResult(result is T typedResult ? typedResult : default);
    }
 
    private static AnimalIntakeSuggestionDto BuildAnimalIntake(string input)
    {
        var lowerInput = input.ToLowerInvariant();
        var trimmed = input.Trim();
        var name = GuessName(trimmed);
        var breed = GuessBreed(lowerInput, trimmed);
        var age = GuessAge(lowerInput);
        var gender = GuessGender(lowerInput);

        var dto = new AnimalIntakeSuggestionDto
        {
            Name = name ?? "Nepoznato",
            Species = GuessSpecies(lowerInput, breed),
            Breed = breed,
            Gender = gender,
            Age = age,
            ArrivalDate = DateTime.Today,
            AnimalStatusId = 1,
            Description = trimmed,
            Confidence = CalculateConfidence(name, breed, age, gender),
            Warnings = BuildIntakeWarnings(name, breed, age, gender)
        };

        return dto;
    }

    private static string GuessSpecies(string lowerInput, string breed)
    {
        if (lowerInput.Contains("mačk") || lowerInput.Contains("mack"))
            return "Mačka";
        if (lowerInput.Contains("ptic") || lowerInput.Contains("papiga"))
            return "Ptica";

        if (lowerInput.Contains("pas") ||
            lowerInput.Contains("štene") || lowerInput.Contains("sten") ||
            lowerInput.Contains("doberman") || lowerInput.Contains("labrador") ||
            lowerInput.Contains("trča") || lowerInput.Contains("trca") ||
            !string.IsNullOrWhiteSpace(breed))
            return "Pas";

        return "Pas";
    }

    private static string GuessGender(string lowerInput)
    {
        if (ContainsGenderTerm(lowerInput,
                "ženka", "zenka", "žensko", "zensko", "ženski", "zenski", "samica"))
            return "Ženka";

        if (ContainsGenderTerm(lowerInput,
                "mužjak", "muzjak", "muški", "muski", "muško", "musko", "mužjaci", "samac"))
            return "Mužjak";

        return string.Empty;
    }

    private static bool ContainsGenderTerm(string lowerInput, params string[] terms)
    {
        foreach (var term in terms)
        {
            if (lowerInput.Contains(term, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static double CalculateConfidence(
        string? name,
        string breed,
        int? age,
        string gender)
    {
        var score = 0.55;

        if (!string.IsNullOrWhiteSpace(name))
            score += 0.12;
        if (!string.IsNullOrWhiteSpace(breed))
            score += 0.12;
        if (age.HasValue)
            score += 0.12;
        if (!string.IsNullOrWhiteSpace(gender))
            score += 0.09;

        return Math.Min(score, 0.92);
    }

    private static List<string> BuildIntakeWarnings(
        string? name,
        string breed,
        int? age,
        string gender)
    {
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
            warnings.Add("Ime nije prepoznato — dopuni ručno.");
        if (string.IsNullOrWhiteSpace(breed))
            warnings.Add("Pasmina nije prepoznata — dopuni ručno.");
        if (!age.HasValue)
            warnings.Add("Starost nije prepoznata — dopuni ručno.");
        if (string.IsNullOrWhiteSpace(gender))
            warnings.Add("Spol nije naveden u bilješci — provjeri ručno.");

        return warnings;
    }

    private static string? GuessName(string input)
    {
        var markers = new[] { "zove se ", "ime ", "nazvan ", "nazvana " };
        var lower = input.ToLowerInvariant();

        foreach (var marker in markers)
        {
            var index = lower.IndexOf(marker, StringComparison.Ordinal);
            if (index < 0)
                continue;

            var start = index + marker.Length;
            var rest = input[start..].Trim();
            var word = rest.Split([' ', ',', '.', ';'], StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(word))
                return char.ToUpper(word[0]) + word[1..].ToLowerInvariant();
        }

        return null;
    }

    private static string GuessBreed(string lowerInput, string originalInput)
    {
        const string pasminaMarker = "pasmina je ";
        var pasminaIndex = lowerInput.IndexOf(pasminaMarker, StringComparison.Ordinal);
        if (pasminaIndex >= 0)
        {
            var start = pasminaIndex + pasminaMarker.Length;
            var rest = originalInput[start..].Trim();
            var word = rest.Split([' ', ',', '.', ';'], StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(word))
                return FormatBreed(word);
        }

        var knownBreeds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["labrador"] = "Labrador",
            ["doberman"] = "Doberman",
            ["mješanac"] = "Mješanac",
            ["mijesanac"] = "Mješanac",
            ["maine coon"] = "Maine Coon mješanac",
            ["domaća kratkodlaka"] = "Domaća kratkodlaka",
            ["domaca kratkodlaka"] = "Domaća kratkodlaka",
            ["njemački ovčar"] = "Njemački ovčar",
            ["njemacki ovcar"] = "Njemački ovčar",
            ["golden retriever"] = "Golden retriever"
        };

        foreach (var (keyword, breed) in knownBreeds)
        {
            if (lowerInput.Contains(keyword, StringComparison.Ordinal))
                return breed;
        }

        return string.Empty;
    }

    private static string FormatBreed(string word)
    {
        if (word.Length == 0)
            return string.Empty;

        return char.ToUpper(word[0]) + word[1..].ToLowerInvariant();
    }

    private static int? GuessAge(string lowerInput)
    {
        foreach (var marker in new[] { "star je ", "star ", "dob ", "godina " })
        {
            var index = lowerInput.IndexOf(marker, StringComparison.Ordinal);
            if (index < 0)
                continue;

            var afterMarker = lowerInput[(index + marker.Length)..];
            var digits = new string(afterMarker.SkipWhile(c => !char.IsDigit(c))
                .TakeWhile(char.IsDigit)
                .ToArray());

            if (int.TryParse(digits, out var years) && years is > 0 and <= 30)
                return years;
        }

        for (var years = 1; years <= 15; years++)
        {
            if (lowerInput.Contains($"{years} god", StringComparison.Ordinal) ||
                lowerInput.Contains($"{years} godin", StringComparison.Ordinal) ||
                lowerInput.Contains($"oko {years}", StringComparison.Ordinal) ||
                lowerInput.Contains($"{years} g ", StringComparison.Ordinal))
                return years;
        }

        if (lowerInput.Contains("mlad", StringComparison.Ordinal) ||
            lowerInput.Contains("štene", StringComparison.Ordinal) ||
            lowerInput.Contains("sten", StringComparison.Ordinal) ||
            lowerInput.Contains("macence", StringComparison.Ordinal))
            return 1;

        return null;
    }

    private static string BuildVolunteerSummary(string input)
    {
        try
        {
            using var document = JsonDocument.Parse(input);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
                return "Nema otvorenih zadataka za sažetak.";

            var today = DateTime.Today;
            var tasks = root.EnumerateArray()
                .Select(ReadVolunteerSummaryTask)
                .OrderBy(task => task.DueDate ?? DateTime.MaxValue)
                .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var overdue = tasks.Count(task =>
                task.DueDate.HasValue && task.DueDate.Value.Date < today);
            var lines = new List<string>
            {
                $"Imaš {tasks.Count} otvorenih zadataka" +
                (overdue > 0 ? $" ({overdue} s prošlim rokom)." : ".")
            };

            for (var i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                var urgency = DescribeDueDate(task.DueDate, today);
                var animal = string.IsNullOrWhiteSpace(task.Animal) ? "bez životinje" : task.Animal;
                var type = string.IsNullOrWhiteSpace(task.Type) ? "Općenito" : task.Type;

                lines.Add(
                    $"{i + 1}. {task.Title} — {type}, {animal}, " +
                    $"rok: {FormatDueDate(task.DueDate)}{urgency}, status: {task.Status}");
            }

            if (overdue > 0)
                lines.Add("Prioritet: prvo riješi zakašnjele zadatke.");

            return string.Join(Environment.NewLine, lines);
        }
        catch
        {
            return "Sažetak zadataka trenutačno nije moguće formatirati.";
        }
    }

    private static string BuildDailySummary(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "Dnevni operativni sažetak nije dostupan.";

        return
            "Dnevni operativni sažetak azila:" + Environment.NewLine +
            input.Replace("; ", Environment.NewLine + "• ", StringComparison.Ordinal)
                .Insert(0, "• ");
    }

    private static (string Title, string Type, string Animal, string Status, DateTime? DueDate)
        ReadVolunteerSummaryTask(JsonElement element)
    {
        return (
            ReadJsonString(element, "Title"),
            ReadJsonString(element, "Type"),
            ReadJsonString(element, "Animal"),
            ReadJsonString(element, "Status"),
            ReadJsonDate(element, "DueDate"));
    }

    private static string ReadJsonString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var value))
            return value.GetString() ?? string.Empty;

        var camelCase = char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
        if (element.TryGetProperty(camelCase, out value))
            return value.GetString() ?? string.Empty;

        return string.Empty;
    }

    private static DateTime? ReadJsonDate(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            var camelCase = char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
            if (!element.TryGetProperty(camelCase, out value))
                return null;
        }

        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;

        return value.TryGetDateTime(out var date) ? date : null;
    }

    private static string FormatDueDate(DateTime? dueDate) =>
        dueDate?.ToString("dd.MM.yyyy.") ?? "nije zadan";

    private static string DescribeDueDate(DateTime? dueDate, DateTime today)
    {
        if (!dueDate.HasValue)
            return string.Empty;

        if (dueDate.Value.Date < today)
            return " — ZAKAŠNJEO";
        if (dueDate.Value.Date == today)
            return " — DANAS";

        return string.Empty;
    }

    private static AnimalDataCheckDto BuildAnimalDataCheck(string input)
    {
        var warnings = new List<string>();
        using var document = JsonDocument.Parse(input);
        var root = document.RootElement;
 
        if (!root.TryGetProperty("Name", out var name) ||
            string.IsNullOrWhiteSpace(name.GetString()))
            warnings.Add("Ime nije uneseno.");
 
        if (!root.TryGetProperty("Species", out var species) ||
            string.IsNullOrWhiteSpace(species.GetString()))
            warnings.Add("Vrsta nije unesena.");
 
        if (root.TryGetProperty("Age", out var age) &&
            age.ValueKind == JsonValueKind.Number &&
            age.GetInt32() < 0)
            warnings.Add("Starost ne može biti negativna.");
 
        return new AnimalDataCheckDto
        {
            IsReady = warnings.Count == 0,
            Warnings = warnings,
            SuggestedDescription = warnings.Count == 0
                ? "Podaci su spremni za završni ljudski pregled."
                : "Prije spremanja ispravi navedena upozorenja."
        };
    }
}