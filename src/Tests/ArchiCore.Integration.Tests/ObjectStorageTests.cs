using System.Text;
using ArchiCore.Application.Abstractions.Storage;
using ArchiCore.Infrastructure.Storage;
using Xunit;

namespace ArchiCore.Integration.Tests;

public sealed class ObjectStorageTests
{
    [Fact]
    public async Task Local_provider_stores_reads_and_deletes_an_object()
    {
        var root = Path.Combine(Path.GetTempPath(), "archicore-storage-tests", Guid.NewGuid().ToString("N"));
        try
        {
            var storage = new LocalFileSystemObjectStorage(root, 1024);
            var payload = Encoding.UTF8.GetBytes("ArchiCore storage test");

            await using (var input = new MemoryStream(payload))
            {
                await storage.StoreAsync("contracts/contract-01/document-01.txt", input, payload.Length, "text/plain");
            }

            await using var output = new MemoryStream();
            await storage.CopyToAsync("contracts/contract-01/document-01.txt", output);

            Assert.Equal(payload, output.ToArray());

            await storage.DeleteAsync("contracts/contract-01/document-01.txt");
            Assert.False(File.Exists(Path.Combine(root, "contracts", "contract-01", "document-01.txt")));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Theory]
    [InlineData("../secret.txt")]
    [InlineData("contracts/../secret.txt")]
    [InlineData("/rooted/key")]
    [InlineData("contracts//empty-segment.txt")]
    [InlineData("contracts\\file.txt")]
    [InlineData("contracts/file.txt?download=1")]
    [InlineData("contracts/#file.txt")]
    public void Object_key_policy_rejects_traversal_and_unsafe_boundaries(string key)
    {
        Assert.Throws<ArgumentException>(() => ObjectStorageKeyPolicy.Validate(key));
    }

    [Fact]
    public async Task Local_provider_rejects_an_upload_whose_declared_length_is_wrong()
    {
        var root = Path.Combine(Path.GetTempPath(), "archicore-storage-tests", Guid.NewGuid().ToString("N"));
        try
        {
            var storage = new LocalFileSystemObjectStorage(root, 1024);
            var payload = Encoding.UTF8.GetBytes("four");

            await using var input = new MemoryStream(payload);
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                storage.StoreAsync("test/mismatch.bin", input, contentLength: 3, "application/octet-stream"));

            Assert.False(File.Exists(Path.Combine(root, "test", "mismatch.bin")));
            Assert.Empty(Directory.Exists(Path.Combine(root, "test"))
                ? Directory.GetFiles(Path.Combine(root, "test"), "*.tmp")
                : Array.Empty<string>());
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Local_provider_enforces_its_configured_maximum_size()
    {
        var root = Path.Combine(Path.GetTempPath(), "archicore-storage-tests", Guid.NewGuid().ToString("N"));
        try
        {
            var storage = new LocalFileSystemObjectStorage(root, maxObjectSizeBytes: 3);
            await using var input = new MemoryStream(new byte[4]);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                storage.StoreAsync("test/too-large.bin", input, contentLength: 4, "application/octet-stream"));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
