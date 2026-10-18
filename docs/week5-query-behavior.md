# Week 5: EF Core Query Behavior

## SQL execution
- SQL commands I observed:A count query and a query retrieving the requested page.

- When the query actually executed:When CountAsync and ToArrayAsync were called. Building the IQueryable did not execute SQL.

## Projection
- Columns selected by the projection query:Id and CustomerName.
- Tracked entity count:0

## Change tracking
- Initial tracked entity state:Unchanged.
- State after changing the tracked entity:Modified.
- State of the untracked entity:Detached.

## Saving changes
- Original hours in the database:2
- Hours after changing the object without calling SaveChangesAsync:The tracked object had 3 hours, but the database still had 2.
- Why the database value stayed unchanged:The temporary endpoint did not call SaveChangesAsync.

## Filtering and pagination
- Why filtering happens before pagination:Pages must be selected from matching bookings. Paging first could leave out valid matches.
- Why sorting happens before Skip and Take:A consistent order makes page selection predictable when the underlying data is unchanged.


## Cleanup
- Temporary debug endpoints removed:Yes; both now return 404.
- Normal booking endpoints still work:GET /bookings returns 200.
- OpenAPI document still works:GET /openapi/v1.json returns 200 in Development.