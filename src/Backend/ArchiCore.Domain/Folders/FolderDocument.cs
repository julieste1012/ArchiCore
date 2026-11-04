using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.Folders;

/// <summary>Documento interno de una carpeta documental.</summary>
public sealed class FolderDocument : Entity
{
    private readonly List<Folio> _folios = new();
    private readonly List<Annex> _annexes = new();

    public string Title { get; private set; }
    public DateTime? DocumentDate { get; private set; }
    public IReadOnlyCollection<Folio> Folios => _folios.AsReadOnly();
    public IReadOnlyCollection<Annex> Annexes => _annexes.AsReadOnly();

    private FolderDocument()
    {
        Title = string.Empty;
    }

    internal FolderDocument(string title, DateTime? documentDate)
    {
        Title = title;
        DocumentDate = documentDate;
    }

    internal Result<int> AddFolio(int number)
    {
        var expectedNumber = _folios.Count + 1;
        if (number != expectedNumber)
            return Result.Failure<int>(new Error("Folder.InvalidFolioSequence", $"El siguiente folio esperado es {expectedNumber}."));

        var folio = new Folio(number);
        _folios.Add(folio);
        MarkAsUpdated();
        return Result.Success(folio.Number);
    }

    internal Result<int> AddAnnex(string title)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 250)
            return Result.Failure<int>(new Error("Folder.InvalidAnnexTitle", "El título del anexo es obligatorio y no puede superar 250 caracteres."));

        var sequence = _folios.Count + _annexes.Count + 1;
        _annexes.Add(new Annex(title.Trim(), sequence));
        MarkAsUpdated();
        return Result.Success(sequence);
    }
}

/// <summary>Folio numerado dentro de un documento.</summary>
public sealed class Folio : Entity
{
    public int Number { get; private set; }
    private Folio() { }
    internal Folio(int number) => Number = number;
}

/// <summary>Anexo unido al final del documento principal.</summary>
public sealed class Annex : Entity
{
    public string Title { get; private set; }
    public int Sequence { get; private set; }

    private Annex() => Title = string.Empty;

    internal Annex(string title, int sequence)
    {
        Title = title;
        Sequence = sequence;
    }
}
