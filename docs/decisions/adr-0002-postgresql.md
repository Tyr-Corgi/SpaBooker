# ADR-0002: Use PostgreSQL for Database

## Status
Accepted

## Date
2025-11-01

## Context
We needed to select a database for the SpaBooker application. Requirements include:
- Reliable ACID transactions for booking and payment operations
- Good performance for concurrent booking queries
- Cost-effective for small to medium scale
- Support for complex queries (availability checking, reporting)
- Easy deployment to cloud providers

## Decision
We chose **PostgreSQL** as the primary database.

## Consequences

### Positive
- **ACID compliance**: Strong transaction support for financial operations
- **Open source**: No licensing costs
- **Cloud support**: Available on Azure, AWS, GCP as managed service
- **EF Core support**: Excellent Entity Framework Core integration via Npgsql
- **Advanced features**: JSON support, full-text search, CTEs
- **Reliability**: Proven track record for production workloads
- **Concurrency**: MVCC provides excellent concurrent read/write performance

### Negative
- **Hosting complexity**: Slightly more complex than SQLite for local development
- **Resource usage**: Heavier than SQLite for small deployments
- **Learning curve**: Some PostgreSQL-specific syntax differences

### Neutral
- Requires Docker or local installation for development
- Connection pooling configuration needed for production

## Alternatives Considered

### SQL Server
- Rejected: Licensing costs for production
- Azure SQL pricing higher than Azure PostgreSQL

### SQLite
- Rejected: Not suitable for concurrent web application
- No support for multiple simultaneous writers

### MySQL/MariaDB
- Rejected: PostgreSQL has better EF Core support
- PostgreSQL's JSONB and advanced features preferred

### MongoDB
- Rejected: Relational model better fits booking domain
- ACID transactions more straightforward in PostgreSQL

## References
- [Npgsql - .NET PostgreSQL Provider](https://www.npgsql.org/)
- [Azure Database for PostgreSQL](https://azure.microsoft.com/en-us/products/postgresql/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

