using AzilEdu.Shared.DTOs;

namespace AzilEdu.Api.Services;

public static class AiStructuredResponseValidator
{
    public static bool TryValidateAnimalIntake(
        AnimalIntakeSuggestionDto? dto,
        out string error)
    {
        if (dto is null)
        {
            error = "AI servis nije vratio valjan strukturirani odgovor.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Species))
        {
            error = "AI prijedlog nema obavezna polja (ime i vrsta).";
            return false;
        }

        if (dto.Confidence is < 0 or > 1)
        {
            error = "AI prijedlog ima nevaljanu vrijednost pouzdanosti.";
            return false;
        }

        if (dto.AnimalStatusId < 0)
        {
            error = "AI prijedlog ima nevaljan status.";
            return false;
        }

        dto.Warnings ??= new List<string>();
        error = string.Empty;
        return true;
    }

    public static bool TryValidateAnimalDataCheck(
        AnimalDataCheckDto? dto,
        out string error)
    {
        if (dto is null)
        {
            error = "AI servis nije vratio valjan strukturirani odgovor.";
            return false;
        }

        dto.Warnings ??= new List<string>();
        dto.SuggestedDescription ??= string.Empty;
        error = string.Empty;
        return true;
    }
}
