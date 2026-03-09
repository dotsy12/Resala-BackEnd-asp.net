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
﻿using BackEnd.Domain.Common;

namespace BackEnd.Domain.Entities.Notification
{
public sealed class DeliveryArea : BaseEntity<int>
{
public string Name { get; private set; } = null!;

private DeliveryArea() { }

public static DeliveryArea Create(string name)
{
if (string.IsNullOrWhiteSpace(name))
throw new ArgumentException("Area name is required.", nameof(name));

return new DeliveryArea
{
Name = name.Trim(),
IsActive = true,
CreatedOn = DateTime.UtcNow
};
}

public void Rename(string newName)
{
if (string.IsNullOrWhiteSpace(newName))
throw new ArgumentException("Name is required.");
Name = newName.Trim();
UpdatedOn = DateTime.UtcNow;
}
}
}
```

# Notification.cs
```cs
﻿using BackEnd.Domain.Common;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;

namespace BackEnd.Domain.Entities.Notification
{
public sealed class Notification : BaseEntity<int>
{
public int DonorId { get; private set; }
public NotificationType Type { get; private set; }
public string Title { get; private set; } = null!;
public string Message { get; private set; } = null!;
public bool IsRead { get; private set; } = false;
public DateTime? ReadAt { get; private set; }
public int? RelatedEntityId { get; private set; }

public Donor? Donor { get; private set; }

private Notification() { }

public static Notification Create(
int donorId, NotificationType type,
string title, string message,
int? relatedEntityId = null)
{
return new Notification
{
DonorId = donorId,
Type = type,
Title = title.Trim(),
Message = message.Trim(),
IsRead = false,
RelatedEntityId = relatedEntityId,
CreatedOn = DateTime.UtcNow
};
}

public void MarkAsRead()
{
IsRead = true;
ReadAt = DateTime.UtcNow;
UpdatedOn = DateTime.UtcNow;
}
}
}
```

# GeneralDonation.cs
```cs
﻿using BackEnd.Domain.Common;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using BackEnd.Domain.Interfaces;
using BackEnd.Domain.ValueObjects;

namespace BackEnd.Domain.Entities.Payment
{
public sealed class GeneralDonation : BaseEntity<int>, IAggregateRoot
{
public int DonorId { get; private set; }
public Money Amount { get; private set; } = null!;
public DonationType DonationType { get; private set; }
public string? Note { get; private set; }

public Donor? Donor { get; private set; }

private GeneralDonation() { }

public static GeneralDonation Create(
int donorId, decimal amount,
DonationType donationType, string? note = null)
{
if (amount <= 0)
throw new Exceptions.InvalidMoneyAmountException(amount);

return new GeneralDonation
{
DonorId = donorId,
Amount = new Money(amount),
DonationType = donationType,
Note = note?.Trim(),
CreatedOn = DateTime.UtcNow
};
}
}
}
```

# InKindDonation.cs
```cs
﻿using BackEnd.Domain.Common;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Exceptions;

namespace BackEnd.Domain.Entities.Payment
{
public sealed class InKindDonation : BaseEntity<int>
{
public int DonorId { get; private set; }
public string DonationTypeName { get; private set; } = null!;
public int Quantity { get; private set; }
public string? Description { get; private set; }
public int RecordedByStaffId { get; private set; }
public DateTime RecordedAt { get; private set; }

public Donor? Donor { get; private set; }
public StaffMember? RecordedBy { get; private set; }

private InKindDonation() { }

public static InKindDonation Create(
int donorId, string donationTypeName,
int quantity, string? description, int recordedByStaffId)
{
if (quantity <= 0)
throw new ArgumentException("Quantity must be > 0.", nameof(quantity));
if (string.IsNullOrWhiteSpace(donationTypeName))
throw new ArgumentException("Donation type name is required.");

return new InKindDonation
{
DonorId = donorId,
DonationTypeName = donationTypeName.Trim(),
Quantity = quantity,
Description = description?.Trim(),
RecordedByStaffId = recordedByStaffId,
RecordedAt = DateTime.UtcNow,
CreatedOn = DateTime.UtcNow
};
}
}
}
```

# PaymentRequest.cs
```cs
﻿using BackEnd.Domain.Common;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using BackEnd.Domain.Events;
using BackEnd.Domain.Exceptions;
using BackEnd.Domain.Interfaces;
using BackEnd.Domain.ValueObjects;

