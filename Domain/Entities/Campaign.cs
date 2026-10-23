using System;
using System.Collections.Generic;
using System.Text;
using Domain.Enums;

namespace Domain.Entities
{
    public class Campanya
    {
        public Guid Id { get; private set; }
        public string Nom { get; private set; } = string.Empty;
        public decimal PressupostTotal { get; private set; }
        public decimal PressupostConsumit { get; private set; }
        public DateTimeOffset DataInici { get; private set; }
        public DateTimeOffset DataFi { get; private set; }
        public EstatCampanya Estat { get; private set; }

        private Campanya() { }

        public Campanya(string nom, decimal pressupostTotal, DateTimeOffset dataInici, DateTimeOffset dataFi)
        {
            if (string.IsNullOrWhiteSpace(nom) || nom.Trim().Length < 3)
            {
                throw new ArgumentException(
                    "El nom ha de tenir almenys 3 caràcters.",
                    nameof(nom));
            }

            if (pressupostTotal <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pressupostTotal),
                    "El pressupost total ha de ser positiu.");
            }
            if (dataFi <= dataInici)
            {
                throw new ArgumentException(
                    "La data de fi ha de ser posterior a la data d'inici.",
                    nameof(dataFi));
            }

            Id = Guid.NewGuid();
            Nom = nom.Trim();
            PressupostTotal = pressupostTotal;
            PressupostConsumit = 0;
            DataInici = dataInici;
            DataFi = dataFi;
            Estat = EstatCampanya.ESBORRANY;
        }

        public void Activar(DateTimeOffset ara)
        {
            if (Estat == EstatCampanya.FINALITZADA)
                throw new InvalidOperationException(
                    "Una campanya finalitzada no es pot reactivar.");

            if (ara >= DataFi)
                throw new InvalidOperationException(
                    "No es pot activar una campanya caducada.");

            if (PressupostConsumit >= PressupostTotal)
                throw new InvalidOperationException(
                    "No queda pressupost disponible.");

            Estat = EstatCampanya.ACTIVA;
        }

        public void Pausar()
        {
            if (Estat == EstatCampanya.FINALITZADA)
                throw new InvalidOperationException(
                    "Una campanya finalitzada no es pot modificar.");

            if (Estat != EstatCampanya.ACTIVA)
                throw new InvalidOperationException(
                    "Només es pot pausar una campanya activa.");

            Estat = EstatCampanya.PAUSADA;
        }

        public void Consumir(decimal import, DateTimeOffset ara)
        {
            if (import <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(import),
                    "L'import ha de ser positiu.");

            if (Estat != EstatCampanya.ACTIVA)
                throw new InvalidOperationException(
                    "Només es pot consumir pressupost d'una campanya activa.");

            if (ara < DataInici || ara > DataFi)
                throw new InvalidOperationException(
                    "La campanya no es troba dins del període de vigència.");

            if (import > PressupostTotal - PressupostConsumit)
                throw new InvalidOperationException(
                    "El consum supera el pressupost disponible.");

            PressupostConsumit += import;

            if (PressupostConsumit == PressupostTotal)
                Estat = EstatCampanya.FINALITZADA;
        }
    }
}
