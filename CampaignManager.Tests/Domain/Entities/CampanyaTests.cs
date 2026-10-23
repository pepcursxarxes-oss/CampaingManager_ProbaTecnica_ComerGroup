using System;
using Domain.Entities;
using Domain.Enums;

namespace CampaignManager.Tests.Domain.Entities;

public class CampanyaTests
{
    private static readonly DateTimeOffset Inici =
        new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset Fi =
        new(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset Ara =
        new(2026, 10, 15, 12, 0, 0, TimeSpan.Zero);

    private static Campanya CrearCampanya(decimal pressupost = 100m) =>
        new("Campanya de prova", pressupost, Inici, Fi);

    // ==========================================
    // CREACIÓ
    // ==========================================

    [Fact]
    public void Constructor_DadesValides_CreaCampanyaEnEsborrany()
    {
        var campanya = CrearCampanya();

        Assert.NotEqual(Guid.Empty, campanya.Id);
        Assert.Equal("Campanya de prova", campanya.Nom);
        Assert.Equal(100m, campanya.PressupostTotal);
        Assert.Equal(0m, campanya.PressupostConsumit);
        Assert.Equal(EstatCampanya.ESBORRANY, campanya.Estat);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("AB")]
    public void Constructor_NomInvalid_LlencaExcepcio(string nom)
    {
        Assert.Throws<ArgumentException>(() =>
            new Campanya(nom, 100m, Inici, Fi));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Constructor_PressupostNoPositiu_LlencaExcepcio(decimal pressupost)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Campanya("Campanya valida", pressupost, Inici, Fi));
    }

    [Fact]
    public void Constructor_DataFiIgualADataInici_LlencaExcepcio()
    {
        Assert.Throws<ArgumentException>(() =>
            new Campanya("Campanya valida", 100m, Inici, Inici));
    }

    [Fact]
    public void Constructor_DataFiAnteriorADataInici_LlencaExcepcio()
    {
        Assert.Throws<ArgumentException>(() =>
            new Campanya("Campanya valida", 100m, Fi, Inici));
    }

    // ==========================================
    // CANVI D'ESTAT
    // ==========================================

    [Fact]
    public void Activar_CampanyaValida_PassaAActiva()
    {
        var campanya = CrearCampanya();

        campanya.Activar(Ara);

        Assert.Equal(EstatCampanya.ACTIVA, campanya.Estat);
    }

    [Fact]
    public void Activar_CampanyaCaducada_LlencaExcepcio()
    {
        var campanya = CrearCampanya();

        Assert.Throws<InvalidOperationException>(() =>
            campanya.Activar(Fi));
    }

    [Fact]
    public void Activar_CampanyaPassadaDataFi_LlencaExcepcio()
    {
        var campanya = CrearCampanya();

        Assert.Throws<InvalidOperationException>(() =>
            campanya.Activar(Fi.AddDays(1)));
    }

    [Fact]
    public void Activar_CampanyaFinalitzada_LlencaExcepcio()
    {
        var campanya = CrearCampanya();
        campanya.Activar(Ara);
        campanya.Consumir(100m, Ara);

        Assert.Equal(EstatCampanya.FINALITZADA, campanya.Estat);
        Assert.Throws<InvalidOperationException>(() =>
            campanya.Activar(Ara));
    }

    [Fact]
    public void Pausar_CampanyaActiva_PassaAPausada()
    {
        var campanya = CrearCampanya();
        campanya.Activar(Ara);

        campanya.Pausar();

        Assert.Equal(EstatCampanya.PAUSADA, campanya.Estat);
    }

    [Fact]
    public void Pausar_CampanyaEsborrany_LlencaExcepcio()
    {
        var campanya = CrearCampanya();

        Assert.Throws<InvalidOperationException>(() =>
            campanya.Pausar());
    }

    [Fact]
    public void Pausar_CampanyaFinalitzada_LlencaExcepcio()
    {
        var campanya = CrearCampanya();
        campanya.Activar(Ara);
        campanya.Consumir(100m, Ara);

        Assert.Throws<InvalidOperationException>(() =>
            campanya.Pausar());
    }

