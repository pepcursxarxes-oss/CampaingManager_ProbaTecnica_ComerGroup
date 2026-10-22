using Domain.Enums;

namespace Application.DTOs;

public sealed record CampanyaDTO(Guid Id, string Nom, decimal PressupostTotal, decimal PressupostConsumit, DateTimeOffset DataInici, DateTimeOffset DataFi, EstatCampanya Estat);
