# Skill: CRUD Query Construction with Gateway.Core

This skill enables an LLM agent to construct CRUD queries using Gateway.Core with Couchbase.

## Required Imports

```csharp
using Couchbase;
using Couchbase.KeyValue;
using Couchbase.Query;
using Gateway.Core.Extensions;
using Gateway.Core.Filtering;
using Gateway.Core.Pagination;
```

---

## CRUD Query Patterns

### 1. GetAll

Retrieve all documents from a collection, ordered by a field.

```csharp
var scope = await bucket.ScopeAsync(scopeName);
var query = $"SELECT META().id, e.* FROM `{bucket.Name}`.`{scopeName}`.`{collectionName}` e ORDER BY e.createdAt DESC";
var results = await scope.QueryToListAsync<TEntity>(query);
```

### 2. GetPage

Retrieve paginated documents with optional filtering.

```csharp
var scope = await bucket.ScopeAsync(scopeName);
var filter = new FilterBuilder<TEntity>();

// Apply optional filters
if (!string.IsNullOrEmpty(category))
{
    filter.Where("category", category);
}

if (isActive.HasValue)
{
    filter.Where("isActive", isActive.Value);
}

// Sorting
filter.OrderBy("createdAt", descending: true);

// Pagination (fetch one extra to detect next page)
var paginationOptions = new PaginationOptions();
var effectivePageSize = paginationOptions.GetEffectivePageSize(pageSize);
var offset = (pageNumber - 1) * effectivePageSize;
filter.Skip(offset).Take(effectivePageSize + 1);

// Build and execute query
var whereClause = filter.Build();
var query = $"SELECT META().id, e.* FROM `{bucket.Name}`.`{scopeName}`.`{collectionName}` e {whereClause}";

var queryOptions = new QueryOptions();
foreach (var param in filter.Parameters)
{
    queryOptions.Parameter(param.Key, param.Value ?? DBNull.Value);
}

var results = await scope.QueryToListAsync<TEntity>(query, queryOptions);

// Build paged result
var hasNextPage = results.Count > effectivePageSize;
var items = hasNextPage ? results.Take(effectivePageSize).ToList() : results;

var pagedResult = new PagedResult<TEntity>(
    items: items,
    pageNumber: pageNumber,
    pageSize: effectivePageSize,
    hasMoreItems: hasNextPage
);
```

### 3. GetById

Retrieve a single document by key.

```csharp
var scope = await bucket.ScopeAsync(scopeName);
var collection = scope.Collection(collectionName);
var entity = await collection.GetAsync<TEntity>(id);  // Returns null if not found
```

### 4. Create

Insert a new document.

```csharp
var id = $"{entityPrefix}::{Guid.NewGuid()}";  // e.g., "product::abc-123"
var entity = new TEntity
{
    Id = id,
    // ... set properties from request
    CreatedAt = DateTime.UtcNow
};

var scope = await bucket.ScopeAsync(scopeName);
var collection = scope.Collection(collectionName);
await collection.InsertAsync(id, entity);
```

### 5. Update

Replace an existing document.

```csharp
var scope = await bucket.ScopeAsync(scopeName);
var collection = scope.Collection(collectionName);

// Fetch existing
var existing = await collection.GetAsync<TEntity>(id);
if (existing is null) { /* handle not found */ }

// Update using record 'with' expression
var updated = existing with
{
    // ... set properties from request
    UpdatedAt = DateTime.UtcNow
};

await collection.ReplaceAsync(id, updated);
```

### 6. Delete

Remove a document by key.

```csharp
var scope = await bucket.ScopeAsync(scopeName);
var collection = scope.Collection(collectionName);
await collection.RemoveAsync(id);
```

---

## FilterBuilder Reference

| Method | SQL++ Output | Example |
|--------|--------------|---------|
| `Where(prop, value)` | `prop = $p0` | `filter.Where("status", "active")` |
| `WhereNotEqual(prop, value)` | `prop != $p0` | `filter.WhereNotEqual("status", "deleted")` |
| `WhereGreaterThan(prop, value)` | `prop > $p0` | `filter.WhereGreaterThan("price", 100)` |
| `WhereLessThan(prop, value)` | `prop < $p0` | `filter.WhereLessThan("quantity", 10)` |
| `WhereGreaterThanOrEqual(prop, value)` | `prop >= $p0` | `filter.WhereGreaterThanOrEqual("rating", 4)` |
| `WhereLessThanOrEqual(prop, value)` | `prop <= $p0` | `filter.WhereLessThanOrEqual("age", 65)` |
| `WhereLike(prop, pattern)` | `prop LIKE $p0` | `filter.WhereLike("name", "%phone%")` |
| `WhereContains(prop, value)` | `CONTAINS(prop, $p0)` | `filter.WhereContains("desc", "premium")` |
| `WhereIn(prop, values)` | `prop IN $p0` | `filter.WhereIn("category", ["A", "B"])` |
| `WhereNull(prop)` | `prop IS NULL` | `filter.WhereNull("deletedAt")` |
| `WhereNotNull(prop)` | `prop IS NOT NULL` | `filter.WhereNotNull("email")` |
| `WhereBetween(prop, min, max)` | `prop BETWEEN $p0 AND $p1` | `filter.WhereBetween("price", 10, 100)` |
| `OrderBy(prop, desc)` | `ORDER BY prop [DESC]` | `filter.OrderBy("createdAt", true)` |
| `Skip(n)` | `OFFSET n` | `filter.Skip(20)` |
| `Take(n)` | `LIMIT n` | `filter.Take(10)` |

**Build Methods:**
- `filter.Build()` - Returns full clause: `WHERE ... ORDER BY ... LIMIT ... OFFSET ...`
- `filter.BuildWhereClause()` - Returns only the WHERE conditions
- `filter.Parameters` - Dictionary of parameter names to values

---

## Query Format

```
SELECT META().id, alias.* FROM `bucketName`.`scopeName`.`collectionName` alias {filterClause}
```

- `META().id` - Includes document key in results
- `alias.*` - All document properties
- Backticks escape identifiers

---

## Key Conventions

| Item | Convention |
|------|------------|
| ID format | `{entity-type}::{guid}` |
| Timestamps | `CreatedAt` on create, `UpdatedAt` on update |
| Null handling | `GetAsync` returns `null` if not found |
| Pagination | Fetch `pageSize + 1` to detect `hasNextPage` |
