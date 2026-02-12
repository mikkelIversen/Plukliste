using WarehouseAPI.Models;
using WarehouseAPI.DTOs;

namespace WarehouseAPI.Services;

public class NotesService : INotesService
{
    private readonly IDataService _dataService;

    public NotesService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public List<Note> GetAllNotes(NoteFilterRequest? filter = null)
    {
        var notes = _dataService.Load<Note>("notes.json");

        if (filter == null)
            return notes.OrderByDescending(n => n.IsPinned)
                       .ThenByDescending(n => n.CreatedAt)
                       .ToList();

        var query = notes.AsEnumerable();

        if (!string.IsNullOrEmpty(filter.EntityType))
            query = query.Where(n => n.EntityType == filter.EntityType);

        if (!string.IsNullOrEmpty(filter.EntityId))
            query = query.Where(n => n.EntityId == filter.EntityId);

        if (!string.IsNullOrEmpty(filter.Priority))
            query = query.Where(n => n.Priority == filter.Priority);

        if (filter.IsPinned.HasValue)
            query = query.Where(n => n.IsPinned == filter.IsPinned.Value);

        if (filter.IsResolved.HasValue)
            query = query.Where(n => n.IsResolved == filter.IsResolved.Value);

        if (filter.Tags != null && filter.Tags.Any())
            query = query.Where(n => n.Tags.Any(t => filter.Tags.Contains(t)));

        return query.OrderByDescending(n => n.IsPinned)
                   .ThenByDescending(n => n.CreatedAt)
                   .ToList();
    }

    public Note? GetNote(string id)
    {
        var notes = _dataService.Load<Note>("notes.json");
        return notes.FirstOrDefault(n => n.Id == id);
    }

    public List<Note> GetNotesForEntity(string entityType, string entityId)
    {
        var notes = _dataService.Load<Note>("notes.json");
        return notes.Where(n => n.EntityType == entityType && n.EntityId == entityId)
                   .OrderByDescending(n => n.IsPinned)
                   .ThenByDescending(n => n.CreatedAt)
                   .ToList();
    }

    public Note CreateNote(CreateNoteRequest request, string createdBy)
    {
        var notes = _dataService.Load<Note>("notes.json");

        var note = new Note
        {
            Id = Guid.NewGuid().ToString(),
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Title = request.Title,
            Content = request.Content,
            CreatedBy = createdBy,
            CreatedAt = DateTime.Now,
            Priority = request.Priority,
            Tags = request.Tags,
            IsPinned = request.IsPinned,
            IsResolved = false
        };

        notes.Add(note);
        _dataService.Save("notes.json", notes);

        return note;
    }

    public Note UpdateNote(string id, UpdateNoteRequest request)
    {
        var notes = _dataService.Load<Note>("notes.json");
        var note = notes.FirstOrDefault(n => n.Id == id);

        if (note == null)
            throw new KeyNotFoundException($"Note med ID {id} findes ikke");

        if (request.Title != null)
            note.Title = request.Title;

        if (request.Content != null)
            note.Content = request.Content;

        if (request.Priority != null)
            note.Priority = request.Priority;

        if (request.Tags != null)
            note.Tags = request.Tags;

        if (request.IsPinned.HasValue)
            note.IsPinned = request.IsPinned.Value;

        if (request.IsResolved.HasValue)
            note.IsResolved = request.IsResolved.Value;

        note.UpdatedAt = DateTime.Now;

        _dataService.Save("notes.json", notes);
        return note;
    }

    public bool DeleteNote(string id)
    {
        var notes = _dataService.Load<Note>("notes.json");
        var countBefore = notes.Count;
        notes.RemoveAll(n => n.Id == id);

        if (notes.Count < countBefore)
        {
            _dataService.Save("notes.json", notes);
            return true;
        }

        return false;
    }

    public List<Note> GetPinnedNotes()
    {
        var notes = _dataService.Load<Note>("notes.json");
        return notes.Where(n => n.IsPinned && !n.IsResolved)
                   .OrderByDescending(n => n.CreatedAt)
                   .ToList();
    }

    public List<Note> GetUnresolvedNotes()
    {
        var notes = _dataService.Load<Note>("notes.json");
        return notes.Where(n => !n.IsResolved)
                   .OrderByDescending(n => n.IsPinned)
                   .ThenByDescending(n => n.CreatedAt)
                   .ToList();
    }
}
