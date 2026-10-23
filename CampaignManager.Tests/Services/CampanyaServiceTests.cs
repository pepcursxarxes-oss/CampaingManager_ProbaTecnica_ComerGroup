using System;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Services;
using CampaignManager.Tests.Fakes;
using Domain.Entities;
using Domain.Enums;

namespace CampaignManager.Tests.Services;

public class CampanyaServiceTests
{
    private static readonly DateTimeOffset Inici =
        new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset Fi =
        new(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset Ara =
        new(2026, 10, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeGenericRepository<Campanya> _repo = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly TestTimeProvider _timeProvider = new(Ara);
    private readonly CampanyaService _service;

    public CampanyaServiceTests()
    {
        _service = new CampanyaService(_repo, _unitOfWork, _timeProvider);
    }

    [Fact]
    public async Task CrearAsync_DadesValides_CreaIRetornaId()
    {
        var dto = new CrearCampanyaDTO("Nova Campanya", 500m, Inici, Fi);

        var id = await _service.CrearAsync(dto);

        Assert.NotEqual(Guid.Empty, id);
        var creat = await _service.ObtenirPerIdAsync(id);
        Assert.NotNull(creat);
        Assert.Equal("Nova Campanya", creat.Nom);
        Assert.Equal(500m, creat.PressupostTotal);
        Assert.Equal(0m, creat.PressupostConsumit);
        Assert.Equal(EstatCampanya.ESBORRANY, creat.Estat);
    }

    [Fact]
    public async Task CrearAsync_DtoNul_LlencaArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CrearAsync(null!));
    }

    [Fact]
    public async Task CrearAsync_DataFiAnteriorOIgual_LlencaArgumentException()
    {
        var dto = new CrearCampanyaDTO("Nova Campanya", 500m, Fi, Inici);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CrearAsync(dto));
    }

    [Fact]
    public async Task CrearAsync_NomMassaCurt_LlencaArgumentException()
    {
        var dto = new CrearCampanyaDTO("AB", 500m, Inici, Fi);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CrearAsync(dto));
    }

    [Fact]
    public async Task ObtenirPerIdAsync_Inexistent_RetornaNull()
    {
        var resultat = await _service.ObtenirPerIdAsync(Guid.NewGuid());
        Assert.Null(resultat);
    }

    [Fact]
    public async Task LlistarAsync_SenseFiltre_RetornaTotes()
    {
        await _service.CrearAsync(new CrearCampanyaDTO("Campanya 1", 100m, Inici, Fi));
        await _service.CrearAsync(new CrearCampanyaDTO("Campanya 2", 200m, Inici, Fi));

        var llista = await _service.LlistarAsync();

        Assert.Equal(2, llista.Count);
    }

    [Fact]
    public async Task LlistarAsync_AmbFiltreEstat_RetornaNomesCoincidents()
    {
        var id1 = await _service.CrearAsync(new CrearCampanyaDTO("Campanya 1", 100m, Inici, Fi));
        var id2 = await _service.CrearAsync(new CrearCampanyaDTO("Campanya 2", 200m, Inici, Fi));

        await _service.CanviarEstatAsync(id1, EstatCampanya.ACTIVA);

        var actives = await _service.LlistarAsync(EstatCampanya.ACTIVA);
        Assert.Single(actives);
        Assert.Equal(id1, actives[0].Id);

        var esborranys = await _service.LlistarAsync(EstatCampanya.ESBORRANY);
        Assert.Single(esborranys);
        Assert.Equal(id2, esborranys[0].Id);
    }

    [Fact]
    public async Task LlistarAsync_AmbFiltreInvalid_LlencaArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.LlistarAsync((EstatCampanya)99));
    }

    [Fact]
    public async Task CanviarEstatAsync_ActivarIPausar_ActualitzaEstat()
    {
        var id = await _service.CrearAsync(new CrearCampanyaDTO("Campanya 1", 100m, Inici, Fi));

        await _service.CanviarEstatAsync(id, EstatCampanya.ACTIVA);
        var act = await _service.ObtenirPerIdAsync(id);
        Assert.Equal(EstatCampanya.ACTIVA, act!.Estat);

        await _service.CanviarEstatAsync(id, EstatCampanya.PAUSADA);
        var pausada = await _service.ObtenirPerIdAsync(id);
        Assert.Equal(EstatCampanya.PAUSADA, pausada!.Estat);
    }

    [Fact]
    public async Task CanviarEstatAsync_EstatNoPermes_LlencaArgumentException()
    {
        var id = await _service.CrearAsync(new CrearCampanyaDTO("Campanya 1", 100m, Inici, Fi));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CanviarEstatAsync(id, EstatCampanya.FINALITZADA));

        Assert.Equal("Només es permet activar o pausar una campanya.", ex.Message);
    }

    [Fact]
    public async Task CanviarEstatAsync_CampanyaInexistent_LlencaKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CanviarEstatAsync(Guid.NewGuid(), EstatCampanya.ACTIVA));
    }

    [Fact]
    public async Task CanviarEstatAsync_ActivarCaducada_LlencaInvalidOperationException()
    {
        _timeProvider.SetUtcNow(Fi.AddDays(1));
        var id = await _service.CrearAsync(new CrearCampanyaDTO("Campanya 1", 100m, Inici, Fi));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CanviarEstatAsync(id, EstatCampanya.ACTIVA));
    }

    [Fact]
    public async Task ConsumirAsync_CampanyaActiva_ActualitzaPressupostConsumit()
    {
        var id = await _service.CrearAsync(new CrearCampanyaDTO("Campanya 1", 100m, Inici, Fi));
        await _service.CanviarEstatAsync(id, EstatCampanya.ACTIVA);

        await _service.ConsumirAsync(id, 40m);

        var campanya = await _service.ObtenirPerIdAsync(id);
        Assert.Equal(40m, campanya!.PressupostConsumit);
        Assert.Equal(EstatCampanya.ACTIVA, campanya.Estat);
    }

    [Fact]
    public async Task ConsumirAsync_TotalitatDelPressupost_PassaAFinalitzada()
    {
        var id = await _service.CrearAsync(new CrearCampanyaDTO("Campanya 1", 100m, Inici, Fi));
        await _service.CanviarEstatAsync(id, EstatCampanya.ACTIVA);

        await _service.ConsumirAsync(id, 100m);

        var campanya = await _service.ObtenirPerIdAsync(id);
        Assert.Equal(100m, campanya!.PressupostConsumit);
        Assert.Equal(EstatCampanya.FINALITZADA, campanya.Estat);
    }

    [Fact]
    public async Task ConsumirAsync_SuperaPressupost_LlencaInvalidOperationException()
    {
        var id = await _service.CrearAsync(new CrearCampanyaDTO("Campanya 1", 100m, Inici, Fi));
        await _service.CanviarEstatAsync(id, EstatCampanya.ACTIVA);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ConsumirAsync(id, 150m));

        var campanya = await _service.ObtenirPerIdAsync(id);
        Assert.Equal(0m, campanya!.PressupostConsumit);
    }

    [Fact]
    public async Task ConsumirAsync_CampanyaInexistent_LlencaKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.ConsumirAsync(Guid.NewGuid(), 50m));
    }
}