namespace BackEnd.Domain.Entities.Payment
{
public sealed class PaymentRequest : BaseEntity<int>, IAggregateRoot
{
public int? SubscriptionId { get; private set; }
public int? GeneralDonationId { get; private set; }
public Money Amount { get; private set; } = null!;
public PaymentMethod Method { get; private set; }
public PaymentStatus Status { get; private set; }
public string? ReceiptImagePath { get; private set; }
public BranchPaymentDetails? BranchDetails { get; private set; }
public RepresentativeDetails? RepresentativeInfo { get; private set; }
public int? VerifiedByStaffId { get; private set; }
public DateTime? VerifiedAt { get; private set; }
public string? RejectionReason { get; private set; }

private PaymentRequest() { }

public static PaymentRequest CreateElectronic(
int? subscriptionId, int? generalDonationId,
Money amount, PaymentMethod method, string receiptImagePath)
{
ValidateReference(subscriptionId, generalDonationId);
if (method is not (PaymentMethod.VodafoneCash or PaymentMethod.InstaPay))
throw new InvalidPaymentRequestException(
"Use this factory for VodafoneCash or InstaPay only.");
if (string.IsNullOrWhiteSpace(receiptImagePath))
throw new InvalidPaymentRequestException(
"Receipt image is required.");

return new PaymentRequest
{
SubscriptionId = subscriptionId,
GeneralDonationId = generalDonationId,
Amount = amount,
Method = method,
Status = PaymentStatus.Pending,
ReceiptImagePath = receiptImagePath,
CreatedOn = DateTime.UtcNow
};
}

public static PaymentRequest CreateBranch(
int? subscriptionId, int? generalDonationId,
Money amount, BranchPaymentDetails branchDetails)
{
ValidateReference(subscriptionId, generalDonationId);
return new PaymentRequest
{
SubscriptionId = subscriptionId,
GeneralDonationId = generalDonationId,
Amount = amount,
Method = PaymentMethod.Branch,
Status = PaymentStatus.Pending,
BranchDetails = branchDetails
?? throw new ArgumentNullException(nameof(branchDetails)),
CreatedOn = DateTime.UtcNow
};
}

public static PaymentRequest CreateRepresentative(
int? subscriptionId, int? generalDonationId,
Money amount, RepresentativeDetails repDetails)
{
ValidateReference(subscriptionId, generalDonationId);
return new PaymentRequest
{
SubscriptionId = subscriptionId,
GeneralDonationId = generalDonationId,
Amount = amount,
Method = PaymentMethod.Representative,
Status = PaymentStatus.Pending,
RepresentativeInfo = repDetails
?? throw new ArgumentNullException(nameof(repDetails)),
CreatedOn = DateTime.UtcNow
};
}

public void Verify(int staffId)
{
if (Status != PaymentStatus.Pending)
throw new InvalidPaymentRequestException(
$"Cannot verify payment with status '{Status}'.");

Status = PaymentStatus.Verified;
VerifiedByStaffId = staffId;
VerifiedAt = DateTime.UtcNow;
UpdatedOn = DateTime.UtcNow;

AddDomainEvent(new PaymentVerifiedEvent(
Id, SubscriptionId, GeneralDonationId,
Amount.Amount, staffId));
}

public void Reject(int staffId, string reason)
{
if (Status != PaymentStatus.Pending)
throw new InvalidPaymentRequestException(
$"Cannot reject payment with status '{Status}'.");
if (string.IsNullOrWhiteSpace(reason))
throw new ArgumentException("Rejection reason is required.");

Status = PaymentStatus.Rejected;
VerifiedByStaffId = staffId;
RejectionReason = reason.Trim();
UpdatedOn = DateTime.UtcNow;
}

public void Cancel()
{
if (Status != PaymentStatus.Pending)
throw new InvalidPaymentRequestException(
"Only pending payments can be cancelled.");
Status = PaymentStatus.Cancelled;
UpdatedOn = DateTime.UtcNow;
}

private static void ValidateReference(
int? subscriptionId, int? generalDonationId)
{
if (subscriptionId.HasValue == generalDonationId.HasValue)
throw new InvalidPaymentRequestException(
"Link to subscription XOR generalDonation — not both or neither.");
}
}
}
```

# Sponsorship.cs
```cs
﻿using BackEnd.Domain.Common;
using BackEnd.Domain.Enums;
using BackEnd.Domain.Events;
using BackEnd.Domain.Exceptions;
using BackEnd.Domain.Interfaces;
using BackEnd.Domain.ValueObjects;

