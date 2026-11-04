using ArchiCore.Domain.Digitization;
using ArchiCore.Domain.Digitization.Events;
using Xunit;

namespace ArchiCore.Domain.Tests.Digitization;

public sealed class DigitizationRequestTests
{
    private static DigitizationRequest CreateRequest()
        => DigitizationRequest.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;

    private static DigitizedFileInput CreateFile(string key = "projects/p1/folders/f1/master.pdf")
        => new(key, "expediente.pdf", "application/pdf", 1024, new string('a', 64), 12);

    [Fact]
    public void Create_requires_project_folder_and_requester()
    {
        var result = DigitizationRequest.Create(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());
        Assert.True(result.IsFailure);
        Assert.Equal("Digitization.ProjectRequired", result.Error.Code);
    }

    [Fact]
    public void Create_starts_as_requested_and_emits_event()
    {
        var request = CreateRequest();
        Assert.Equal(DigitizationRequestStatus.Requested, request.Status);
        Assert.IsType<DigitizationRequested>(Assert.Single(request.DomainEvents));
    }

    [Fact]
    public void Request_must_start_before_it_can_be_completed()
    {
        var request = CreateRequest();
        var result = request.Complete(Guid.NewGuid(), new[] { CreateFile() });
        Assert.True(result.IsFailure);
        Assert.Equal("Digitization.InvalidCompletionState", result.Error.Code);
    }

    [Fact]
    public void Completion_requires_at_least_one_valid_file()
    {
        var request = CreateRequest();
        Assert.True(request.Start(Guid.NewGuid()).IsSuccess);
        var result = request.Complete(Guid.NewGuid(), Array.Empty<DigitizedFileInput>());
        Assert.True(result.IsFailure);
        Assert.Equal("Digitization.FilesRequired", result.Error.Code);
        Assert.Equal(DigitizationRequestStatus.Processing, request.Status);
    }

    [Fact]
    public void Completion_registers_file_metadata_and_domain_events()
    {
        var request = CreateRequest();
        Assert.True(request.Start(Guid.NewGuid()).IsSuccess);
        request.ClearDomainEvents();

        var result = request.Complete(Guid.NewGuid(), new[] { CreateFile() });

        Assert.True(result.IsSuccess);
        Assert.Equal(DigitizationRequestStatus.Completed, request.Status);
        Assert.Single(request.Files);
        Assert.Contains(request.DomainEvents, item => item is DigitalFileGenerated);
        Assert.Contains(request.DomainEvents, item => item is DigitizationCompleted);
    }

    [Fact]
    public void Completion_rejects_unsafe_storage_keys()
    {
        var request = CreateRequest();
        Assert.True(request.Start(Guid.NewGuid()).IsSuccess);
        var result = request.Complete(Guid.NewGuid(), new[] { CreateFile("../outside.pdf") });
        Assert.True(result.IsFailure);
        Assert.Equal("Digitization.InvalidObjectKey", result.Error.Code);
        Assert.Equal(DigitizationRequestStatus.Processing, request.Status);
    }

    [Fact]
    public void Completion_rejects_duplicate_storage_keys()
    {
        var request = CreateRequest();
        Assert.True(request.Start(Guid.NewGuid()).IsSuccess);
        var result = request.Complete(Guid.NewGuid(), new[] { CreateFile(), CreateFile() });
        Assert.True(result.IsFailure);
        Assert.Equal("Digitization.DuplicateObjectKey", result.Error.Code);
    }

    [Fact]
    public void Completed_request_cannot_be_started_or_completed_again()
    {
        var request = CreateRequest();
        Assert.True(request.Start(Guid.NewGuid()).IsSuccess);
        Assert.True(request.Complete(Guid.NewGuid(), new[] { CreateFile() }).IsSuccess);
        var startAgain = request.Start(Guid.NewGuid());
        var completeAgain = request.Complete(Guid.NewGuid(), new[] { CreateFile("projects/p1/folders/f1/second.pdf") });
        Assert.True(startAgain.IsFailure);
        Assert.True(completeAgain.IsFailure);
        Assert.Equal(DigitizationRequestStatus.Completed, request.Status);
    }
}
