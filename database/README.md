# Database setup

For a fresh database, run the scripts in numeric order with SQL Server Management Studio or `sqlcmd`:

1. `00_create_database.sql`
2. `01_create_users.sql`
3. `02_create_customers.sql`
4. `03_create_orders.sql`
5. `04_create_order_items.sql`
6. `05_create_refresh_sessions.sql`
7. `06_triggers.sql`
8. `07_seed_data.sql`
9. `08_views_dashboard.sql`

For a database created by the original scripts, run only `09_auth_identity_migration.sql`. It adds `DisplayName`, widens the user email column, and creates refresh-session storage without recreating tables or deleting existing rows. Do not rerun the initial schema or seed scripts against an existing database.

The seed script intentionally does not create a demo user. Register a user through AuthService so the password is hashed with the same algorithm used by the application. The seed script can be run again without duplicating its sample customers, orders, or items.

`Users.Id` is an `INT`, which AuthService uses for account IDs. Refresh tokens are represented by a SHA-256 digest, never by the raw token.