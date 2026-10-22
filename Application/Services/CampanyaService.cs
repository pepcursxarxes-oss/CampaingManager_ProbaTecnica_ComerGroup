using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public sealed class CampanyaService : ICampanyaService
{
    private readonly IGenericRepository<Campanya> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CampanyaService(
        IGenericRepository<Campanya> repository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> CrearAsync(
        CrearCampanyaDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.DataFi <= dto.DataInici)
        {
            throw new ArgumentException(
                "La data de finalització ha de ser posterior a la data d'inici.");
        }

        var campanya = new Campanya(
            dto.Nom,
            dto.PressupostTotal,
            dto.DataInici,
            dto.DataFi);

        await _repository.AddAsync(campanya, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return campanya.Id;
    }

    public async Task<CampanyaDTO?> ObtenirPerIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var campanya = await _repository.GetByIdAsync(
            id, cancellationToken);

        return campanya is null ? null : MapToDto(campanya);
    }

    public async Task<IReadOnlyList<CampanyaDTO>> LlistarAsync(
        EstatCampanya? estat = null,
        CancellationToken cancellationToken = default)
    {
        var campanyes = await _repository.GetAllAsync(
            cancellationToken);

        var resultat = estat.HasValue
            ? campanyes.Where(c => c.Estat == estat.Value)
            : campanyes;

        return resultat
            .Select(MapToDto)
            .ToList();
    }

    public async Task CanviarEstatAsync(
        Guid id,
        EstatCampanya estat,
        CancellationToken cancellationToken = default)
    {
        var campanya = await ObtenirEntitatAsync(id, cancellationToken);
        var ara = _timeProvider.GetUtcNow();

        switch (estat)
        {
            case EstatCampanya.ACTIVA:
                campanya.Activar(ara);
                break;

            case EstatCampanya.PAUSADA:
                campanya.Pausar();
                break;

            default:
                throw new ArgumentException(
                    "Només es permet activar o pausar una campanya.");
        }

        _repository.Update(campanya);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ConsumirAsync(
        Guid id,
        decimal import,
        CancellationToken cancellationToken = default)
    {
        var campanya = await ObtenirEntitatAsync(id, cancellationToken);
        var ara = _timeProvider.GetUtcNow();

        campanya.Consumir(import, ara);

        _repository.Update(campanya);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Campanya> ObtenirEntitatAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var campanya = await _repository.GetByIdAsync(
            id, cancellationToken);

        return campanya ?? throw new KeyNotFoundException(
            $"No s'ha trobat la campanya amb identificador {id}.");
    }

    private static CampanyaDTO MapToDto(Campanya campanya)
    {
        return new CampanyaDTO(
            campanya.Id,
            campanya.Nom,
            campanya.PressupostTotal,
            campanya.PressupostConsumit,
            campanya.DataInici,
            campanya.DataFi,
            campanya.Estat);
    }
}