## ⚠️ NOTE: Filtered & Compact Export
---

# IFileService.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using Microsoft.AspNetCore.Http;

namespace BackEnd.Application.Interfaces.Services
{
public interface IFileService
{
Task<Result<string>> UploadFileAsync(IFormFile file, string targetFolder, string expectedType);
Task<Result<List<string>>> UploadMultipleFilesAsync(IFormFileCollection files, string targetFolder, string expectedType);
Task<string> CalculateFileHashAsync(IFormFile file);
Result<bool> DeleteFile(string relativePath);
}
}
```

# DeliveryArea.cs
```cs
﻿using System;

namespace BackEnd.Domain.Entities.Notification
{
internal class DeliveryArea
{
}
}
```

# Notification.cs
```cs
﻿using System;

namespace BackEnd.Domain.Entities.Notification
{
internal class Notification
{
}
}
```

# GeneralDonation.cs
```cs
﻿using System;

namespace BackEnd.Domain.Entities.Payment
{
internal class GeneralDonation
{
}
}
```

# InKindDonation.cs
```cs
﻿using System;

namespace BackEnd.Domain.Entities.Payment
{
internal class InKindDonation
{
}
}
```

# PaymentRequest.cs
```cs
﻿using System;

namespace BackEnd.Domain.Entities.Payment
{
internal class PaymentRequest
{
}
}
```

# Sponsorship.cs
```cs
﻿using System;

namespace BackEnd.Domain.Entities.Sponsorship
{
internal class Sponsorship
{
}
}
```

# SponsorshipSubscription.cs
```cs
﻿using System;

namespace BackEnd.Domain.Entities.Sponsorship
{
internal class SponsorshipSubscription
{
}
}
```

# IAggregateRoot.cs
```cs
﻿using System;

namespace BackEnd.Domain.Interfaces
{
internal interface IAggregateRoot
{
}
}
```

# IDomainEvent.cs
```cs
﻿using System;

namespace BackEnd.Domain.Interfaces
{
internal interface IDomainEvent
{
}
}
```

# IEntity.cs
```cs
﻿using System;

