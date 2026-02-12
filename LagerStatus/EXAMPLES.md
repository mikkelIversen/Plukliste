# API Usage Examples

## Notes System Usage Examples

### 1. Create a Product-Related Note

When you notice a product issue or need to track something:

```bash
POST http://localhost:5000/notes
Authorization: your-token
Content-Type: application/json

{
  "entityType": "product",
  "entityId": "LAPTOP-001",
  "title": "Quality issue reported",
  "content": "Customer reported screen flickering on batch #2024-03. Contact manufacturer for replacement units.",
  "priority": "high",
  "tags": ["quality-issue", "manufacturer", "urgent"],
  "isPinned": true
}
```

### 2. Create a Picklist Note

Add notes to track picking issues or special instructions:

```bash
POST http://localhost:5000/notes
Authorization: your-token
Content-Type: application/json

{
  "entityType": "picklist",
  "entityId": "PL-12345",
  "title": "Special handling required",
  "content": "Items are fragile. Use extra bubble wrap and mark boxes as fragile.",
  "priority": "normal",
  "tags": ["fragile", "special-handling"],
  "isPinned": false
}
```

### 3. Create a General Warehouse Note

For general information not tied to specific entities:

```bash
POST http://localhost:5000/notes
Authorization: your-token
Content-Type: application/json

{
  "entityType": "general",
  "entityId": null,
  "title": "Warehouse maintenance scheduled",
  "content": "HVAC maintenance scheduled for Saturday 8am-12pm. North wing will be offline.",
  "priority": "urgent",
  "tags": ["maintenance", "downtime"],
  "isPinned": true
}
```

### 4. Get All Notes for a Specific Product

```bash
GET http://localhost:5000/notes/entity/product/LAPTOP-001
Authorization: your-token
```

### 5. Get All Pinned Notes

View all important pinned notes:

```bash
GET http://localhost:5000/notes/pinned
Authorization: your-token
```

### 6. Get Unresolved Notes

See all notes that still need attention:

```bash
GET http://localhost:5000/notes/unresolved
Authorization: your-token
```

### 7. Filter Notes by Priority

Get only high-priority notes:

```bash
GET http://localhost:5000/notes?priority=high
Authorization: your-token
```

Get urgent unresolved notes:

```bash
GET http://localhost:5000/notes?priority=urgent&isResolved=false
Authorization: your-token
```

### 8. Update a Note

Mark a note as resolved:

```bash
PUT http://localhost:5000/notes/note-id-here
Authorization: your-token
Content-Type: application/json

{
  "isResolved": true
}
```

Update note content and priority:

```bash
PUT http://localhost:5000/notes/note-id-here
Authorization: your-token
Content-Type: application/json

{
  "content": "Issue resolved. Manufacturer sent replacement units.",
  "priority": "low",
  "isResolved": true
}
```

### 9. Pin/Unpin a Note

```bash
PUT http://localhost:5000/notes/note-id-here
Authorization: your-token
Content-Type: application/json

{
  "isPinned": true
}
```

### 10. Delete a Note

```bash
DELETE http://localhost:5000/notes/note-id-here
Authorization: your-token
```

## Complete Workflow Example

### Scenario: Low Stock Alert and Reorder Process

**Step 1: Check low stock items**
```bash
GET http://localhost:5000/inventory/low-stock
Authorization: your-token
```

**Step 2: Create a note for reordering**
```bash
POST http://localhost:5000/notes
Authorization: your-token
Content-Type: application/json

{
  "entityType": "product",
  "entityId": "WIDGET-500",
  "title": "Reorder required - below minimum stock",
  "content": "Current stock: 5 units. Minimum: 20 units. Contact supplier XYZ Corp. Last order: 100 units @ $15/unit",
  "priority": "high",
  "tags": ["reorder", "low-stock", "supplier-xyz"],
  "isPinned": true
}
```

**Step 3: When order is placed, update the note**
```bash
PUT http://localhost:5000/notes/note-id-here
Authorization: your-token
Content-Type: application/json

{
  "content": "Reorder placed: PO #98765 for 100 units. Expected delivery: 2024-02-20. Tracking: TRACK123",
  "priority": "normal"
}
```

**Step 4: When stock arrives, adjust inventory**
```bash
POST http://localhost:5000/inventory/WIDGET-500/adjust
Authorization: your-token
Content-Type: application/json

{
  "quantity": 100,
  "reason": "Received PO #98765"
}
```

**Step 5: Mark the note as resolved**
```bash
PUT http://localhost:5000/notes/note-id-here
Authorization: your-token
Content-Type: application/json

{
  "isResolved": true,
  "isPinned": false
}
```

## Filter Combinations

### Get product notes that are high priority and unresolved
```bash
GET http://localhost:5000/notes?entityType=product&priority=high&isResolved=false
```

### Get all pinned product notes
```bash
GET http://localhost:5000/notes?entityType=product&isPinned=true
```

### Get general warehouse notes
```bash
GET http://localhost:5000/notes?entityType=general
```

## Integration with Other Endpoints

### Creating a Product with Initial Note

```bash
# 1. Create product
POST http://localhost:5000/products
{
  "id": "NEW-PROD-001",
  "name": "New Product",
  "category": "Electronics",
  "location": "A1-B2",
  "minStock": 10,
  "initialQty": 50
}

# 2. Add setup note
POST http://localhost:5000/notes
{
  "entityType": "product",
  "entityId": "NEW-PROD-001",
  "title": "Initial setup complete",
  "content": "Product added to system. Initial stock of 50 units received from Supplier ABC.",
  "priority": "normal",
  "tags": ["setup", "initial-stock"]
}
```

### Creating Picklist with Instructions Note

```bash
# 1. Create picklist
POST http://localhost:5000/picklists
{
  "id": "PL-001",
  "name": "Order #5432",
  "items": [
    { "productId": "PROD-001", "qty": 5 }
  ]
}

# 2. Add picking instructions
POST http://localhost:5000/notes
{
  "entityType": "picklist",
  "entityId": "PL-001",
  "title": "Special shipping instructions",
  "content": "Customer requested express shipping. Must ship by end of day.",
  "priority": "high",
  "tags": ["express", "deadline"],
  "isPinned": true
}
```

## Statistics with Notes

The stats endpoint now includes note metrics:

```bash
GET http://localhost:5000/stats
Authorization: your-token
```

Response includes:
```json
{
  "totalProducts": 150,
  "totalStock": 5430,
  "totalReserved": 234,
  "activePicklists": 8,
  "completedPicklists": 142,
  "lowStockItems": 12,
  "totalNotes": 45,
  "unresolvedNotes": 15,
  "pinnedNotes": 5,
  "urgentNotes": 3
}
```
