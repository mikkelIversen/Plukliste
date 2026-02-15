##  Project Structure

```
WarehouseAPI/
├── Program.cs                  
├── Models/                   
│   ├── User.cs
│   ├── Product.cs
│   ├── Picklist.cs
│   └── Note.cs               
├── DTOs/                      
│   ├── AuthDtos.cs
│   ├── ProductDtos.cs
│   └── NoteDtos.cs           
├── Services/                  
│   ├── IDataService.cs
│   ├── JsonDataService.cs
│   ├── IAuthService.cs
│   ├── AuthService.cs
│   ├── IUserService.cs
│   ├── UserService.cs
│   ├── IProductService.cs
│   ├── ProductService.cs
│   ├── IInventoryService.cs
│   ├── InventoryService.cs
│   ├── IPicklistService.cs
│   ├── PicklistService.cs
│   ├── INotesService.cs      
│   ├── NotesService.cs       
│   ├── IStatsService.cs
│   └── StatsService.cs
├── Endpoints/                 
│   ├── AuthEndpoints.cs
│   ├── UserEndpoints.cs
│   ├── ProductEndpoints.cs
│   ├── InventoryEndpoints.cs
│   ├── PicklistEndpoints.cs
│   ├── NotesEndpoints.cs     # New: Notes endpoints
│   └── StatsEndpoints.cs
└── Middleware/
    └── AuthMiddleware.cs
```

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK or later

### Running the Application

```bash
cd WarehouseAPI
dotnet run
```

The API will be available at `http://localhost:5000`

### Default Credentials
- **Username**: `admin`
- **Password**: `admin123`

##  API Endpoints

### Authentication
- `POST /auth/login` - Login and receive token
- `POST /auth/logout` - Logout and invalidate token
- `POST /auth/validate` - Validate current token

### Users
- `GET /users` - Get all users
- `POST /users` - Create new user
- `DELETE /users/{username}` - Delete user

### Products
- `GET /products` - Get all products
- `GET /products/{id}` - Get specific product
- `POST /products` - Create new product
- `PUT /products/{id}` - Update product
- `DELETE /products/{id}` - Delete product
- `GET /categories` - Get all product categories

### Inventory
- `GET /inventory` - Get all inventory items
- `GET /inventory/low-stock` - Get low stock items
- `POST /inventory/{productId}/adjust` - Adjust inventory quantity

### Picklists
- `GET /picklists` - Get all picklists
- `GET /picklists/{id}` - Get specific picklist
- `POST /picklists` - Create new picklist
- `POST /picklists/{id}/complete` - Complete picklist
- `POST /picklists/{id}/cancel` - Cancel picklist
- `DELETE /picklists/{id}` - Delete picklist

### Notes (NEW!)
- `GET /notes` - Get all notes (with optional filters)
- `GET /notes/pinned` - Get pinned notes
- `GET /notes/unresolved` - Get unresolved notes
- `GET /notes/{id}` - Get specific note
- `GET /notes/entity/{entityType}/{entityId}` - Get notes for entity
- `POST /notes` - Create new note
- `PUT /notes/{id}` - Update note
- `DELETE /notes/{id}` - Delete note

### Statistics
- `GET /stats` - Get warehouse statistics

## 📋 Notes System

The integrated notes system allows you to:

1. **Attach to Entities**: Link notes to products, picklists, or inventory
2. **General Notes**: Create standalone notes for general warehouse information
3. **Prioritize**: Set priority levels (low, normal, high, urgent)
4. **Organize**: Use tags for better organization
5. **Pin Important**: Pin critical notes for visibility
6. **Track Resolution**: Mark notes as resolved when addressed

### Note Structure

```json
{
  "id": "unique-id",
  "entityType": "product|picklist|inventory|general",
  "entityId": "entity-id or null",
  "title": "Note title",
  "content": "Note content",
  "createdBy": "username",
  "createdAt": "2024-02-12T10:00:00",
  "updatedAt": "2024-02-12T11:00:00",
  "priority": "normal|low|high|urgent",
  "tags": ["tag1", "tag2"],
  "isPinned": false,
  "isResolved": false
}
```

### Example: Creating a Note

```bash
POST /notes
Authorization: your-session-token
Content-Type: application/json

{
  "entityType": "product",
  "entityId": "PROD-001",
  "title": "Reorder needed",
  "content": "Stock running low, contact supplier",
  "priority": "high",
  "tags": ["reorder", "urgent"],
  "isPinned": true
}
```

##  Authentication

All endpoints except `/auth/login` require authentication. Include the session token in the Authorization header:

```
Authorization: your-session-token-here
```

##  Data Storage

Data is stored in JSON files in the `Data/` directory:
- `users.json` - User accounts
- `sessions.json` - Active sessions
- `products.json` - Product catalog
- `inventory.json` - Inventory levels
- `picklists.json` - Picking lists
- `notes.json` - Notes (NEW!)

