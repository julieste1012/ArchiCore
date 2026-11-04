using ArchiCore.Domain.DocumentaryReception;
using ArchiCore.Domain.DocumentaryReception.Events;
using ReceptionAggregate = ArchiCore.Domain.DocumentaryReception.DocumentaryReception;
using Xunit;

namespace ArchiCore.Domain.Tests.Receptions;

public sealed class DocumentaryReceptionTests
{
    [Fact]
    public void Create_records_contract_customer_receiver_and_created_event()
    {
        var contractId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();

        var result = ReceptionAggregate.Create(contractId, customerId, receiverId, "Entrega programada.");

        Assert.True(result.IsSuccess);
        var reception = result.Value;
        Assert.Equal(contractId, reception.ContractId);
        Assert.Equal(customerId, reception.CustomerId);
        Assert.Equal(receiverId, reception.ReceivedByUserId);
        Assert.Equal(DocumentaryReceptionStatus.Received, reception.Status);
        Assert.IsType<DocumentaryReceptionCreated>(Assert.Single(reception.DomainEvents));
    }

    [Fact]
    public void Counting_cannot_be_recorded_before_a_safe_physical_inspection()
    {
        var reception = CreateReception();

        var result = reception.RecordCounting(Guid.NewGuid(), 10, 100, 10, 100, null);

        Assert.True(result.IsFailure);
        Assert.Equal("Reception.InvalidCountingState", result.Error.Code);
    }

    [Fact]
    public void Inspection_with_unacceptable_physical_condition_suspends_the_reception()
    {
        var reception = CreateReception();
        var inspectorId = Guid.NewGuid();

        var result = reception.RecordInspection(inspectorId, false, "Se detectó humedad y sellos rotos.");

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentaryReceptionStatus.Suspended, reception.Status);
        Assert.Equal(inspectorId, reception.SuspendedByUserId);
        Assert.NotNull(reception.SuspensionReason);
        Assert.Contains(reception.DomainEvents, item => item is DocumentaryReceptionSuspended);
    }

    [Fact]
    public void Counting_discrepancies_require_a_report_and_coordinator_approval()
    {
        var reception = CreateReception();
        Assert.True(reception.RecordInspection(Guid.NewGuid(), true, "Sin daños aparentes.").IsSuccess);

        var missingReport = reception.RecordCounting(Guid.NewGuid(), 10, 100, 9, 98, null);
        Assert.True(missingReport.IsFailure);
        Assert.Equal("Reception.DiscrepancyReportRequired", missingReport.Error.Code);

        var countedBy = Guid.NewGuid();
        var count = reception.RecordCounting(countedBy, 10, 100, 9, 98, "Falta una caja y dos carpetas.");
        Assert.True(count.IsSuccess);
        Assert.Equal(DocumentaryReceptionStatus.CountingRecorded, reception.Status);
        Assert.Equal(countedBy, reception.CountedByUserId);

        var approvedBy = Guid.NewGuid();
        var approval = reception.ApproveCounting(approvedBy);
        Assert.True(approval.IsSuccess);
        Assert.Equal(DocumentaryReceptionStatus.Counted, reception.Status);
        Assert.Equal(approvedBy, reception.ApprovedByUserId);
        Assert.NotNull(reception.ApprovedAtUtc);
        Assert.Contains(reception.DomainEvents, item => item is DocumentaryReceptionCountingApproved);
    }

    [Fact]
    public void Negative_physical_counts_are_rejected()
    {
        var reception = CreateReception();
        Assert.True(reception.RecordInspection(Guid.NewGuid(), true, null).IsSuccess);

        var result = reception.RecordCounting(Guid.NewGuid(), 1, 1, -1, 1, "Invalid count.");

        Assert.True(result.IsFailure);
        Assert.Equal("Reception.InvalidCount", result.Error.Code);
    }

    private static ReceptionAggregate CreateReception()
        => ReceptionAggregate.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Material recibido.").Value;
}
