namespace ArchiCore.Domain.Boxes;

/// <summary>Identity reference to a Folder belonging to this physical Box.</summary>
public sealed class BoxFolderReference
{
    public Guid FolderId { get; private set; }

    private BoxFolderReference() { }
    internal BoxFolderReference(Guid folderId) => FolderId = folderId;
}
