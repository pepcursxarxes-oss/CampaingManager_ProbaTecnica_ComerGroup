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
                    "El pressupost total ha de ser estrictament positiu.");
            }

            Id = Guid.NewGuid();
            Nom = nom;
            PressupostTotal = pressupostTotal;
            PressupostConsumit = 0;
            DataInici = dataInici;
            DataFi = dataFi;
            Estat = EstatCampanya.ESBORRANY;
        }

    }
}