namespace BackEnd.Domain.Entities.Sponsorship
{
public sealed class Sponsorship : BaseEntity<int>, IAggregateRoot
{
public string Name { get; private set; } = null!;
public string Description { get; private set; } = null!;
public string? ImagePath { get; private set; }
public string? IconPath { get; private set; }
public string Category { get; private set; } = null!;
public SponsorshipStatus Status { get; private set; }
public UrgencyLevel UrgencyLevel { get; private set; }
public Money? FinancialGoal { get; private set; }
public Money TotalCollected { get; private set; } = null!;
public SponsorshipPolicy Policy { get; private set; } = null!;

private Sponsorship() { }

public static Sponsorship Create(
string name, string description, string category,
Money? financialGoal = null, SponsorshipPolicy? policy = null)
{
if (string.IsNullOrWhiteSpace(name))
throw new ArgumentException("Name is required.", nameof(name));
if (string.IsNullOrWhiteSpace(description))
throw new ArgumentException("Description is required.", nameof(description));

return new Sponsorship
{
Name = name.Trim(),
Description = description.Trim(),
Category = category.Trim(),
Status = SponsorshipStatus.Active,
UrgencyLevel = UrgencyLevel.Normal,
FinancialGoal = financialGoal,
TotalCollected = Money.Zero(),
Policy = policy ?? SponsorshipPolicy.Default,
CreatedOn = DateTime.UtcNow
};
}

public void Activate() { Status = SponsorshipStatus.Active; UpdatedOn = DateTime.UtcNow; }
public void Deactivate() { Status = SponsorshipStatus.Inactive; UpdatedOn = DateTime.UtcNow; }

public void SetUrgencyLevel(UrgencyLevel level)
{
if (UrgencyLevel == level) return;
UrgencyLevel = level;
UpdatedOn = DateTime.UtcNow;
AddDomainEvent(new SponsorshipUrgencyChangedEvent(Id, level));
}

public void UpdatePolicy(SponsorshipPolicy policy)
{
Policy = policy ?? throw new ArgumentNullException(nameof(policy));
UpdatedOn = DateTime.UtcNow;
}

public void AddToTotalCollected(Money amount)
{
TotalCollected = TotalCollected.Add(amount);
UpdatedOn = DateTime.UtcNow;
}

public void UpdateImages(string? imagePath, string? iconPath)
{
ImagePath = imagePath;
IconPath = iconPath;
UpdatedOn = DateTime.UtcNow;
}

public bool IsActive => Status == SponsorshipStatus.Active;
}
}
```

# SponsorshipSubscription.cs
```cs
﻿using BackEnd.Domain.Common;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using BackEnd.Domain.Events;
using BackEnd.Domain.Exceptions;
using BackEnd.Domain.ValueObjects;

