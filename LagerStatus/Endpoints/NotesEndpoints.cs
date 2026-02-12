using WarehouseAPI.DTOs;
using WarehouseAPI.Services;

namespace WarehouseAPI.Middleware;

public static class NotesEndpoints
{
    public static void RegisterNotesEndpoints(this WebApplication app)
    {
        // Get all notes with optional filtering
        app.MapGet("/notes", (HttpContext context, IAuthService authService, INotesService notesService,
            string? entityType, string? entityId, string? priority, bool? isPinned, bool? isResolved) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService))
                return Results.Unauthorized();

            var filter = new NoteFilterRequest
            {
                EntityType = entityType,
                EntityId = entityId,
                Priority = priority,
                IsPinned = isPinned,
                IsResolved = isResolved
            };

            return Results.Ok(notesService.GetAllNotes(filter));
        });

        // Get pinned notes
        app.MapGet("/notes/pinned", (HttpContext context, IAuthService authService, INotesService notesService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService))
                return Results.Unauthorized();

            return Results.Ok(notesService.GetPinnedNotes());
        });

        // Get unresolved notes
        app.MapGet("/notes/unresolved", (HttpContext context, IAuthService authService, INotesService notesService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService))
                return Results.Unauthorized();

            return Results.Ok(notesService.GetUnresolvedNotes());
        });

        // Get specific note
        app.MapGet("/notes/{id}", (HttpContext context, string id, IAuthService authService, INotesService notesService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService))
                return Results.Unauthorized();

            var note = notesService.GetNote(id);
            return note != null ? Results.Ok(note) : Results.NotFound();
        });

        // Get notes for specific entity
        app.MapGet("/notes/entity/{entityType}/{entityId}", (HttpContext context, string entityType, string entityId,
            IAuthService authService, INotesService notesService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService))
                return Results.Unauthorized();

            return Results.Ok(notesService.GetNotesForEntity(entityType, entityId));
        });

        // Create note
        app.MapPost("/notes", async (HttpContext context, IAuthService authService, INotesService notesService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService))
                return Results.Unauthorized();

            var req = await context.Request.ReadFromJsonAsync<CreateNoteRequest>();
            if (req == null) return Results.BadRequest("Invalid request");

            var token = AuthMiddleware.GetAuthToken(context);
            var username = authService.GetUsernameFromToken(token) ?? "unknown";

            var note = notesService.CreateNote(req, username);
            return Results.Ok(note);
        });

        // Update note
        app.MapPut("/notes/{id}", async (HttpContext context, string id, IAuthService authService, INotesService notesService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService))
                return Results.Unauthorized();

            var req = await context.Request.ReadFromJsonAsync<UpdateNoteRequest>();
            if (req == null) return Results.BadRequest("Invalid request");

            try
            {
                var note = notesService.UpdateNote(id, req);
                return Results.Ok(note);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        // Delete note
        app.MapDelete("/notes/{id}", (HttpContext context, string id, IAuthService authService, INotesService notesService) =>
        {
            if (!AuthMiddleware.IsAuthenticated(context, authService))
                return Results.Unauthorized();

            var deleted = notesService.DeleteNote(id);
            return deleted ? Results.Ok() : Results.NotFound();
        });
    }
}
