using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using CampaignApi.Controllers;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CampaignManager.Tests.Controllers;

public class CampaignsControllerTests
{
    private class FakeCampanyaService : ICampanyaService
    {
        public bool ThrowArgumentExceptionOnCrear { get; set; }
        public bool ThrowKeyNotFound { get; set; }
        public bool ThrowArgumentException { get; set; }
        public bool ThrowInvalidOperationException { get; set; }
        public bool ThrowArgumentExceptionOnLlistar { get; set; }

        public Task<Guid> CrearAsync(CrearCampanyaDTO dto, CancellationToken cancellationToken = default)
        {
            if (ThrowArgumentExceptionOnCrear)
                throw new ArgumentException("Nom massa curt.");

            return Task.FromResult(Guid.NewGuid());
        }

        public Task<CampanyaDTO?> ObtenirPerIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (ThrowKeyNotFound)
                return Task.FromResult<CampanyaDTO?>(null);

            var dto = new CampanyaDTO(id, "Test", 100m, 0m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(10), EstatCampanya.ESBORRANY);
            return Task.FromResult<CampanyaDTO?>(dto);
        }

        public Task<IReadOnlyList<CampanyaDTO>> LlistarAsync(EstatCampanya? estat = null, CancellationToken cancellationToken = default)
        {
            if (ThrowArgumentExceptionOnLlistar)
                throw new ArgumentException("Estat invalid.");

            IReadOnlyList<CampanyaDTO> llista = new List<CampanyaDTO>
            {
                new(Guid.NewGuid(), "Campanya 1", 100m, 0m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(10), EstatCampanya.ESBORRANY)
            };
            return Task.FromResult(llista);
        }

        public Task CanviarEstatAsync(Guid id, EstatCampanya estat, CancellationToken cancellationToken = default)
        {
            if (ThrowKeyNotFound)
                throw new KeyNotFoundException("No trobada.");
            if (ThrowArgumentException)
                throw new ArgumentException("Estat invàlid.");
            if (ThrowInvalidOperationException)
                throw new InvalidOperationException("No es pot modificar.");

            return Task.CompletedTask;
        }

        public Task ConsumirAsync(Guid id, decimal import, CancellationToken cancellationToken = default)
        {
            if (ThrowKeyNotFound)
                throw new KeyNotFoundException("No trobada.");
            if (ThrowArgumentException)
                throw new ArgumentException("Import no vàlid.");
            if (ThrowInvalidOperationException)
                throw new InvalidOperationException("Consum supera disponible.");

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Crear_DadesValides_Retorna201Created()
    {
        var fakeService = new FakeCampanyaService();
        var controller = new CampaignsController(fakeService);
        var dto = new CrearCampanyaDTO("Campanya Nova", 1000m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));

        var result = await controller.Crear(dto, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
    }

    [Fact]
    public async Task Crear_DadesInvalides_Retorna400BadRequest()
    {
        var fakeService = new FakeCampanyaService { ThrowArgumentExceptionOnCrear = true };
        var controller = new CampaignsController(fakeService);
        var dto = new CrearCampanyaDTO("AB", 1000m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));

        var result = await controller.Crear(dto, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task ObtenirPerId_Existent_Retorna200Ok()
    {
        var fakeService = new FakeCampanyaService();
        var controller = new CampaignsController(fakeService);
        var id = Guid.NewGuid();

        var result = await controller.ObtenirPerId(id, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task ObtenirPerId_Inexistent_Retorna404NotFound()
    {
        var fakeService = new FakeCampanyaService { ThrowKeyNotFound = true };
        var controller = new CampaignsController(fakeService);

        var result = await controller.ObtenirPerId(Guid.NewGuid(), CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task Llistar_SenseFiltre_Retorna200Ok()
    {
        var fakeService = new FakeCampanyaService();
        var controller = new CampaignsController(fakeService);

        var result = await controller.Llistar(null, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task Llistar_FiltreInvalid_Retorna400BadRequest()
    {
        var fakeService = new FakeCampanyaService { ThrowArgumentExceptionOnLlistar = true };
        var controller = new CampaignsController(fakeService);

        var result = await controller.Llistar((EstatCampanya)99, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task CanviarEstat_Valid_Retorna204NoContent()
    {
        var fakeService = new FakeCampanyaService();
        var controller = new CampaignsController(fakeService);
        var dto = new CanviarEstatCampanyaDTO(EstatCampanya.ACTIVA);

        var result = await controller.CanviarEstat(Guid.NewGuid(), dto, CancellationToken.None);

        var noContent = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContent.StatusCode);
    }

    [Fact]
    public async Task CanviarEstat_NoTrobada_Retorna404NotFound()
    {
        var fakeService = new FakeCampanyaService { ThrowKeyNotFound = true };
        var controller = new CampaignsController(fakeService);
        var dto = new CanviarEstatCampanyaDTO(EstatCampanya.ACTIVA);

        var result = await controller.CanviarEstat(Guid.NewGuid(), dto, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task CanviarEstat_TransicioInvalida_Retorna400BadRequest()
    {
        var fakeService = new FakeCampanyaService { ThrowArgumentException = true };
        var controller = new CampaignsController(fakeService);
        var dto = new CanviarEstatCampanyaDTO(EstatCampanya.FINALITZADA);

        var result = await controller.CanviarEstat(Guid.NewGuid(), dto, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task CanviarEstat_ConflicteEstat_Retorna409Conflict()
    {
        var fakeService = new FakeCampanyaService { ThrowInvalidOperationException = true };
        var controller = new CampaignsController(fakeService);
        var dto = new CanviarEstatCampanyaDTO(EstatCampanya.ACTIVA);

        var result = await controller.CanviarEstat(Guid.NewGuid(), dto, CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(409, conflict.StatusCode);
    }

    [Fact]
    public async Task Consumir_Valid_Retorna204NoContent()
    {
        var fakeService = new FakeCampanyaService();
        var controller = new CampaignsController(fakeService);
        var dto = new ConsumirPressupostDto(50m);

        var result = await controller.Consumir(Guid.NewGuid(), dto, CancellationToken.None);

        var noContent = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContent.StatusCode);
    }

    [Fact]
    public async Task Consumir_NoTrobada_Retorna404NotFound()
    {
        var fakeService = new FakeCampanyaService { ThrowKeyNotFound = true };
        var controller = new CampaignsController(fakeService);
        var dto = new ConsumirPressupostDto(50m);

        var result = await controller.Consumir(Guid.NewGuid(), dto, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task Consumir_ImportNegatiu_Retorna400BadRequest()
    {
        var fakeService = new FakeCampanyaService { ThrowArgumentException = true };
        var controller = new CampaignsController(fakeService);
        var dto = new ConsumirPressupostDto(-10m);

        var result = await controller.Consumir(Guid.NewGuid(), dto, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task Consumir_SuperaPressupost_Retorna409Conflict()
    {
        var fakeService = new FakeCampanyaService { ThrowInvalidOperationException = true };
        var controller = new CampaignsController(fakeService);
        var dto = new ConsumirPressupostDto(500m);

        var result = await controller.Consumir(Guid.NewGuid(), dto, CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(409, conflict.StatusCode);
    }
}