namespace BackEnd.Domain.Interfaces
{
internal interface IEntity
{
}
}
```

# ApplicationDbContext.cs
```cs
﻿using BackEnd.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.DbContext
{
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
{
}

protected override void OnModelCreating(ModelBuilder builder)
{
base.OnModelCreating(builder);

builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

var adminRoleId = "admin-role-id";
var EmployeeRoleId = "employee-role-id";
var adminUserId = "admin-user-id";

builder.Entity<IdentityRole>().HasData(
new IdentityRole
{
Id = adminRoleId,
Name = "Admin",
NormalizedName = "ADMIN",
ConcurrencyStamp = "admin-role-concurrency"
},
new IdentityRole
{
Id = EmployeeRoleId,
Name = "Employee",
NormalizedName = "Employee",
ConcurrencyStamp = "employee-role-concurrency"
}
);

builder.Entity<ApplicationUser>().HasData(
new ApplicationUser
{
Id = adminUserId,
UserName = "admin@admin.com",
NormalizedUserName = "ADMIN@ADMIN.COM",
Email = "admin@admin.com",
NormalizedEmail = "ADMIN@ADMIN.COM",
EmailConfirmed = true,
PasswordHash = "AQAAAAIAAYagAAAAEDZffCZ1Jv0MApj6ocE4KMf3SPhwXC54xd93VsfFUTGo7wUq9IuNZL8SrGw7iMxqIg==",
SecurityStamp = "admin-user-security",
ConcurrencyStamp = "admin-user-concurrency",
FirstName = "Admin",
LastName = "User",
IsActive = true,
CreatedOn = new DateTime(2026, 1, 1)
}
);

builder.Entity<IdentityUserRole<string>>().HasData(
new IdentityUserRole<string>
{
UserId = adminUserId,
RoleId = adminRoleId
}
);
}

}
}
```

# EntityStateRepository.cs
```cs
﻿using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Application.Abstractions.Persistence.BackEnd.Application.Abstractions.Persistence;
using BackEnd.Application.Common.SearchCriteria;
using BackEnd.Domain.Common;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
public class EntityStateRepository<TEntity, TId>
: IEntityStateRepository<TEntity, TId>
where TEntity : BaseEntity<TId>
{
protected readonly ApplicationDbContext _context;
protected readonly DbSet<TEntity> _set;

public EntityStateRepository(ApplicationDbContext context)
{
_context = context;
_set = context.Set<TEntity>();
}

public async Task ActivateAsync(TId id)
{
var entity = await _set.FindAsync(id);
if (entity == null) return;

entity.IsActive = true;
entity.UpdatedOn = DateTime.UtcNow;
}

public async Task DeactivateAsync(TId id)
{
var entity = await _set.FindAsync(id);
if (entity == null) return;

entity.IsActive = false;
entity.UpdatedOn = DateTime.UtcNow;
}

public async Task SoftDeleteAsync(TId id)
{
var entity = await _set.FindAsync(id);
if (entity == null) return;

entity.IsDeleted = true;
entity.UpdatedOn = DateTime.UtcNow;
}

public async Task RestoreAsync(TId id)
{
var entity = await _set.FindAsync(id);
if (entity == null) return;

entity.IsDeleted = false;
entity.UpdatedOn = DateTime.UtcNow;
}

public async Task<int> CountAsync(BaseSearchCriteria criteria)
{
IQueryable<TEntity> query = _set.AsQueryable();

if (criteria.IsDeleted.HasValue)
query = query.Where(e => e.IsDeleted == criteria.IsDeleted);

if (criteria.IsActive.HasValue)
query = query.Where(e => e.IsActive == criteria.IsActive);

query = ApplyCountCustomFilters(query, criteria);

return await query.CountAsync();
}
public virtual IQueryable<TEntity> ApplyCountCustomFilters(
IQueryable<TEntity> query,
BaseSearchCriteria criteria)
{
return query;
}
}
}
```

# GenericRepository.cs
```cs
﻿using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Domain.Common;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
public class GenericRepository<TEntity, TId> : IGenericRepository<TEntity, TId>
where TEntity : BaseEntity<TId>
{
protected readonly ApplicationDbContext _context;
protected readonly DbSet<TEntity> _set;

public GenericRepository(ApplicationDbContext context)
{
_context = context;
_set = context.Set<TEntity>();
}

public virtual async Task<TEntity?> GetByIdAsync(TId id)
{
return await _set.FirstOrDefaultAsync(e => e.Id!.Equals(id));
}

public virtual async Task AddAsync(TEntity entity)
{
entity.CreatedOn = DateTime.UtcNow;
await _set.AddAsync(entity);
}

public virtual void Update(TEntity entity)
{
entity.UpdatedOn = DateTime.UtcNow;
_set.Update(entity);
}

public virtual void Remove(TEntity entity)
{
entity.IsDeleted = true;
_set.Update(entity);
}
}
}
```

# ReadRepository.cs
```cs
﻿using Azure.Core;
using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Application.Common.Extensions;
using BackEnd.Application.Common.SearchCriteria;
using BackEnd.Domain.Common;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
public class ReadRepository<TEntity, TEntitySC, TId> : IReadRepository<TEntity, TEntitySC, TId>
where TEntity : BaseEntity<TId>
where TEntitySC : BaseSearchCriteria
{
private readonly ApplicationDbContext _context;

public ReadRepository(ApplicationDbContext context)
{
_context = context;
}

public virtual IQueryable<TEntity> GetAllAsync()
{
return _context.Set<TEntity>().AsNoTracking();
}
public virtual Task<TEntity?> GetByIdAsync(TId id)
{
return _context.Set<TEntity>()
.AsNoTracking()
.FirstOrDefaultAsync(x => x.Id!.Equals(id));
}

public virtual IQueryable<TEntity> GetAllBySearchCriteria(
IQueryable<TEntity> query,
TEntitySC criteria)
{
var pageIndex = criteria.PageIndex ?? 0;
var pageSize = criteria.PageSize ?? 10;

if (pageSize > criteria.MaxPageSize)
pageSize = criteria.MaxPageSize;

query = query.WhereIf(criteria.IsDeleted.HasValue,e => e.IsDeleted == criteria.IsDeleted);

query = ApplyCustomFilters(query, criteria);

int skip = (pageIndex - 1) * pageSize;
query = query.Skip(skip).Take(pageSize);

return query;
}
public virtual IQueryable<TEntity> ApplyCustomFilters(
IQueryable<TEntity> query,
TEntitySC criteria)
{
return query;
}
}
}
```

# FileService.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace BackEnd.Infrastructure.Services
{
public class FileService : IFileService
{
private readonly IWebHostEnvironment _webHostEnvironment;

public FileService(IWebHostEnvironment webHostEnvironment)
{
_webHostEnvironment = webHostEnvironment;
}

public async Task<Result<string>> UploadFileAsync(IFormFile file, string targetFolder, string expectedType)
{
if (file == null || file.Length == 0)
return Result<string>.Failure("File is empty.", ErrorType.BadRequest);

var extValidation = ValidateExtension(file, expectedType);
if (!extValidation.IsSuccess)
return extValidation;

var mimeValidation = ValidateMimeType(file, expectedType);
if (!mimeValidation.IsSuccess)
return mimeValidation;

var sizeValidation = ValidateFileSize(file, 10 * 1024 * 1024); // 10MB
if (!sizeValidation.IsSuccess)
return Result<string>.Failure(sizeValidation.Message, ErrorType.BadRequest);

string webRoot = _webHostEnvironment.WebRootPath;
if (string.IsNullOrWhiteSpace(webRoot))
{
webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
}

string uploadsFolder = Path.Combine(webRoot, "Uploads", targetFolder);

if (!Directory.Exists(uploadsFolder))
Directory.CreateDirectory(uploadsFolder);

string uniqueFileName = $"{Guid.NewGuid()}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
string filePath = Path.Combine(uploadsFolder, uniqueFileName);

using (var stream = new FileStream(filePath, FileMode.Create))
{
await file.CopyToAsync(stream);
}

return Result<string>.Success($"/Uploads/{targetFolder}/{uniqueFileName}");
}

public async Task<Result<List<string>>> UploadMultipleFilesAsync(IFormFileCollection files, string targetFolder, string expectedType)
{
if (files == null || files.Count == 0)
return Result<List<string>>.Failure("No files were provided.", ErrorType.BadRequest);

var uploadedPaths = new List<string>();
string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", targetFolder);

if (!Directory.Exists(uploadsFolder))
Directory.CreateDirectory(uploadsFolder);

foreach (var file in files)
{
var extValidation = ValidateExtension(file, expectedType);
if (!extValidation.IsSuccess)
return Result<List<string>>.Failure($"File '{file.FileName}' failed: {extValidation.Message}", ErrorType.BadRequest);

var mimeValidation = ValidateMimeType(file, expectedType);
if (!mimeValidation.IsSuccess)
return Result<List<string>>.Failure($"File '{file.FileName}' failed: {mimeValidation.Message}", ErrorType.BadRequest);

var sizeValidation = ValidateFileSize(file, 10 * 1024 * 1024);
if (!sizeValidation.IsSuccess)
return Result<List<string>>.Failure($"File '{file.FileName}' failed: {sizeValidation.Message}", ErrorType.BadRequest);

string uniqueFileName = $"{Guid.NewGuid()}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
string filePath = Path.Combine(uploadsFolder, uniqueFileName);

using (var stream = new FileStream(filePath, FileMode.Create))
{
await file.CopyToAsync(stream);
}

uploadedPaths.Add($"/Uploads/{targetFolder}/{uniqueFileName}");
}

return Result<List<string>>.Success(uploadedPaths);
}

public Result<bool> DeleteFile(string relativePath)
{
try
{
string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

if (File.Exists(fullPath))
{
File.Delete(fullPath);
return Result<bool>.Success(true);
}

return Result<bool>.Failure("File not found.", ErrorType.NotFound);
}
catch (Exception ex)
{
return Result<bool>.Failure($"Error deleting file: {ex.Message}", ErrorType.InternalServerError);
}
}

public async Task<string> CalculateFileHashAsync(IFormFile file)
{
using (var md5 = MD5.Create())
using (var stream = file.OpenReadStream())
{
var hash = await md5.ComputeHashAsync(stream);
return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
}
}

private Result<string> ValidateExtension(IFormFile file, string expectedType)
{
var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
var allowedVideoExtensions = new[] { ".mp4", ".avi", ".mov" };
var fileExtension = Path.GetExtension(file.FileName).ToLower();

if (expectedType == "image" && !allowedImageExtensions.Contains(fileExtension))
return Result<string>.Failure("Invalid file type. Only JPG, JPEG, PNG, GIF, or WEBP are allowed.", ErrorType.BadRequest);

if (expectedType == "video" && !allowedVideoExtensions.Contains(fileExtension))
return Result<string>.Failure("Invalid file type. Only MP4, AVI, or MOV are allowed.", ErrorType.BadRequest);

return Result<string>.Success("Extension is valid.");
}

private Result<string> ValidateMimeType(IFormFile file, string expectedType)
{
var allowedImageMimeTypes = new[] { "image/jpeg", "image/png", "image/jpg", "image/gif", "image/webp" };
var allowedVideoMimeTypes = new[] { "video/mp4", "video/avi", "video/quicktime" };
var mimeType = file.ContentType.ToLowerInvariant();

if (expectedType == "image" && !allowedImageMimeTypes.Contains(mimeType))
return Result<string>.Failure("Invalid MIME type. Only image files are allowed.", ErrorType.BadRequest);

if (expectedType == "video" && !allowedVideoMimeTypes.Contains(mimeType))
return Result<string>.Failure("Invalid MIME type. Only video files are allowed.", ErrorType.BadRequest);

return Result<string>.Success("MIME type is valid.");
}

public Result<bool> ValidateFileSize(IFormFile file, long maxSizeInBytes)
{
if (file == null)
return Result<bool>.Failure("File is null.", ErrorType.BadRequest);

if (file.Length > maxSizeInBytes)
return Result<bool>.Failure($"File size exceeds {maxSizeInBytes / 1024 / 1024} MB limit.", ErrorType.BadRequest);

return Result<bool>.Success(true);
}
}
}
```

