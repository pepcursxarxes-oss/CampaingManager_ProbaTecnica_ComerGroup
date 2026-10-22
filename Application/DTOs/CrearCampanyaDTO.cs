using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public sealed record CrearCampanyaDTO(string Nom, decimal PressupostTotal, decimal PressupostConsumit, DateTimeOffset DataInici, DateTimeOffset DataFi);
}
