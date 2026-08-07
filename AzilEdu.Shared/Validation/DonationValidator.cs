using AzilEdu.Shared.DTOs;

namespace AzilEdu.Shared.Validation;

public static class DonationValidator
{
    public const int MoneyDonationTypeId = 1;

    public static string? Validate(SaveDonationDto request, DateTime? today = null)
    {
        var referenceDate = (today ?? DateTime.Today).Date;

        if (request.DonorId <= 0)
            return "Donator je obavezan.";

        if (request.DonationTypeId <= 0)
            return "Tip donacije je obavezan.";

        if (request.DonationStatusId <= 0)
            return "Status donacije je obavezan.";

        if (request.DonationDate.Date > referenceDate)
            return "Datum donacije ne smije biti u budućnosti.";

        if (request.Quantity.HasValue && request.Quantity.Value < 0)
            return "Količina ne smije biti negativna.";

        if (request.EstimatedValue.HasValue && request.EstimatedValue.Value < 0)
            return "Procijenjena vrijednost ne smije biti negativna.";

        if (request.Amount.HasValue && request.Amount.Value < 0)
            return "Iznos ne smije biti negativan.";

        var isMoneyDonation = request.DonationTypeId == MoneyDonationTypeId;

        if (isMoneyDonation)
        {
            if (!request.Amount.HasValue || request.Amount.Value <= 0)
                return "Za novčanu donaciju potrebno je upisati iznos veći od nule.";
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.ItemName))
                return "Za nenovčanu donaciju potrebno je upisati naziv donacije.";

            if (!request.Quantity.HasValue || request.Quantity.Value <= 0)
                return "Za nenovčanu donaciju potrebno je upisati količinu veću od nule.";
        }

        return null;
    }
}