namespace BackEnd.Domain.Entities.Sponsorship
{
public sealed class SponsorshipSubscription : BaseEntity<int>
{
public int DonorId { get; private set; }
public int SponsorshipId { get; private set; }
public Money Amount { get; private set; } = null!;
public PaymentCycle PaymentCycle { get; private set; }
public SubscriptionStatus Status { get; private set; }
public DateTime StartDate { get; private set; }
public DateTime NextPaymentDate { get; private set; }
public DateTime? CancelledAt { get; private set; }
public string? CancelReason { get; private set; }

public Donor? Donor { get; private set; }
public Sponsorship? Sponsorship { get; private set; }

private SponsorshipSubscription() { }

public static SponsorshipSubscription Create(
int donorId, int sponsorshipId,
Sponsorship sponsorship,
Money amount, PaymentCycle cycle)
{
if (!sponsorship.IsActive)
throw new SponsorshipNotActiveException(sponsorshipId);
if (amount.Amount <= 0)
throw new InvalidMoneyAmountException(amount.Amount);

var startDate = DateTime.UtcNow;
var sub = new SponsorshipSubscription
{
DonorId = donorId,
SponsorshipId = sponsorshipId,
Amount = amount,
PaymentCycle = cycle,
Status = SubscriptionStatus.Active,
StartDate = startDate,
NextPaymentDate = startDate.AddMonths((int)cycle),
CreatedOn = startDate
};

sub.AddDomainEvent(
new SubscriptionCreatedEvent(0, donorId, sponsorshipId));
return sub;
}

public void Cancel(string? reason = null)
{
if (Status == SubscriptionStatus.Cancelled)
throw new InvalidSubscriptionOperationException(
"Subscription is already cancelled.");

Status = SubscriptionStatus.Cancelled;
CancelledAt = DateTime.UtcNow;
CancelReason = reason?.Trim();
UpdatedOn = DateTime.UtcNow;

AddDomainEvent(new SubscriptionCancelledEvent(Id, DonorId, reason));
}

public void Suspend()
{
if (Status != SubscriptionStatus.Active)
throw new InvalidSubscriptionOperationException(
"Only active subscriptions can be suspended.");

Status = SubscriptionStatus.Suspended;
UpdatedOn = DateTime.UtcNow;
AddDomainEvent(new LatePaymentDetectedEvent(Id, DonorId, NextPaymentDate));
}

public void AdvancePaymentDate()
{
NextPaymentDate = NextPaymentDate.AddMonths((int)PaymentCycle);
if (Status == SubscriptionStatus.Suspended)
Status = SubscriptionStatus.Active;
UpdatedOn = DateTime.UtcNow;
}

public bool IsLate(int gracePeriodDays) =>
Status == SubscriptionStatus.Active &&
DateTime.UtcNow > NextPaymentDate.AddDays(gracePeriodDays);
}
}
```

# Enums.cs
```cs
﻿// Domain/Enums/Enums.cs
namespace BackEnd.Domain.Enums
{
public enum UserRole { Admin = 1, Reception = 2, Donor = 3 }
public enum StaffType { Admin = 1, Reception = 2 }
public enum AccountStatus { Pending = 1, Active = 2, Locked = 3 }

public enum SponsorshipStatus { Active = 1, Inactive = 2 }
public enum UrgencyLevel { Normal = 1, Urgent = 2, Critical = 3 }

public enum PaymentCycle
{
Monthly = 1,
Quarterly = 3,
SemiAnnual = 6
}

public enum SubscriptionStatus
{
Active = 1,
Cancelled = 2,
Suspended = 3,
Expired = 4
}

public enum PaymentMethod
{
VodafoneCash = 1,
InstaPay = 2,
Branch = 3,
Representative = 4
}

public enum PaymentStatus { Pending = 1, Verified = 2, Rejected = 3, Cancelled = 4 }
public enum DonationType { General = 1, Emergency = 2 }

public enum NotificationType
{
PaymentReminder = 1,
LatePayment = 2,
PaymentVerified = 3,
Congratulations = 4,
UrgentSponsorship = 5,
LowFunding = 6,
SubscriptionCreated = 7
}
}
```

# IAggregateRoot.cs
```cs
﻿using System;

namespace BackEnd.Domain.Interfaces
{
public interface IAggregateRoot
{
}
}
```

# IDomainEvent.cs
```cs
﻿using System;

namespace BackEnd.Domain.Interfaces
{
public interface IDomainEvent
{
}
}
```

# IEntity.cs
```cs
﻿using System;

namespace BackEnd.Domain.Interfaces
{
public interface IEntity
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

var adminUserId = "admin-user-id";

builder.Entity<IdentityRole>().HasData(
new IdentityRole
{
Id = adminRoleId,
Name = "Admin",
NormalizedName = "ADMIN",
ConcurrencyStamp = "admin-role-concurrency"
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

