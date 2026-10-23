using System;

namespace Application.DTOs
{
    public sealed record CrearCampanyaDTO(
        string Nom,
        decimal PressupostTotal,
        DateTimeOffset DataInici,
        DateTimeOffset DataFi,
        decimal PressupostConsumit = 0);
}