    // ==========================================
    // CONSUM DE PRESSUPOST
    // ==========================================

    [Fact]
    public void Consumir_CampanyaActiva_ActualitzaPressupost()
    {
        var campanya = CrearCampanya();
        campanya.Activar(Ara);

        campanya.Consumir(30m, Ara);

        Assert.Equal(30m, campanya.PressupostConsumit);
        Assert.Equal(EstatCampanya.ACTIVA, campanya.Estat);
    }

    [Fact]
    public void Consumir_ImportsSuccessius_AcumulaCorrectament()
    {
        var campanya = CrearCampanya(200m);
        campanya.Activar(Ara);

        campanya.Consumir(40m, Ara);
        campanya.Consumir(60m, Ara);

        Assert.Equal(100m, campanya.PressupostConsumit);
        Assert.Equal(EstatCampanya.ACTIVA, campanya.Estat);
    }

    [Fact]
    public void Consumir_PressupostExacte_FinalitzaCampanya()
    {
        var campanya = CrearCampanya();
        campanya.Activar(Ara);

        campanya.Consumir(100m, Ara);

        Assert.Equal(100m, campanya.PressupostConsumit);
        Assert.Equal(EstatCampanya.FINALITZADA, campanya.Estat);
    }

    [Fact]
    public void Consumir_ImportSuperiorAlSaldo_LlencaExcepcio()
    {
        var campanya = CrearCampanya();
        campanya.Activar(Ara);

        Assert.Throws<InvalidOperationException>(() =>
            campanya.Consumir(101m, Ara));

        Assert.Equal(0m, campanya.PressupostConsumit);
        Assert.Equal(EstatCampanya.ACTIVA, campanya.Estat);
    }

    [Fact]
    public void Consumir_CampanyaNoActiva_LlencaExcepcio()
    {
        var campanya = CrearCampanya();

        Assert.Throws<InvalidOperationException>(() =>
            campanya.Consumir(10m, Ara));

        Assert.Equal(0m, campanya.PressupostConsumit);
    }

    [Fact]
    public void Consumir_CampanyaPausada_LlencaExcepcio()
    {
        var campanya = CrearCampanya();
        campanya.Activar(Ara);
        campanya.Pausar();

        Assert.Throws<InvalidOperationException>(() =>
            campanya.Consumir(10m, Ara));

        Assert.Equal(0m, campanya.PressupostConsumit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Consumir_ImportNoPositiu_LlencaExcepcio(decimal import)
    {
        var campanya = CrearCampanya();
        campanya.Activar(Ara);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            campanya.Consumir(import, Ara));

        Assert.Equal(0m, campanya.PressupostConsumit);
    }

    [Fact]
    public void Consumir_AbansDeDataInici_LlencaExcepcio()
    {
        var campanya = CrearCampanya();
        campanya.Activar(Ara);

        Assert.Throws<InvalidOperationException>(() =>
            campanya.Consumir(10m, Inici.AddTicks(-1)));

        Assert.Equal(0m, campanya.PressupostConsumit);
    }

    [Fact]
    public void Consumir_DespresDeDataFi_LlencaExcepcio()
    {
        var campanya = CrearCampanya();
        campanya.Activar(Ara);

        Assert.Throws<InvalidOperationException>(() =>
            campanya.Consumir(10m, Fi.AddTicks(1)));

        Assert.Equal(0m, campanya.PressupostConsumit);
    }

    [Fact]
    public void Consumir_LímitsVigènciaIniciIFi_SónValids()
    {
        var campanya = CrearCampanya(100m);
        campanya.Activar(Ara);

        // A la mateixa DataInici ha de funcionar
        campanya.Consumir(10m, Inici);
        Assert.Equal(10m, campanya.PressupostConsumit);

        // A la mateixa DataFi ha de funcionar
        campanya.Consumir(10m, Fi);
        Assert.Equal(20m, campanya.PressupostConsumit);
    }
}
