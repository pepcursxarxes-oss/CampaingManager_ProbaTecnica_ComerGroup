using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public sealed record ConsumirPressupostDto(
    [param: Required]
    [param: ValidarImportPositiu]
    decimal Import);

public sealed class ValidarImportPositiuAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is decimal import && import > 0m;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"El camp {name} ha de ser superior a zero.";
    }
}