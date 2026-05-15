# Admin and Database Setup

This backend uses SQL Server through EF Core. The first schema migration creates:

- `UserAccounts`
- `Products`
- `ProductPriceHistories`

The migration also seeds the current Farmelo product catalog.

## Apply The Migration

Set `ConnectionStrings:DatabaseConnection` in environment-specific config or user secrets, then run:

```powershell
dotnet ef database update --project src\Farmelo.Data --startup-project src\Farmelo.API
```

If `dotnet ef` is not installed:

```powershell
dotnet tool install --global dotnet-ef
```

## Create The First Admin

Generate a password hash with the same PBKDF2 format used by the API:

```powershell
$Password = "ChangeThis@123"
$Iterations = 210000
$Salt = [byte[]]::new(16)
[System.Security.Cryptography.RandomNumberGenerator]::Fill($Salt)
$Key = [System.Security.Cryptography.Rfc2898DeriveBytes]::Pbkdf2(
  $Password,
  $Salt,
  $Iterations,
  [System.Security.Cryptography.HashAlgorithmName]::SHA256,
  32
)
"{0}${1}${2}${3}" -f "PBKDF2-SHA256", $Iterations, [Convert]::ToBase64String($Salt), [Convert]::ToBase64String($Key)
```

Insert the Admin with the generated hash:

```sql
INSERT INTO UserAccounts
    (FullName, Email, PasswordHash, Role, IsActive, CreatedBy, CreatedOn)
VALUES
    ('Farmelo Admin', 'admin@farmelo.local', '<PASTE_HASH_HERE>', 'Admin', 1, 'SYSTEM', SYSUTCDATETIME());
```

After this, log in from the frontend at `/login`. Admin users manage owner accounts; owners manage products and prices.
