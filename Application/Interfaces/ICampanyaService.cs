using Application.DTOs;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface ICampanyaService
    {
        Task<Guid> CrearAsync(
            CrearCampanyaDTO dto,
            CancellationToken cancellationToken = default);

        Task<CampanyaDTO?> ObtenirPerIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CampanyaDTO>> LlistarAsync(
            EstatCampanya? estat = null,
            CancellationToken cancellationToken = default);

        Task CanviarEstatAsync(
            Guid id,
            EstatCampanya estat,
            CancellationToken cancellationToken = default);

        Task ConsumirAsync(
            Guid id,
            decimal import,
            CancellationToken cancellationToken = default);
        
    }
}
