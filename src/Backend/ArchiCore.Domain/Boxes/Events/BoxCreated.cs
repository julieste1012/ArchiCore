using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Boxes.Events;

/// <summary>
/// Evento de dominio generado cuando una <see cref="Box"/> es creada.
/// </summary>
public sealed class BoxCreated : DomainEvent
{
    public Guid BoxId { get; }
    public string BoxCode { get; }

    public BoxCreated(Guid boxId, string boxCode)
    {
        BoxId = boxId;
        BoxCode = boxCode;
    }
}
