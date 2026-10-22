using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public sealed record ConsumirPressupostDto(
    [property: Range(
        typeof(decimal), "0.01", "79228162514264337593543950335")]
    decimal Import);