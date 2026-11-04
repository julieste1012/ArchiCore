using ArchiCore.Domain.Common;
using ArchiCore.Domain.Loans.Events;

namespace ArchiCore.Domain.Loans;

/// <summary>
/// Aggregate Root que controla el préstamo temporal de una carpeta documental.
/// La autorización efectiva por rol/proyecto se valida en Application/API.
/// </summary>
public sealed class Loan : Entity, IAggregateRoot
{
    public Guid ProjectId { get; private set; }
    public Guid FolderId { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public DateTimeOffset DueAtUtc { get; private set; }
    public LoanStatus Status { get; private set; }
    public Guid? AuthorizedByUserId { get; private set; }
    public DateTimeOffset? AuthorizedAtUtc { get; private set; }
    public Guid? DeliveredByUserId { get; private set; }
    public DateTimeOffset? DeliveredAtUtc { get; private set; }
    public Guid? ReturnedByUserId { get; private set; }
    public DateTimeOffset? ReturnedAtUtc { get; private set; }
    public DateTimeOffset? OverdueDetectedAtUtc { get; private set; }

    private Loan() { }

    private Loan(Guid projectId, Guid folderId, Guid requestedByUserId, DateTimeOffset requestedAtUtc, DateTimeOffset dueAtUtc)
    {
        ProjectId = projectId;
        FolderId = folderId;
        RequestedByUserId = requestedByUserId;
        RequestedAtUtc = requestedAtUtc;
        DueAtUtc = dueAtUtc;
        Status = LoanStatus.Requested;
    }

    public static Result<Loan> Create(
        Guid projectId,
        Guid folderId,
        Guid requestedByUserId,
        DateTimeOffset dueAtUtc)
    {
        if (projectId == Guid.Empty)
            return Result.Failure<Loan>(new Error("Loan.ProjectRequired", "El proyecto asociado es obligatorio."));
        if (folderId == Guid.Empty)
            return Result.Failure<Loan>(new Error("Loan.FolderRequired", "La carpeta solicitada es obligatoria."));
        if (requestedByUserId == Guid.Empty)
            return Result.Failure<Loan>(new Error("Loan.RequesterRequired", "El solicitante es obligatorio."));

        var requestedAtUtc = DateTimeOffset.UtcNow;
        if (dueAtUtc <= requestedAtUtc)
            return Result.Failure<Loan>(new Error("Loan.InvalidDueDate", "La fecha de devolución prevista debe ser posterior a la solicitud."));

        dueAtUtc = dueAtUtc.ToUniversalTime();
        var loan = new Loan(projectId, folderId, requestedByUserId, requestedAtUtc, dueAtUtc);
        loan.AddDomainEvent(new LoanRequested(loan.Id, projectId, folderId, requestedByUserId, requestedAtUtc, dueAtUtc));
        return Result.Success(loan);
    }

    public Result Authorize(Guid authorizedByUserId)
    {
        var validation = EnsureStatusIs(LoanStatus.Requested, "Loan.InvalidAuthorization", "Solo un préstamo Requested puede autorizarse.");
        if (validation.IsFailure) return validation;
        if (authorizedByUserId == Guid.Empty)
            return Result.Failure(new Error("Loan.AuthorizerRequired", "El usuario que autoriza el préstamo es obligatorio."));

        AuthorizedByUserId = authorizedByUserId;
        AuthorizedAtUtc = DateTimeOffset.UtcNow;
        Status = LoanStatus.Authorized;
        MarkAsUpdated();
        AddDomainEvent(new LoanAuthorized(Id, FolderId, authorizedByUserId, AuthorizedAtUtc.Value));
        return Result.Success();
    }

    public Result Deliver(Guid deliveredByUserId)
    {
        var validation = EnsureStatusIs(LoanStatus.Authorized, "Loan.InvalidDelivery", "Solo un préstamo Authorized puede entregarse.");
        if (validation.IsFailure) return validation;
        if (deliveredByUserId == Guid.Empty)
            return Result.Failure(new Error("Loan.DelivererRequired", "El usuario que entrega la carpeta es obligatorio."));

        DeliveredByUserId = deliveredByUserId;
        DeliveredAtUtc = DateTimeOffset.UtcNow;
        Status = LoanStatus.Delivered;
        MarkAsUpdated();
        AddDomainEvent(new LoanDelivered(Id, FolderId, deliveredByUserId, DeliveredAtUtc.Value));
        return Result.Success();
    }

    public Result MarkOverdue(DateTimeOffset evaluatedAtUtc)
    {
        var validation = EnsureStatusIs(LoanStatus.Delivered, "Loan.InvalidOverdueTransition", "Solo un préstamo Delivered puede marcarse como vencido.");
        if (validation.IsFailure) return validation;

        evaluatedAtUtc = evaluatedAtUtc.ToUniversalTime();
        if (evaluatedAtUtc <= DueAtUtc)
            return Result.Failure(new Error("Loan.NotYetOverdue", "El préstamo todavía no supera su fecha prevista de devolución."));

        OverdueDetectedAtUtc = evaluatedAtUtc;
        Status = LoanStatus.Overdue;
        MarkAsUpdated();
        AddDomainEvent(new LoanOverdue(Id, FolderId, DueAtUtc, evaluatedAtUtc));
        return Result.Success();
    }

    public Result Return(Guid returnedByUserId, DateTimeOffset returnedAtUtc)
    {
        if (Status is not (LoanStatus.Delivered or LoanStatus.Overdue))
            return Result.Failure(new Error("Loan.InvalidReturn", "Solo un préstamo Delivered u Overdue puede devolverse."));
        if (returnedByUserId == Guid.Empty)
            return Result.Failure(new Error("Loan.ReturnerRequired", "El usuario que recibe la devolución es obligatorio."));

        returnedAtUtc = returnedAtUtc.ToUniversalTime();
        if (DeliveredAtUtc is null || returnedAtUtc < DeliveredAtUtc.Value)
            return Result.Failure(new Error("Loan.InvalidReturnDate", "La devolución no puede ser anterior a la entrega."));

        ReturnedByUserId = returnedByUserId;
        ReturnedAtUtc = returnedAtUtc;
        Status = LoanStatus.Returned;
        MarkAsUpdated();
        AddDomainEvent(new LoanReturned(Id, FolderId, returnedByUserId, returnedAtUtc));
        return Result.Success();
    }

    private Result EnsureStatusIs(LoanStatus expected, string errorCode, string errorMessage)
        => Status != expected ? Result.Failure(new Error(errorCode, errorMessage)) : Result.Success();
}
