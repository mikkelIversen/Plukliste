using WarehouseAPI.Models;
using WarehouseAPI.DTOs;

namespace WarehouseAPI.Services;

public interface INotesService
{
    List<Note> GetAllNotes(NoteFilterRequest? filter = null);
    Note? GetNote(string id);
    List<Note> GetNotesForEntity(string entityType, string entityId);
    Note CreateNote(CreateNoteRequest request, string createdBy);
    Note UpdateNote(string id, UpdateNoteRequest request);
    bool DeleteNote(string id);
    List<Note> GetPinnedNotes();
    List<Note> GetUnresolvedNotes();
}
