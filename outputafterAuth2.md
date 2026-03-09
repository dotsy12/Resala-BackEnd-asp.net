## Compact Export
---

# BackEnd.Application\Abstractions\Messaging\ICommand.cs
```cs
﻿using MediatR;

namespace BackEnd.Application.Abstractions.Messaging
{
    public interface ICommand<out TResponse> : IRequest<TResponse> { }
}
```

# BackEnd.Application\Abstractions\Messaging\IQuery.cs
```cs
﻿using MediatR;

namespace BackEnd.Application.Abstractions.Messaging
{
    public interface IQuery<out TResponse> : IRequest<TResponse>
    {
    }
}
```

# BackEnd.Application\Abstractions\Persistence\IEntityStateRepository.cs
```cs
﻿using BackEnd.Application.Common.SearchCriteria;
using BackEnd.Domain.Common;

namespace BackEnd.Application.Abstractions.Persistence
{
    namespace BackEnd.Application.Abstractions.Persistence
    {
        public interface IEntityStateRepository<TEntity, TId>
            where TEntity : BaseEntity<TId>
        {
            Task ActivateAsync(TId id);
            Task DeactivateAsync(TId id);
            Task SoftDeleteAsync(TId id);
            Task RestoreAsync(TId id);
            Task<int> CountAsync(BaseSearchCriteria criteria);
            IQueryable<TEntity> ApplyCountCustomFilters(
                   IQueryable<TEntity> query,
                   BaseSearchCriteria criteria);
        }
    }

}
```

# BackEnd.Application\Abstractions\Persistence\IGenericRepository.cs
```cs
﻿using BackEnd.Domain.Common;

namespace BackEnd.Application.Abstractions.Persistence
{
    public interface IGenericRepository<TEntity, TId>
        where TEntity : BaseEntity<TId>
    {
        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Remove(TEntity entity);
    }
}
```

# BackEnd.Application\Abstractions\Persistence\IReadRepository.cs
```cs
﻿using BackEnd.Application.Common.SearchCriteria;
using BackEnd.Domain.Common;
using System.Security.Cryptography;

namespace BackEnd.Application.Abstractions.Persistence
{
    public interface IReadRepository<TEntity, TEntitySC, TId>
        where TEntity : BaseEntity<TId>
        where TEntitySC : BaseSearchCriteria
    {
        IQueryable<TEntity> GetAllAsync();
        Task<TEntity?> GetByIdAsync(TId id);
        IQueryable<TEntity> GetAllBySearchCriteria(
            IQueryable<TEntity> query,
            TEntitySC criteria);
        IQueryable<TEntity> ApplyCustomFilters(
                IQueryable<TEntity> query,
                TEntitySC criteria);
    }
}
```

# BackEnd.Application\Abstractions\Persistence\IUnitOfWork.cs
```cs
﻿using BackEnd.Application.Interfaces.Services;

namespace BackEnd.Application.Abstractions.Persistence
{
    public interface IUnitOfWork
    {

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
```

# BackEnd.Application\Abstractions\Queries\IEntityQuery.cs
```cs
﻿using System;

namespace BackEnd.Application.Abstractions.Queries
{
    public interface IEntityQuery<TEntity>
    {
        IQueryable<TEntity> Apply(IQueryable<TEntity> query);
    }
}
```

# BackEnd.Application\Abstractions\Queries\PagedResult.cs
```cs
﻿using System;

namespace BackEnd.Application.Abstractions.Queries
{
    public sealed class PagedResult<T>
    {
        public PagedResult(IReadOnlyList<T> dtoList, int total, int pageIndex, int pageSize)
        {
            Items = dtoList;
            TotalRows = total;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        public int TotalRows { get; init; }
        public int PageSize { get; init; }
        public int PageIndex { get; init; }
        public IReadOnlyList<T> Items { get; init; } = [];
    }
}
```

# BackEnd.Application\Behaviors\ValidationBehavior.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using FluentValidation;
using MediatR;

namespace BackEnd.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        private readonly IValidator<TRequest>? _validator;

        public ValidationBehavior(IValidator<TRequest>? validator = null)
        {
            _validator = validator;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (_validator != null)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));

                    var responseType = typeof(TResponse);
                    if (responseType.IsGenericType &&
                        responseType.GetGenericTypeDefinition() == typeof(Result<>))
                    {
                        var innerType = responseType.GetGenericArguments()[0];
                        var failureMethod = typeof(Result<>)
                            .MakeGenericType(innerType)
                            .GetMethod("Failure", new[] { typeof(string), typeof(ErrorType) });

                        if (failureMethod != null)
                        {
                            var failureResult = failureMethod.Invoke(null, new object[] { errors, ErrorType.BadRequest });
                            return (TResponse)failureResult!;
                        }
                    }

                    throw new InvalidOperationException($"Validation failed for {typeof(TRequest).Name} but could not construct a valid Result<T> response.");
                }
            }

            return await next();
        }
    }
}
```

# BackEnd.Application\Common\ApplicationRoles.cs
```cs
﻿using System;

namespace BackEnd.Application.Common
{
    public class ApplicationRoles
    {
        public const string Admin = "Admin";
        public const string Employee = "Employee";
    }
}
```

# BackEnd.Application\Common\Extensions\QueryableExtensions.cs
```cs
﻿using System;
using System.Linq.Expressions;

namespace BackEnd.Application.Common.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> WhereIf<T>(
            this IQueryable<T> query,
            bool condition,
            Expression<Func<T, bool>> predicate)
        {
            return condition ? query.Where(predicate) : query;
        }
    }
}
```

# BackEnd.Application\Common\ResponseFormat\ApiResponse.cs
```cs
﻿using System;
using System.Net;

namespace BackEnd.Application.Common.ResponseFormat
{
    public class ApiResponse<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public object Meta { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(T data, string message = null)
        {
            Succeeded = true;
            Data = data;
            Message = message;
            StatusCode = HttpStatusCode.OK;
        }

        public ApiResponse(string message, bool succeeded = false)
        {
            Succeeded = succeeded;
            Message = message;
            StatusCode = succeeded ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
        }

        public static ApiResponse<T> Success(T data, HttpStatusCode statusCode = HttpStatusCode.OK, string? message = null)
            => new ApiResponse<T> { Data = data, StatusCode = statusCode, Message = message, Succeeded = true };

        public static ApiResponse<T> Fail(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, List<string> errors = null) =>
            new ApiResponse<T>(message, false) { StatusCode = statusCode, Errors = errors, Succeeded = false };
    }

}
```

# BackEnd.Application\Common\ResponseFormat\Result.cs
```cs
﻿namespace BackEnd.Application.Common.ResponseFormat
{
    public interface IResult
    {
        bool IsSuccess { get; }
        string Message { get; }
        ErrorType ErrorType { get; }
        object Value { get; }
    }

    public class Result<T> : IResult
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string Message { get; }
        public ErrorType ErrorType { get; }
        public Dictionary<string, string[]>? Errors { get; }  // ✅ جديد

        object IResult.Value => Value!;

        private Result(T value, string? message = null)
        {
            IsSuccess = true;
            Value = value;
            Message = message ?? "Operation completed successfully.";
            ErrorType = ErrorType.None;
        }

        private Result(string message, ErrorType errorType)
        {
            IsSuccess = false;
            Value = default!;
            Message = message;
            ErrorType = errorType;
        }

        private Result(string message, ErrorType errorType,
            Dictionary<string, string[]>? errors)
        {
            IsSuccess = false;
            Value = default!;
            Message = message;
            ErrorType = errorType;
            Errors = errors;
        }

        public static Result<T> Success(T value, string? message = null)
            => new(value, message);

        public static Result<T> Failure(string message, ErrorType errorType)
            => new(message, errorType);

        public static Result<T> Failure(
            string message,
            ErrorType errorType,
            Dictionary<string, string[]>? errors)
            => new(message, errorType, errors);
    }

    public enum ErrorType
    {
        None,
        NotFound,
        BadRequest,
        Conflict,
        UnprocessableEntity,
        InternalServerError,
        Unauthorized,
        Forbidden
    }
}
```

# BackEnd.Application\Common\SearchCriteria\BaseSearchCriteria.cs
```cs
﻿using System;

namespace BackEnd.Application.Common.SearchCriteria
{
    public abstract class BaseSearchCriteria
    {
        public string? Search { get; set; }
        public bool? IsDeleted { get; set; } = false;

        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }

        public bool? IsActive { get; set; }

        public int MaxPageSize = 10000;
    }

}
```

# BackEnd.Application\Common\SearchCriteria\ProductSearchCriteria.cs
```cs
﻿using System;

namespace BackEnd.Application.Common.SearchCriteria
{
    public class ProductSearchCriteria : BaseSearchCriteria
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Name { get; set; }
        public string? UserId { get; set; }
        public bool? OnlyActive { get; set; } = true;
    }

}
```

# BackEnd.Application\Dependencies\ApplicationModule.cs
```cs
﻿using BackEnd.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BackEnd.Application.ALLApplicationDependencies
{
    public static class ApplicationModule
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(assembly);
            });

            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(assembly);
            });

            services.AddValidatorsFromAssembly(assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
```

# BackEnd.Application\Features\Auth\Commands\CreateStaffCommand.cs.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Domain.Enums;
using MediatR;

namespace BackEnd.Application.Features.Auth.Commands
{
    public record CreateStaffCommand(
        string Name,
        string Username,
        string Email,
        string PhoneNumber,
        string Password,
        StaffType StaffType
    ) : IRequest<Result<CreateStaffResponse>>;

    public record CreateStaffResponse(int StaffId, string Username);
}
```

# BackEnd.Application\Features\Auth\Commands\CreateStaffHandler.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Auth.Commands
{
    public class CreateStaffHandler
        : IRequestHandler<CreateStaffCommand, Result<CreateStaffResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepo;
        private readonly IStaffRepository _staffRepo;

        public CreateStaffHandler(
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepo,
            IStaffRepository staffRepo)
        {
            _userManager = userManager;
            _userRepo = userRepo;
            _staffRepo = staffRepo;
        }

        public async Task<Result<CreateStaffResponse>> Handle(
            CreateStaffCommand request, CancellationToken ct)
        {
            var usernameExists = await _userRepo.GetByUsernameAsync(request.Username, ct);
            if (usernameExists is not null)
                return Result<CreateStaffResponse>.Failure(
                    "Username already taken.", ErrorType.Conflict);

            var nameParts = request.Name.Trim().Split(' ', 2);
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email.ToLowerInvariant().Trim(),
                PhoneNumber = request.PhoneNumber,
                FirstName = nameParts[0],
                LastName = nameParts.Length > 1 ? nameParts[1] : "",
                IsActive = true,
                EmailConfirmed = true,
                CreatedOn = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors
                    .ToDictionary(e => e.Code, e => new[] { e.Description });
                return Result<CreateStaffResponse>.Failure(
                    "Failed to create staff account.", ErrorType.BadRequest, errors);
            }

            var roleName = request.StaffType == StaffType.Admin ? "Admin" : "Reception";
            await _userManager.AddToRoleAsync(user, roleName);

            var staff = StaffMember.Create(
                userId: user.Id,
                firstName: nameParts[0],
                lastName: nameParts.Length > 1 ? nameParts[1] : "",
                username: request.Username,
                email: request.Email,
                phone: request.PhoneNumber,
                staffType: request.StaffType
            );

            await _staffRepo.AddAsync(staff, ct);
            await _staffRepo.SaveChangesAsync(ct);

            return Result<CreateStaffResponse>.Success(
                new CreateStaffResponse(staff.Id, staff.Username));
        }
    }
}
```

# BackEnd.Application\Features\Auth\Commands\ForgotPasswordCommand.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Auth.Commands
{
    public record ForgotPasswordCommand(string Email)
        : IRequest<Result<string>>;
}
```

# BackEnd.Application\Features\Auth\Commands\ForgotPasswordHandler.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Auth.Commands
{
    public class ForgotPasswordHandler
        : IRequestHandler<ForgotPasswordCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;

        public ForgotPasswordHandler(
            UserManager<ApplicationUser> userManager,
            IOtpService otpService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _otpService = otpService;
            _emailService = emailService;
        }

        public async Task<Result<string>> Handle(
            ForgotPasswordCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Result<string>.Failure(
                    "User with this email does not exist.", ErrorType.NotFound);

            var otpCode = _otpService.GenerateOtp();
            await _otpService.SaveOtpAsync(user.Email!, otpCode, "PasswordReset", ct);
            await _emailService.SendOtpEmailAsync(user.Email!, otpCode, "PasswordReset", ct);

            return Result<string>.Success("An OTP has been sent to your email address.");
        }
    }
}
```

# BackEnd.Application\Features\Auth\Commands\LoginCommand.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Auth.Commands
{
    public record LoginCommand(
        string? PhoneNumber,
        string? Username,
        string Password
    ) : IRequest<Result<LoginResponse>>;

    public record LoginResponse(
        string Token,
        string Role,
        int UserId,
        string Name,
        string? PhoneNumber
    );

}
```

# BackEnd.Application\Features\Auth\Commands\LoginHandler.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.Auth.Commands;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository _userRepo;
    private readonly IDonorRepository _donorRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IJwtService _jwtService;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        IUserRepository userRepo,
        IDonorRepository donorRepo,
        IStaffRepository staffRepo,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _userRepo = userRepo;
        _donorRepo = donorRepo;
        _staffRepo = staffRepo;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request, CancellationToken ct)
    {
        ApplicationUser? user = null;

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            user = await _userRepo.GetByPhoneAsync(request.PhoneNumber, ct);
        else if (!string.IsNullOrWhiteSpace(request.Username))
            user = await _userRepo.GetByUsernameAsync(request.Username, ct);

        if (user is null)
            return Result<LoginResponse>.Failure(
                "Invalid credentials.", ErrorType.Unauthorized);

        var passwordOk = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordOk)
            return Result<LoginResponse>.Failure(
                "Invalid credentials.", ErrorType.Unauthorized);

        if (!user.IsActive)
            return Result<LoginResponse>.Failure(
                "Account is not active. Please verify your email.",
                ErrorType.Unauthorized);

        var role = await _userRepo.GetRoleAsync(user, ct) ?? "Donor";

        int? donorId = null, staffId = null;

        if (role == "Donor")
        {
            donorId = await _donorRepo.GetIdByUserIdAsync(user.Id, ct);
        }
        else
        {
            staffId = await _staffRepo.GetIdByUserIdAsync(user.Id, ct);

            if (staffId.HasValue)
            {
                var status = await _staffRepo.GetStatusByIdAsync(staffId.Value, ct);

                if (status == AccountStatus.Locked)
                    return Result<LoginResponse>.Failure(
                        "Account is locked. Contact admin.", ErrorType.Forbidden);

                if (status == AccountStatus.Pending)
                    return Result<LoginResponse>.Failure(
                        "Account is pending approval.", ErrorType.Forbidden);
            }
        }

        var token = _jwtService.GenerateToken(user, role, donorId, staffId);

        return Result<LoginResponse>.Success(new LoginResponse(
            Token: token,
            Role: role,
            UserId: donorId ?? staffId ?? 0,
            Name: $"{user.FirstName} {user.LastName}".Trim(),
            PhoneNumber: user.PhoneNumber
        ), "Login successful.");
    }
}
```

# BackEnd.Application\Features\Auth\Commands\RegisterDonorByStaffCommand.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Auth.Commands
{
    public record RegisterDonorByStaffCommand(
        string Name,
        string Email,
        string PhoneNumber,
        string Password,
        string? Job = null,
        string? Landline = null
    ) : IRequest<Result<RegisterDonorResponse>>;
}
```

# BackEnd.Application\Features\Auth\Commands\RegisterDonorByStaffHandler.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Auth.Commands
{
    public class RegisterDonorByStaffHandler
        : IRequestHandler<RegisterDonorByStaffCommand, Result<RegisterDonorResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepo;
        private readonly IDonorRepository _donorRepo;

        public RegisterDonorByStaffHandler(
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepo,
            IDonorRepository donorRepo)
        {
            _userManager = userManager;
            _userRepo = userRepo;
            _donorRepo = donorRepo;
        }

        public async Task<Result<RegisterDonorResponse>> Handle(
            RegisterDonorByStaffCommand request, CancellationToken ct)
        {
            if (await _userRepo.PhoneExistsAsync(request.PhoneNumber, ct))
                return Result<RegisterDonorResponse>.Failure(
                    "Phone number already registered.", ErrorType.Conflict);

            var nameParts = request.Name.Trim().Split(' ', 2);
            var user = new ApplicationUser
            {
                UserName = request.PhoneNumber,
                Email = request.Email.ToLowerInvariant().Trim(),
                PhoneNumber = request.PhoneNumber,
                FirstName = nameParts[0],
                LastName = nameParts.Length > 1 ? nameParts[1] : "",
                IsActive = true,          // الـ Staff يفعّله مباشرة
                EmailConfirmed = true,
                CreatedOn = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors
                    .ToDictionary(e => e.Code, e => new[] { e.Description });
                return Result<RegisterDonorResponse>.Failure(
                    "Registration failed.", ErrorType.BadRequest, errors);
            }

            await _userManager.AddToRoleAsync(user, "Donor");

            var donor = Donor.Create(
                userId: user.Id,
                firstName: nameParts[0],
                lastName: nameParts.Length > 1 ? nameParts[1] : "",
                email: request.Email,
                phoneNumber: request.PhoneNumber,
                job: request.Job,
                landline: request.Landline
            );

            await _donorRepo.AddAsync(donor, ct);
            await _donorRepo.SaveChangesAsync(ct);

            return Result<RegisterDonorResponse>.Success(
                new RegisterDonorResponse(donor.Id, "Donor registered by staff."));
        }
    }
}
```

# BackEnd.Application\Features\Auth\Commands\RegisterDonorCommand.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Auth.Commands
{
    public record RegisterDonorCommand(
        string Name,
        string Email,
        string PhoneNumber,
        string Password,
        string? Job = null,
        string? Landline = null
    ) : IRequest<Result<RegisterDonorResponse>>;

    public record RegisterDonorResponse(int UserId, string Message);

}
```

# BackEnd.Application\Features\Auth\Commands\RegisterDonorHandler.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.Auth.Commands;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

public class RegisterDonorHandler
     : IRequestHandler<RegisterDonorCommand, Result<RegisterDonorResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDonorRepository _donorRepo;
    private readonly IUserRepository _userRepo;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public RegisterDonorHandler(
        UserManager<ApplicationUser> userManager,
        IDonorRepository donorRepo,
        IUserRepository userRepo,
        IOtpService otpService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _donorRepo = donorRepo;
        _userRepo = userRepo;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<Result<RegisterDonorResponse>> Handle(
        RegisterDonorCommand request, CancellationToken ct)
    {
        if (await _userRepo.PhoneExistsAsync(request.PhoneNumber, ct))
            return Result<RegisterDonorResponse>.Failure(
                "Phone number already registered.", ErrorType.Conflict);

        if (await _userRepo.EmailExistsAsync(request.Email, ct))
            return Result<RegisterDonorResponse>.Failure(
                "Email already registered.", ErrorType.Conflict);

        var nameParts = request.Name.Trim().Split(' ', 2);
        var user = new ApplicationUser
        {
            UserName = request.PhoneNumber,
            Email = request.Email.ToLowerInvariant().Trim(),
            PhoneNumber = request.PhoneNumber,
            FirstName = nameParts[0],
            LastName = nameParts.Length > 1 ? nameParts[1] : "",
            IsActive = false,
            EmailConfirmed = false,
            CreatedOn = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors
                .ToDictionary(e => e.Code, e => new[] { e.Description });
            return Result<RegisterDonorResponse>.Failure(
                "Registration failed.", ErrorType.BadRequest, errors);
        }

        await _userManager.AddToRoleAsync(user, "Donor");

        var donor = Donor.Create(
            userId: user.Id,
            firstName: nameParts[0],
            lastName: nameParts.Length > 1 ? nameParts[1] : "",
            email: request.Email,
            phoneNumber: request.PhoneNumber,
            job: request.Job,
            landline: request.Landline
        );

        await _donorRepo.AddAsync(donor, ct);
        await _donorRepo.SaveChangesAsync(ct);

        var otpCode = _otpService.GenerateOtp();
        await _otpService.SaveOtpAsync(user.Email!, otpCode, "EmailVerification", ct);
        await _emailService.SendOtpEmailAsync(user.Email!, otpCode, "EmailVerification", ct);

        return Result<RegisterDonorResponse>.Success(
            new RegisterDonorResponse(donor.Id,
                "Registration successful. Check your email for OTP."));
    }
}
```

# BackEnd.Application\Features\Auth\Commands\ResetPasswordCommand.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Auth.Commands
{
    public record ResetPasswordCommand(
        string Email,
        string Otp,
        string NewPassword
    ) : IRequest<Result<string>>;
}
```

# BackEnd.Application\Features\Auth\Commands\ResetPasswordHandler.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Auth.Commands
{
    public class ResetPasswordHandler
        : IRequestHandler<ResetPasswordCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpService _otpService;

        public ResetPasswordHandler(
            UserManager<ApplicationUser> userManager,
            IOtpService otpService)
        {
            _userManager = userManager;
            _otpService = otpService;
        }

        public async Task<Result<string>> Handle(
            ResetPasswordCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Result<string>.Failure("User not found.", ErrorType.NotFound);

            var isValid = await _otpService.ValidateOtpAsync(
                request.Email, request.Otp, "PasswordReset", ct);

            if (!isValid)
                return Result<string>.Failure(
                    "Invalid or expired OTP.", ErrorType.BadRequest);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(
                user, resetToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .ToDictionary(e => e.Code, e => new[] { e.Description });
                return Result<string>.Failure(
                    "Password reset failed.", ErrorType.BadRequest, errors);
            }

            return Result<string>.Success(
                "Password has been reset successfully. You can now log in.");
        }
    }
}
```

# BackEnd.Application\Features\Auth\Commands\VerifyEmailCommand.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.Auth.Commands
{
    public record VerifyEmailCommand(string Email, string Otp)
        : IRequest<Result<string>>;
}
```

# BackEnd.Application\Features\Auth\Commands\VerifyEmailHandler.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.Auth.Commands
{
    public class VerifyEmailHandler
        : IRequestHandler<VerifyEmailCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpService _otpService;

        public VerifyEmailHandler(
            UserManager<ApplicationUser> userManager,
            IOtpService otpService)
        {
            _userManager = userManager;
            _otpService = otpService;
        }

        public async Task<Result<string>> Handle(
            VerifyEmailCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Result<string>.Failure("User not found.", ErrorType.NotFound);

            if (user.EmailConfirmed)
                return Result<string>.Failure("Email already verified.", ErrorType.BadRequest);

            var isValid = await _otpService.ValidateOtpAsync(
                request.Email, request.Otp, "EmailVerification", ct);

            if (!isValid)
                return Result<string>.Failure("Invalid or expired OTP.", ErrorType.BadRequest);

            user.EmailConfirmed = true;
            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return Result<string>.Success("Email verified successfully. You can now log in.");
        }
    }
}
```

# BackEnd.Application\Features\LoginAndTokens\LoginUser\Command\Handler\UserLogInHandler.cs
```cs
﻿using AutoMapper;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.AuthenticationFeatures.LoginAndTokens.LoginUser.Command.Model;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity.AuthenticationHepler;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Application.Features.AuthenticationFeatures.LoginAndTokens.LoginUser.Command.Handler
{
    public class UserLogInHandler : IRequestHandler<UserLogInCommand, Result<ResponseAuthModel>>
    {
        private readonly IIdentityService _identityServies;
        private readonly IValidator<UserLogInCommand> _validator;
        private readonly IMapper _mapper;

        public UserLogInHandler(
            IValidator<UserLogInCommand> validator,
            IMapper mapper,
            IIdentityService identityServies)
        {
            _validator = validator;
            _mapper = mapper;
            _identityServies = identityServies;
        }
        public async Task<Result<ResponseAuthModel>> Handle(UserLogInCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if(!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return Result<ResponseAuthModel>.Failure(string.Join(" | ", errors), ErrorType.BadRequest);
            }
            var userData = await _identityServies.GetUserByEmailAsync(request.Email);
            var user = userData.Value;
            if (user is null)
                return Result<ResponseAuthModel>.Failure("Invalid credentials", ErrorType.Unauthorized);


            var isPasswordValid = await _identityServies.IsPasswordExist(user, request.Password, cancellationToken);

            if (!isPasswordValid.Value)
                return Result<ResponseAuthModel>.Failure("Invalid credentials", ErrorType.Unauthorized);

            var response = await _identityServies.GenerateRefreshTokenAsync(user, request.RememberMe, cancellationToken);

            return Result<ResponseAuthModel>.Success(response.Value);
        }
    }
}
```

# BackEnd.Application\Features\LoginAndTokens\LoginUser\Command\Model\UserLogInCommand.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Domain.Entities.Identity.AuthenticationHepler;
using MediatR;

namespace BackEnd.Application.Features.AuthenticationFeatures.LoginAndTokens.LoginUser.Command.Model
{
    public class UserLogInCommand : IRequest<Result<ResponseAuthModel>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
```

# BackEnd.Application\Features\LoginAndTokens\LoginUser\Command\Validator\UserLogInValidator.cs
```cs
﻿using BackEnd.Application.Features.AuthenticationFeatures.LoginAndTokens.LoginUser.Command.Model;
using FluentValidation;

namespace BackEnd.Application.Features.AuthenticationFeatures.LoginAndTokens.LoginUser.Command.Validator
{
    public class UserLogInValidator : AbstractValidator<UserLogInCommand>
    {
        public UserLogInValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is Required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is Required.");

        }
    }
}
```

# BackEnd.Application\Features\LoginAndTokens\Logout\Command\UserLogoutCommand.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.AuthenticationFeatures.LoginAndTokens.Logout.Command
{
    public class UserLogoutCommand : IRequest<Result<bool>>
    {

    }
}
```

# BackEnd.Application\Features\LoginAndTokens\Logout\Handler\UserLogoutHandler.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.AuthenticationFeatures.LoginAndTokens.Logout.Command;
using BackEnd.Application.Interfaces.Services;
using MediatR;

namespace BackEnd.Application.Features.AuthenticationFeatures.LoginAndTokens.Logout.Handler
{
    public class UserLogoutHandler : IRequestHandler<UserLogoutCommand, Result<bool>>
    {
        private readonly IIdentityService _identityServies;
        public UserLogoutHandler(IIdentityService identityServies)
        {
            _identityServies = identityServies;
        }
        public async Task<Result<bool>> Handle(UserLogoutCommand request, CancellationToken cancellationToken)
        {
            var result = await _identityServies.RevokeRefreshTokenFromCookiesAsync();
            if (!result.Value)
                return Result<bool>.Failure("Logout failed", ErrorType.InternalServerError);

            return Result<bool>.Success(true, "Logout Success");
        }
    }
}
```

# BackEnd.Application\Features\LoginAndTokens\RefreshToken\Handler\RefreshTokenHandler.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.AuthenticationFeatures.LoginAndTokens.RefreshToken.Model;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity.AuthenticationHepler;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BackEnd.Application.Features.AuthenticationFeatures.LoginAndTokens.RefreshToken.Handler
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<ResponseAuthModel>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityService _identityServies;
        public RefreshTokenHandler(
            IHttpContextAccessor httpContextAccessor ,
             IIdentityService identityServies)
        {
            _httpContextAccessor = httpContextAccessor;
            _identityServies = identityServies;
        }

        public async Task<Result<ResponseAuthModel>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["RefreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Result<ResponseAuthModel>.Failure("No refresh token provided." , ErrorType.NotFound);

            var result = await _identityServies.RefreshTokenAsunc(refreshToken);

            if (!string.IsNullOrEmpty(result.Message))
                return Result<ResponseAuthModel>.Failure(result.Message, ErrorType.NotFound);

            return Result<ResponseAuthModel>.Success(result.Value);

        }
    }
}
```

# BackEnd.Application\Features\LoginAndTokens\RefreshToken\Model\RefreshTokenCommand.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Domain.Entities.Identity.AuthenticationHepler;
using MediatR;

namespace BackEnd.Application.Features.AuthenticationFeatures.LoginAndTokens.RefreshToken.Model
{
    public class RefreshTokenCommand : IRequest<Result<ResponseAuthModel>>
    {

    }
}
```

# BackEnd.Application\Features\Password\RestPassword\Command\RestPasswordCommand.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace BackEnd.Application.Features.AuthenticationFeatures.Password.RestPassword.Command
{
    public class ResetPasswordCommand : IRequest<Result<string>>
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }

    }

}
```

# BackEnd.Application\Features\Password\RestPassword\Command\SendTokenToRestPasswordCommand.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using MediatR;

namespace ECommerce.Application.Features.AuthenticationFeatures.Password.RestPassword.Command
{
    public class SendTokenToRestPasswordCommand : IRequest<Result<ResetPasswordTokenResponse>>
    {
        public string Email { get; set; }
    }
    public class ResetPasswordTokenResponse
    {
        public string Token { get; set; }
        public string UserId { get; set; }
    }

}
```

# BackEnd.Application\Features\Password\RestPassword\Handler\RestPasswordHandler.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.AuthenticationFeatures.Password.RestPassword.Command;
using BackEnd.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.WebUtilities;

namespace BackEnd.Application.Features.AuthenticationFeatures.Password.RestPassword.Handler
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Result<string>>
    {
        private readonly IIdentityService _identityServies;

        public ResetPasswordHandler(IIdentityService identityServies)
        {
            _identityServies = identityServies;
        }

        public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var userData = await _identityServies.IsUserExist(request.UserId);
            var user = userData.Value;
            if (user == null)
                return Result<string>.Failure("Invalid user ID.", ErrorType.NotFound);

            var tokenBytes = WebEncoders.Base64UrlDecode(request.Token);
            var decodedToken = Encoding.UTF8.GetString(tokenBytes);

            var result = await _identityServies.ResetPasswordAsync(user, decodedToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
                return Result<string>.Failure(errors, ErrorType.BadRequest);
            }

            return Result<string>.Success("Password reset successfully!");
        }
    }

}
```

# BackEnd.Application\Features\Password\RestPassword\Handler\SendTokenToRestPasswordHandler.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using ECommerce.Application.Features.AuthenticationFeatures.Password.RestPassword.Command;
using MediatR;

namespace ECommerce.Application.Features.AuthenticationFeatures.Password.RestPassword.Handler
{
    public class SendTokenToRestPasswordHandler : IRequestHandler<SendTokenToRestPasswordCommand, Result<ResetPasswordTokenResponse>>
    {
        private readonly IIdentityService _identityServies;

        public SendTokenToRestPasswordHandler(IIdentityService identityServies)
        {
            _identityServies = identityServies;
        }

        public async Task<Result<ResetPasswordTokenResponse>> Handle(SendTokenToRestPasswordCommand request, CancellationToken cancellationToken)
        {
            var userData = await _identityServies.GetUserByEmailAsync(request.Email);
            var user = userData.Value;
            if (user == null)
                return Result<ResetPasswordTokenResponse>.Failure("Can't Send Rest Password Token to this email", ErrorType.BadRequest);

            var restPasswordToken = await _identityServies.GetRestPasswordTokenAsync(user);

            var ResetPasswordResponse = new ResetPasswordTokenResponse
            {
                Token = restPasswordToken,
                UserId = user.Id.ToString()
            };
            return Result<ResetPasswordTokenResponse>.Success(ResetPasswordResponse,"Rest Password Token");
        }
    }
}
```

# BackEnd.Application\Features\Password\RestPassword\Validator\ResetPasswordCommandValidator .cs
```cs
﻿using BackEnd.Application.Features.AuthenticationFeatures.Password.RestPassword.Command;
using FluentValidation;

namespace BackEnd.Application.Features.AuthenticationFeatures.Password.RestPassword.Validator
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.");

            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
        }
    }
}
```

# BackEnd.Application\Features\RegisterUser\Commands\Dtos\RegisterUserDto.cs
```cs
﻿using System;

namespace BackEnd.Application.Features.RegisterUser.Commands.Dtos
{
    public class RegisterUserDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
    }
}
```

# BackEnd.Application\Features\RegisterUser\Commands\Handler\AdminRegisterHandler.cs
```cs
﻿using AutoMapper;
using BackEnd.Application.Common;
using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.Login.Commands.Models;
using BackEnd.Application.Features.RegisterUser.Commands.Dtos;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using FluentValidation;
using MediatR;

namespace BackEnd.Application.Features.Login.Commands.Handler
{























}
```

# BackEnd.Application\Features\RegisterUser\Commands\Models\AdminRegisterCommand.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.RegisterUser.Commands.Dtos;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Application.Features.Login.Commands.Models
{
    public class EmployeeRegisterCommand : IRequest<Result<RegisterUserDto>>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
    public class AdminRegisterCommand : IRequest<Result<RegisterUserDto>>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

}
```

# BackEnd.Application\Features\RegisterUser\Commands\Validator\AdminRegisterValidator.cs
```cs
﻿using BackEnd.Application.Features.Login.Commands.Models;
using FluentValidation;

namespace BackEnd.Application.Features.Login.Commands.Validator
{
    public class AdminRegisterValidator : AbstractValidator<AdminRegisterCommand>
    {
        public AdminRegisterValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Full name is Requird.")
                .MinimumLength(2).WithMessage("Full Name must be at least 2 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Full name is Requird.")
                .MinimumLength(2).WithMessage("Full Name must be at least 2 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .Matches(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$")
                .WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }
    public class EmployeeRegisterValidator : AbstractValidator<EmployeeRegisterCommand>
    {
        public EmployeeRegisterValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Full name is Requird.")
                .MinimumLength(2).WithMessage("Full Name must be at least 2 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Full name is Requird.")
                .MinimumLength(2).WithMessage("Full Name must be at least 2 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .Matches(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$")
                .WithMessage("Invalid email format.");

            RuleFor(x => x.BirthDate)
                .NotEmpty()
                .LessThan(DateTime.Today)
                .Must(birthDate =>
                {
                    var today = DateTime.Today;
                    var age = today.Year - birthDate.Year;

                    if (birthDate.Date > today.AddYears(-age))
                        age--;

                    return age >= 18 && age <= 60;
                })
                .WithMessage("Employee age must be between 18 and 60 years");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }
}
```

# BackEnd.Application\Interfaces\Repositories\IDonorRepository.cs
```cs
﻿using BackEnd.Domain.Entities.Identity;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IDonorRepository
    {
        Task AddAsync(Donor donor, CancellationToken ct = default);
        Task<int?> GetIdByUserIdAsync(string userId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
```

# BackEnd.Application\Interfaces\Repositories\IStaffRepository.cs
```cs
﻿using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IStaffRepository
    {
        Task AddAsync(StaffMember staff, CancellationToken ct = default);
        Task<int?> GetIdByUserIdAsync(string userId, CancellationToken ct = default);
        Task<AccountStatus?> GetStatusByIdAsync(int staffId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
```

# BackEnd.Application\Interfaces\Repositories\IUserRepository.cs
```cs
﻿using BackEnd.Domain.Entities.Identity;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByPhoneAsync(string phone, CancellationToken ct = default);
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
        Task<bool> PhoneExistsAsync(string phone, CancellationToken ct = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
        Task<string?> GetRoleAsync(ApplicationUser user, CancellationToken ct = default);
    }
}
```

# BackEnd.Application\Interfaces\Services\IEmailService.cs
```cs
﻿namespace BackEnd.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode, string purpose,
            CancellationToken ct = default);
    }
}
```

# BackEnd.Application\Interfaces\Services\IFileService.cs
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

# BackEnd.Application\Interfaces\Services\IIdentityServies.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Entities.Identity.AuthenticationHepler;
using Microsoft.AspNetCore.Identity;
using System.Threading;

namespace BackEnd.Application.Interfaces.Services
{
    public interface IIdentityService
    {
        Task<Result<ResponseAuthModel>> RefreshTokenAsunc(string token);

        Task<Result<ResponseAuthModel>> GenerateRefreshTokenAsync(ApplicationUser user, bool rememberMe, CancellationToken cancellationToken = default);

        Task<Result<bool>> RevokeRefreshTokenFromCookiesAsync();

        Task<Result<bool>> IsInRole(string userId, string role);

        Task<Result<ApplicationUser>> IsUserExist(string userId);

        Task<Result<ApplicationUser?>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<Result<bool>> IsEmailExist(string email, CancellationToken cancellationToken = default);

        Task<Result<bool>> IsPasswordExist(ApplicationUser user, string Password, CancellationToken cancellationToken = default);
        Task<Result<IdentityResult>> CreateUserAsync(ApplicationUser user, string password,string Role, CancellationToken cancellationToken = default);

        Task<Result<string>> CreateJwtToken(ApplicationUser user, CancellationToken cancellationToken = default);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
        Task<string> GetRestPasswordTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default);
        string? GetUserId();

    }
}
```

# BackEnd.Application\Interfaces\Services\IJwtService.cs
```cs
﻿using BackEnd.Domain.Entities.Identity;

namespace BackEnd.Application.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user, string role, int? donorId, int? staffId);
    }
}
```

# BackEnd.Application\Interfaces\Services\IOtpService.cs
```cs
﻿namespace BackEnd.Application.Interfaces.Services
{
    public interface IOtpService
    {
        string GenerateOtp();
        Task SaveOtpAsync(string email, string code, string purpose,
            CancellationToken ct = default);
        Task<bool> ValidateOtpAsync(string email, string code, string purpose,
            CancellationToken ct = default);
    }
}
```

# BackEnd.Application\Mappings\DomainMapping.cs
```cs
﻿using AutoMapper;
using BackEnd.Domain.Entities;

namespace BackEnd.Application.Mappings
{
    public class DomainMapping : Profile
    {
        public DomainMapping()
        {

        }
    }
}
```

# BackEnd.Application\Mappings\SupUserRegisterMappingProfile.cs
```cs
﻿using AutoMapper;
using BackEnd.Application.Features.Login.Commands.Models;
using BackEnd.Domain.Entities.Identity;

namespace BackEnd.Application.Mappings
{
    public class VolunteerRegisterMappingProfile : Profile
    {
        public VolunteerRegisterMappingProfile()
        {
            CreateMap<AdminRegisterCommand, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            CreateMap<EmployeeRegisterCommand, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        }
    }
}
```

# BackEnd.Domain\BaseEntity\BaseEntity.cs
```cs
﻿using BackEnd.Domain.Interfaces;

namespace BackEnd.Domain.Common
{
    public abstract class BaseEntity<TId> : IEntity
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public TId Id { get; set; } = default!;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public IReadOnlyList<IDomainEvent> DomainEvents =>
            _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent)
            => _domainEvents.Add(domainEvent);

        public void ClearDomainEvents()
            => _domainEvents.Clear();
    }
}
```

# BackEnd.Domain\Entities\Identity\ApplicationUser.cs
```cs
﻿// Domain/Entities/Identity/ApplicationUser.cs
using BackEnd.Domain.Entities.Identity.AuthenticationHepler;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Domain.Entities.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfileImagePath { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
        public ICollection<RefreshToken> refreshTokens { get; set; } = new List<RefreshToken>();
    }
}
```

# BackEnd.Domain\Entities\Identity\AuthenticationHepler\JwtSittings.cs
```cs
﻿using System;

namespace BackEnd.Domain.Entities.Identity.AuthenticationHepler
{
    public class JwtSittings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public double DurationInHours { get; set; }
    }
}
```

# BackEnd.Domain\Entities\Identity\AuthenticationHepler\RefreshToken.cs
```cs
﻿using Microsoft.EntityFrameworkCore;

namespace BackEnd.Domain.Entities.Identity.AuthenticationHepler
{
    [Owned] // Add Automatic id the table owned to Application user table
    public class RefreshToken
    {
        public string token { get; set; }
        public DateTime ExpireOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? RevokedOn { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpireOn;
        public bool IsActive => RevokedOn == null && !IsExpired;
    }
}
```

# BackEnd.Domain\Entities\Identity\AuthenticationHepler\ResponseAuthModel.cs
```cs
﻿using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace BackEnd.Domain.Entities.Identity.AuthenticationHepler
{
    public class ResponseAuthModel
    {
        public string Message { get; set; }
        public string Token { get; set; }
        public List<string> Roles { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
        [JsonIgnore]
        public CookieOptions CookieOptions { get; set; } // New property for cookie settings
    }
}
```

# BackEnd.Domain\Entities\Identity\Donor.cs
```cs
﻿// Domain/Entities/Identity/Donor.cs
using BackEnd.Domain.Common;
using BackEnd.Domain.Events;
using BackEnd.Domain.Interfaces;
using BackEnd.Domain.ValueObjects;

namespace BackEnd.Domain.Entities.Identity
{
    public sealed class Donor : BaseEntity<int>, IAggregateRoot
    {
        public string UserId { get; private set; } = null!;
        public PersonName FullName { get; private set; } = null!;
        public EmailAddress Email { get; private set; } = null!;
        public PhoneNumber PhoneNumber { get; private set; } = null!;
        public string? Landline { get; private set; }
        public string? Job { get; private set; }

        public ApplicationUser? User { get; private set; }

        private Donor() { }

        public static Donor Create(
            string userId,
            string firstName, string lastName,
            string email, string phoneNumber,
            string? job = null, string? landline = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var donor = new Donor
            {
                UserId = userId,
                FullName = new PersonName(firstName, lastName),
                Email = new EmailAddress(email),
                PhoneNumber = new PhoneNumber(phoneNumber),
                Job = job?.Trim(),
                Landline = landline?.Trim(),
                CreatedOn = DateTime.UtcNow
            };

            donor.AddDomainEvent(new DonorRegisteredEvent(0, email));
            return donor;
        }

        public void UpdateProfile(
            string firstName, string lastName,
            string? job, string? landline)
        {
            FullName = new PersonName(firstName, lastName);
            Job = job?.Trim();
            Landline = landline?.Trim();
            UpdatedOn = DateTime.UtcNow;
        }
    }
}
```

# BackEnd.Domain\Entities\Identity\OtpRecord.cs
```cs
﻿using BackEnd.Domain.Common;

namespace BackEnd.Domain.Entities.Identity
{
    public sealed class OtpRecord : BaseEntity<int>
    {
        public string Email { get; private set; } = null!;
        public string Code { get; private set; } = null!;
        public string Purpose { get; private set; } = null!; // "EmailVerification" | "PasswordReset"
        public DateTime ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; } = false;

        private OtpRecord() { }

        public static OtpRecord Create(string email, string code, string purpose, int expiryMinutes = 10)
        {
            return new OtpRecord
            {
                Email = email.ToLowerInvariant().Trim(),
                Code = code,
                Purpose = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                IsUsed = false,
                CreatedOn = DateTime.UtcNow
            };
        }

        public bool IsValid() => !IsUsed && DateTime.UtcNow <= ExpiresAt;

        public void MarkAsUsed()
        {
            IsUsed = true;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}
```

# BackEnd.Domain\Entities\Identity\StaffMember.cs
```cs
﻿// Domain/Entities/Identity/StaffMember.cs
using BackEnd.Domain.Common;
using BackEnd.Domain.Enums;
using BackEnd.Domain.Interfaces;
using BackEnd.Domain.ValueObjects;

namespace BackEnd.Domain.Entities.Identity
{
    public sealed class StaffMember : BaseEntity<int>, IAggregateRoot
    {
        public string UserId { get; private set; } = null!;
        public PersonName FullName { get; private set; } = null!;
        public string Username { get; private set; } = null!;
        public EmailAddress Email { get; private set; } = null!;
        public PhoneNumber Phone { get; private set; } = null!;
        public StaffType StaffType { get; private set; }
        public AccountStatus AccountStatus { get; private set; }

        public ApplicationUser? User { get; private set; }

        private StaffMember() { }

        public static StaffMember Create(
            string userId,
            string firstName, string lastName,
            string username, string email, string phone,
            StaffType staffType)
        {
            return new StaffMember
            {
                UserId = userId,
                FullName = new PersonName(firstName, lastName),
                Username = username.Trim(),
                Email = new EmailAddress(email),
                Phone = new PhoneNumber(phone),
                StaffType = staffType,
                AccountStatus = AccountStatus.Active,
                CreatedOn = DateTime.UtcNow
            };
        }

        public void SetAccountStatus(AccountStatus status)
        {
            AccountStatus = status;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}
```

# BackEnd.Domain\Entities\Notification\DeliveryArea.cs
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

# BackEnd.Domain\Entities\Notification\Notification.cs
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

# BackEnd.Domain\Entities\Payment\GeneralDonation.cs
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

# BackEnd.Domain\Entities\Payment\InKindDonation.cs
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

# BackEnd.Domain\Entities\Payment\PaymentRequest.cs
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

# BackEnd.Domain\Entities\Sponsorship\Sponsorship.cs
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

# BackEnd.Domain\Entities\Sponsorship\SponsorshipSubscription.cs
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

# BackEnd.Domain\Enums\Enums.cs
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

# BackEnd.Domain\Events\DomainEvents.cs.cs
```cs
﻿// Domain/Events/DomainEvents.cs
using BackEnd.Domain.Enums;
using BackEnd.Domain.Interfaces;

namespace BackEnd.Domain.Events
{
    public sealed record DonorRegisteredEvent(
        int DonorId, string Email)
        : IDomainEvent
    { public DateTime OccurredOn { get; } = DateTime.UtcNow; }

    public sealed record PaymentVerifiedEvent(
        int PaymentRequestId,
        int? SubscriptionId,
        int? GeneralDonationId,
        decimal Amount,
        int VerifiedByStaffId)
        : IDomainEvent
    { public DateTime OccurredOn { get; } = DateTime.UtcNow; }

    public sealed record PaymentRejectedEvent(
        int PaymentRequestId, int DonorId, string Reason)
        : IDomainEvent
    { public DateTime OccurredOn { get; } = DateTime.UtcNow; }

    public sealed record SubscriptionCreatedEvent(
        int SubscriptionId, int DonorId, int SponsorshipId)
        : IDomainEvent
    { public DateTime OccurredOn { get; } = DateTime.UtcNow; }

    public sealed record SubscriptionCancelledEvent(
        int SubscriptionId, int DonorId, string? Reason)
        : IDomainEvent
    { public DateTime OccurredOn { get; } = DateTime.UtcNow; }

    public sealed record LatePaymentDetectedEvent(
        int SubscriptionId, int DonorId, DateTime DueDate)
        : IDomainEvent
    { public DateTime OccurredOn { get; } = DateTime.UtcNow; }

    public sealed record SponsorshipUrgencyChangedEvent(
        int SponsorshipId, UrgencyLevel NewLevel)
        : IDomainEvent
    { public DateTime OccurredOn { get; } = DateTime.UtcNow; }
}
```

# BackEnd.Domain\Exceptions\DomainException.cs
```cs
﻿// Domain/Exceptions/DomainException.cs
namespace BackEnd.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        public string Code { get; }
        protected DomainException(string message, string code)
            : base(message) => Code = code;
    }

    public sealed class InvalidMoneyAmountException : DomainException
    {
        public InvalidMoneyAmountException(decimal amount)
            : base($"Amount '{amount}' must be >= 0.", "INVALID_AMOUNT") { }
    }

    public sealed class InvalidEmailException : DomainException
    {
        public InvalidEmailException(string email)
            : base($"'{email}' is not a valid email.", "INVALID_EMAIL") { }
    }

    public sealed class InvalidPhoneNumberException : DomainException
    {
        public InvalidPhoneNumberException(string phone)
            : base($"'{phone}' is not a valid Egyptian phone number.", "INVALID_PHONE") { }
    }

    public sealed class SponsorshipNotActiveException : DomainException
    {
        public SponsorshipNotActiveException(int id)
            : base($"Sponsorship '{id}' is not active.", "SPONSORSHIP_INACTIVE") { }
    }

    public sealed class DuplicateSubscriptionException : DomainException
    {
        public DuplicateSubscriptionException(int donorId, int sponsorshipId)
            : base($"Donor '{donorId}' already subscribed to sponsorship '{sponsorshipId}'.",
                   "DUPLICATE_SUBSCRIPTION")
        { }
    }

    public sealed class InvalidPaymentRequestException : DomainException
    {
        public InvalidPaymentRequestException(string reason)
            : base($"Invalid payment request: {reason}", "INVALID_PAYMENT") { }
    }

    public sealed class InvalidSubscriptionOperationException : DomainException
    {
        public InvalidSubscriptionOperationException(string reason)
            : base($"Invalid subscription operation: {reason}", "INVALID_SUB_OP") { }
    }
}
```

# BackEnd.Domain\Interfaces\IAggregateRoot.cs
```cs
﻿using System;

namespace BackEnd.Domain.Interfaces
{
    public interface IAggregateRoot
    {
    }
}
```

# BackEnd.Domain\Interfaces\IDomainEvent.cs
```cs
﻿using System;

namespace BackEnd.Domain.Interfaces
{
    public interface IDomainEvent
    {
    }
}
```

# BackEnd.Domain\Interfaces\IEntity.cs
```cs
﻿using System;

namespace BackEnd.Domain.Interfaces
{
    public interface IEntity
    {
    }
}
```

# BackEnd.Domain\ValueObjects\BranchPaymentDetails.cs
```cs
﻿// Domain/ValueObjects/ValueObjects.cs
using BackEnd.Domain.Exceptions;

namespace BackEnd.Domain.ValueObjects
{
    public sealed class BranchPaymentDetails : IEquatable<BranchPaymentDetails>
    {
        public string DonorName { get; private set; } = null!;
        public string Address { get; private set; } = null!;
        public string ContactNumber { get; private set; } = null!;
        public DateTime ScheduledDate { get; private set; }

        private BranchPaymentDetails() { }

        public BranchPaymentDetails(
            string donorName, string address,
            string contactNumber, DateTime scheduledDate)
        {
            if (string.IsNullOrWhiteSpace(donorName))
                throw new ArgumentException("Donor name is required.");
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address is required.");
            if (scheduledDate.ToUniversalTime() <= DateTime.UtcNow)
                throw new InvalidPaymentRequestException(
                    "Scheduled date must be in the future.");

            DonorName = donorName.Trim();
            Address = address.Trim();
            ContactNumber = contactNumber.Trim();
            ScheduledDate = scheduledDate.ToUniversalTime();
        }

        public bool Equals(BranchPaymentDetails? other) =>
            other is not null &&
            DonorName == other.DonorName && ScheduledDate == other.ScheduledDate;

        public override bool Equals(object? obj) => Equals(obj as BranchPaymentDetails);
        public override int GetHashCode() =>
            HashCode.Combine(DonorName, ScheduledDate);
    }
}
```

# BackEnd.Domain\ValueObjects\EmailAddress.cs
```cs
﻿// Domain/ValueObjects/ValueObjects.cs
using BackEnd.Domain.Exceptions;

namespace BackEnd.Domain.ValueObjects
{
    public sealed class EmailAddress : IEquatable<EmailAddress>
    {
        private static readonly System.Text.RegularExpressions.Regex _regex =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                System.Text.RegularExpressions.RegexOptions.Compiled |
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        public string Value { get; private set; } = null!;

        private EmailAddress() { }

        public EmailAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email is required.");
            if (!_regex.IsMatch(value))
                throw new InvalidEmailException(value);

            Value = value.Trim().ToLowerInvariant();
        }

        public bool Equals(EmailAddress? other) =>
            other is not null &&
            string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj) => Equals(obj as EmailAddress);
        public override int GetHashCode() => Value.ToLower().GetHashCode();
        public override string ToString() => Value;
        public static implicit operator string(EmailAddress e) => e.Value;
    }
}
```

# BackEnd.Domain\ValueObjects\Money.cs
```cs
﻿// Domain/ValueObjects/ValueObjects.cs
using BackEnd.Domain.Exceptions;

namespace BackEnd.Domain.ValueObjects
{
    public sealed class Money : IEquatable<Money>
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = null!;

        private Money() { }   // EF Core

        public Money(decimal amount, string currency = "EGP")
        {
            if (amount < 0)
                throw new InvalidMoneyAmountException(amount);

            Amount = Math.Round(amount, 2);
            Currency = currency.ToUpperInvariant();
        }

        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Amount - other.Amount, Currency);
        }

        public static Money Zero(string currency = "EGP") => new(0, currency);

        private void EnsureSameCurrency(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException(
                    $"Cannot operate on {Currency} and {other.Currency}");
        }

        public bool Equals(Money? other) =>
            other is not null &&
            Amount == other.Amount && Currency == other.Currency;

        public override bool Equals(object? obj) => Equals(obj as Money);
        public override int GetHashCode() => HashCode.Combine(Amount, Currency);
        public override string ToString() => $"{Amount:F2} {Currency}";
    }
}
```

# BackEnd.Domain\ValueObjects\PersonName.cs
```cs
﻿// Domain/ValueObjects/ValueObjects.cs
namespace BackEnd.Domain.ValueObjects
{
    public sealed class PersonName : IEquatable<PersonName>
    {
        public string FirstName { get; private set; } = null!;
        public string LastName { get; private set; } = null!;
        public string FullName => $"{FirstName} {LastName}";

        private PersonName() { }

        public PersonName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required.");
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required.");

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
        }

        public bool Equals(PersonName? other) =>
            other is not null &&
            string.Equals(FirstName, other.FirstName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(LastName, other.LastName, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj) => Equals(obj as PersonName);
        public override int GetHashCode() =>
            HashCode.Combine(FirstName.ToLower(), LastName.ToLower());
        public override string ToString() => FullName;
    }
}
```

# BackEnd.Domain\ValueObjects\PhoneNumber.cs
```cs
﻿// Domain/ValueObjects/ValueObjects.cs
using BackEnd.Domain.Exceptions;

namespace BackEnd.Domain.ValueObjects
{
    public sealed class PhoneNumber : IEquatable<PhoneNumber>
    {
        private static readonly System.Text.RegularExpressions.Regex _regex =
            new(@"^(010|011|012|015)\d{8}$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        public string Value { get; private set; } = null!;

        private PhoneNumber() { }

        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number is required.");

            var cleaned = value.Trim().Replace("-", "").Replace(" ", "");
            if (!_regex.IsMatch(cleaned))
                throw new InvalidPhoneNumberException(value);

            Value = cleaned;
        }

        public bool Equals(PhoneNumber? other) =>
            other is not null && Value == other.Value;

        public override bool Equals(object? obj) => Equals(obj as PhoneNumber);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;
        public static implicit operator string(PhoneNumber p) => p.Value;
    }
}
```

# BackEnd.Domain\ValueObjects\RepresentativeDetails.cs
```cs
﻿// Domain/ValueObjects/ValueObjects.cs
namespace BackEnd.Domain.ValueObjects
{
    public sealed class RepresentativeDetails : IEquatable<RepresentativeDetails>
    {
        public int DeliveryAreaId { get; private set; }
        public string DeliveryAreaName { get; private set; } = null!;
        public string? Notes { get; private set; }

        private RepresentativeDetails() { }

        public RepresentativeDetails(
            int deliveryAreaId, string deliveryAreaName, string? notes = null)
        {
            if (deliveryAreaId <= 0)
                throw new ArgumentException("Delivery area is required.");
            if (string.IsNullOrWhiteSpace(deliveryAreaName))
                throw new ArgumentException("Delivery area name is required.");

            DeliveryAreaId = deliveryAreaId;
            DeliveryAreaName = deliveryAreaName.Trim();
            Notes = notes?.Trim();
        }

        public bool Equals(RepresentativeDetails? other) =>
            other is not null && DeliveryAreaId == other.DeliveryAreaId;

        public override bool Equals(object? obj) => Equals(obj as RepresentativeDetails);
        public override int GetHashCode() => DeliveryAreaId.GetHashCode();
    }
}
```

# BackEnd.Domain\ValueObjects\SponsorshipPolicy.cs
```cs
﻿// Domain/ValueObjects/ValueObjects.cs
namespace BackEnd.Domain.ValueObjects
{
    public sealed class SponsorshipPolicy : IEquatable<SponsorshipPolicy>
    {
        public int GracePeriodDays { get; private set; }
        public int MaxDelayDays { get; private set; }
        public int ReminderDaysBeforeDue { get; private set; }

        public static SponsorshipPolicy Default =>
            new(gracePeriodDays: 7, maxDelayDays: 30, reminderDaysBeforeDue: 3);

        private SponsorshipPolicy() { }

        public SponsorshipPolicy(
            int gracePeriodDays, int maxDelayDays, int reminderDaysBeforeDue)
        {
            if (gracePeriodDays < 0)
                throw new ArgumentException("Grace period cannot be negative.");
            if (maxDelayDays < gracePeriodDays)
                throw new ArgumentException("MaxDelayDays must be >= GracePeriodDays.");
            if (reminderDaysBeforeDue < 1)
                throw new ArgumentException("Reminder days must be >= 1.");

            GracePeriodDays = gracePeriodDays;
            MaxDelayDays = maxDelayDays;
            ReminderDaysBeforeDue = reminderDaysBeforeDue;
        }

        public bool Equals(SponsorshipPolicy? other) =>
            other is not null &&
            GracePeriodDays == other.GracePeriodDays &&
            MaxDelayDays == other.MaxDelayDays &&
            ReminderDaysBeforeDue == other.ReminderDaysBeforeDue;

        public override bool Equals(object? obj) => Equals(obj as SponsorshipPolicy);
        public override int GetHashCode() =>
            HashCode.Combine(GracePeriodDays, MaxDelayDays, ReminderDaysBeforeDue);
    }
}
```

# BackEnd.Infrastructure\DependencyInjection\AuthenticationModule.cs
```cs
﻿using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Entities.Identity.AuthenticationHepler;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BackEnd.Infrastructure.AllInfrastructureDependencies
{
    public static class AuthenticationModule
    {
        public static IServiceCollection AuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = new JwtSittings();
            configuration.GetSection("jwtSittings").Bind(jwtSettings);
            services.AddSingleton(jwtSettings);

            services.AddIdentity<ApplicationUser, IdentityRole>(
                option => {
                    option.Password.RequiredLength = 8;
                    option.Password.RequireDigit = true;
                    option.Password.RequireLowercase = false;
                    option.Password.RequireUppercase = false;
                    option.Password.RequireNonAlphanumeric = false;
                    option.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }

    }

}
```

# BackEnd.Infrastructure\DependencyInjection\InfrastructureModule.cs
```cs
﻿using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Infrastructure.Persistence;
using BackEnd.Infrastructure.Persistence.DbContext;
using BackEnd.Infrastructure.Persistence.Repositories;
using BackEnd.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BackEnd.Infrastructure.InfrastructureDependencies
{
    public static class InfrastructureDependencies
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IIdentityService, IdentityServies>();

            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
           services.AddScoped<IJwtService, JwtService>();
           services.AddScoped<IOtpService, OtpService>();
           services.AddScoped<IEmailService, EmailService>();

            services.AddScoped(
                    typeof(IGenericRepository<,>),
                    typeof(GenericRepository<,>)

                );
            return services;

        }
    }
}
```

# BackEnd.Infrastructure\Migrations\20260203145402_InitialCreate.Designer.cs
```cs
﻿// <auto-generated />
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260203145402_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BackEnd.Domain.Entities.Identity.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool?>("IsDelete")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.HasIndex("UserId");

                    b.ToTable("Products", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Identity.ApplicationUser", b =>
                {
                    b.OwnsMany("BackEnd.Domain.Entities.Identity.AuthenticationHepler.RefreshToken", "refreshTokens", b1 =>
                        {
                            b1.Property<string>("ApplicationUserId")
                                .HasColumnType("nvarchar(450)");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("Id"));

                            b1.Property<DateTime>("CreatedOn")
                                .HasColumnType("datetime2");

                            b1.Property<DateTime>("ExpireOn")
                                .HasColumnType("datetime2");

                            b1.Property<DateTime?>("RevokedOn")
                                .HasColumnType("datetime2");

                            b1.Property<string>("token")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("ApplicationUserId", "Id");

                            b1.ToTable("RefreshToken");

                            b1.WithOwner()
                                .HasForeignKey("ApplicationUserId");
                        });

                    b.Navigation("refreshTokens");
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Product", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
```

# BackEnd.Infrastructure\Migrations\20260203145402_InitialCreate.cs
```cs
﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpireOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => new { x.ApplicationUserId, x.Id });
                    table.ForeignKey(
                        name: "FK_RefreshToken_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UserId",
                table: "Products",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
```

# BackEnd.Infrastructure\Migrations\20260203153458_AddRolesToDateBaseAndAdmin.Designer.cs
```cs
﻿// <auto-generated />
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260203153458_AddRolesToDateBaseAndAdmin")]
    partial class AddRolesToDateBaseAndAdmin
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BackEnd.Domain.Entities.Identity.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool?>("IsDelete")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "admin-user-id",
                            AccessFailedCount = 0,
                            ConcurrencyStamp = "admin-user-concurrency",
                            CreatedOn = new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "admin@admin.com",
                            EmailConfirmed = true,
                            FirstName = "Admin",
                            IsActive = true,
                            LastName = "User",
                            LockoutEnabled = false,
                            NormalizedEmail = "ADMIN@ADMIN.COM",
                            NormalizedUserName = "ADMIN@ADMIN.COM",
                            PasswordHash = "AQAAAAIAAYagAAAAEDZffCZ1Jv0MApj6ocE4KMf3SPhwXC54xd93VsfFUTGo7wUq9IuNZL8SrGw7iMxqIg==",
                            PhoneNumberConfirmed = false,
                            SecurityStamp = "admin-user-security",
                            TwoFactorEnabled = false,
                            UserName = "admin@admin.com"
                        });
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.HasIndex("UserId");

                    b.ToTable("Products", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "admin-role-id",
                            ConcurrencyStamp = "admin-role-concurrency",
                            Name = "Admin",
                            NormalizedName = "ADMIN"
                        },
                        new
                        {
                            Id = "employee-role-id",
                            ConcurrencyStamp = "employee-role-concurrency",
                            Name = "Employee",
                            NormalizedName = "Employee"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);

                    b.HasData(
                        new
                        {
                            UserId = "admin-user-id",
                            RoleId = "admin-role-id"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Identity.ApplicationUser", b =>
                {
                    b.OwnsMany("BackEnd.Domain.Entities.Identity.AuthenticationHepler.RefreshToken", "refreshTokens", b1 =>
                        {
                            b1.Property<string>("ApplicationUserId")
                                .HasColumnType("nvarchar(450)");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("Id"));

                            b1.Property<DateTime>("CreatedOn")
                                .HasColumnType("datetime2");

                            b1.Property<DateTime>("ExpireOn")
                                .HasColumnType("datetime2");

                            b1.Property<DateTime?>("RevokedOn")
                                .HasColumnType("datetime2");

                            b1.Property<string>("token")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("ApplicationUserId", "Id");

                            b1.ToTable("RefreshToken");

                            b1.WithOwner()
                                .HasForeignKey("ApplicationUserId");
                        });

                    b.Navigation("refreshTokens");
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Product", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
```

# BackEnd.Infrastructure\Migrations\20260203153458_AddRolesToDateBaseAndAdmin.cs
```cs
﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackEnd.Infrastructure.Migrations
{
    public partial class AddRolesToDateBaseAndAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "admin-role-id", "admin-role-concurrency", "Admin", "ADMIN" },
                    { "employee-role-id", "employee-role-concurrency", "Employee", "Employee" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedOn", "Email", "EmailConfirmed", "FirstName", "IsActive", "IsDelete", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UpdatedOn", "UserName" },
                values: new object[] { "admin-user-id", 0, "admin-user-concurrency", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@admin.com", true, "Admin", true, null, "User", false, null, "ADMIN@ADMIN.COM", "ADMIN@ADMIN.COM", "AQAAAAIAAYagAAAAEDZffCZ1Jv0MApj6ocE4KMf3SPhwXC54xd93VsfFUTGo7wUq9IuNZL8SrGw7iMxqIg==", null, false, "admin-user-security", false, null, "admin@admin.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "admin-role-id", "admin-user-id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "employee-role-id");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "admin-role-id", "admin-user-id" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id");
        }
    }
}
```

# BackEnd.Infrastructure\Migrations\20260203203932_AddEmployeeTable.Designer.cs
```cs
﻿// <auto-generated />
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260203203932_AddEmployeeTable")]
    partial class AddEmployeeTable
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BackEnd.Domain.Entities.Identity.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool?>("IsDelete")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "admin-user-id",
                            AccessFailedCount = 0,
                            ConcurrencyStamp = "admin-user-concurrency",
                            CreatedOn = new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "admin@admin.com",
                            EmailConfirmed = true,
                            FirstName = "Admin",
                            IsActive = true,
                            LastName = "User",
                            LockoutEnabled = false,
                            NormalizedEmail = "ADMIN@ADMIN.COM",
                            NormalizedUserName = "ADMIN@ADMIN.COM",
                            PasswordHash = "AQAAAAIAAYagAAAAEDZffCZ1Jv0MApj6ocE4KMf3SPhwXC54xd93VsfFUTGo7wUq9IuNZL8SrGw7iMxqIg==",
                            PhoneNumberConfirmed = false,
                            SecurityStamp = "admin-user-security",
                            TwoFactorEnabled = false,
                            UserName = "admin@admin.com"
                        });
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Identity.Employee", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Employees", (string)null);
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.HasIndex("UserId");

                    b.ToTable("Products", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "admin-role-id",
                            ConcurrencyStamp = "admin-role-concurrency",
                            Name = "Admin",
                            NormalizedName = "ADMIN"
                        },
                        new
                        {
                            Id = "employee-role-id",
                            ConcurrencyStamp = "employee-role-concurrency",
                            Name = "Employee",
                            NormalizedName = "Employee"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);

                    b.HasData(
                        new
                        {
                            UserId = "admin-user-id",
                            RoleId = "admin-role-id"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Identity.ApplicationUser", b =>
                {
                    b.OwnsMany("BackEnd.Domain.Entities.Identity.AuthenticationHepler.RefreshToken", "refreshTokens", b1 =>
                        {
                            b1.Property<string>("ApplicationUserId")
                                .HasColumnType("nvarchar(450)");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("Id"));

                            b1.Property<DateTime>("CreatedOn")
                                .HasColumnType("datetime2");

                            b1.Property<DateTime>("ExpireOn")
                                .HasColumnType("datetime2");

                            b1.Property<DateTime?>("RevokedOn")
                                .HasColumnType("datetime2");

                            b1.Property<string>("token")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("ApplicationUserId", "Id");

                            b1.ToTable("RefreshToken");

                            b1.WithOwner()
                                .HasForeignKey("ApplicationUserId");
                        });

                    b.Navigation("refreshTokens");
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Identity.Employee", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", "ApplicationUser")
                        .WithOne("Employee")
                        .HasForeignKey("BackEnd.Domain.Entities.Identity.Employee", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ApplicationUser");
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Product", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Identity.ApplicationUser", b =>
                {
                    b.Navigation("Employee");
                });
#pragma warning restore 612, 618
        }
    }
}
```

# BackEnd.Infrastructure\Migrations\20260203203932_AddEmployeeTable.cs
```cs
﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    public partial class AddEmployeeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
```

# BackEnd.Infrastructure\Migrations\ApplicationDbContextModelSnapshot.cs
```cs
﻿// <auto-generated />
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BackEnd.Domain.Entities.Identity.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool?>("IsDelete")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "admin-user-id",
                            AccessFailedCount = 0,
                            ConcurrencyStamp = "admin-user-concurrency",
                            CreatedOn = new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "admin@admin.com",
                            EmailConfirmed = true,
                            FirstName = "Admin",
                            IsActive = true,
                            LastName = "User",
                            LockoutEnabled = false,
                            NormalizedEmail = "ADMIN@ADMIN.COM",
                            NormalizedUserName = "ADMIN@ADMIN.COM",
                            PasswordHash = "AQAAAAIAAYagAAAAEDZffCZ1Jv0MApj6ocE4KMf3SPhwXC54xd93VsfFUTGo7wUq9IuNZL8SrGw7iMxqIg==",
                            PhoneNumberConfirmed = false,
                            SecurityStamp = "admin-user-security",
                            TwoFactorEnabled = false,
                            UserName = "admin@admin.com"
                        });
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Identity.Employee", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Employees", (string)null);
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.HasIndex("UserId");

                    b.ToTable("Products", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "admin-role-id",
                            ConcurrencyStamp = "admin-role-concurrency",
                            Name = "Admin",
                            NormalizedName = "ADMIN"
                        },
                        new
                        {
                            Id = "employee-role-id",
                            ConcurrencyStamp = "employee-role-concurrency",
                            Name = "Employee",
                            NormalizedName = "Employee"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);

                    b.HasData(
                        new
                        {
                            UserId = "admin-user-id",
                            RoleId = "admin-role-id"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Identity.ApplicationUser", b =>
                {
                    b.OwnsMany("BackEnd.Domain.Entities.Identity.AuthenticationHepler.RefreshToken", "refreshTokens", b1 =>
                        {
                            b1.Property<string>("ApplicationUserId")
                                .HasColumnType("nvarchar(450)");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("Id"));

                            b1.Property<DateTime>("CreatedOn")
                                .HasColumnType("datetime2");

                            b1.Property<DateTime>("ExpireOn")
                                .HasColumnType("datetime2");

                            b1.Property<DateTime?>("RevokedOn")
                                .HasColumnType("datetime2");

                            b1.Property<string>("token")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("ApplicationUserId", "Id");

                            b1.ToTable("RefreshToken");

                            b1.WithOwner()
                                .HasForeignKey("ApplicationUserId");
                        });

                    b.Navigation("refreshTokens");
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Identity.Employee", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", "ApplicationUser")
                        .WithOne("Employee")
                        .HasForeignKey("BackEnd.Domain.Entities.Identity.Employee", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ApplicationUser");
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Product", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("BackEnd.Domain.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BackEnd.Domain.Entities.Identity.ApplicationUser", b =>
                {
                    b.Navigation("Employee");
                });
#pragma warning restore 612, 618
        }
    }
}
```

# BackEnd.Infrastructure\Persistence\DbContext\ApplicationDbContext.cs
```cs
﻿using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Entities.Notification;
using BackEnd.Domain.Entities.Payment;
using BackEnd.Domain.Entities.Sponsorship;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Donor> Donors { get; set; }
        public DbSet<StaffMember> StaffMembers { get; set; }
        public DbSet<OtpRecord> OtpRecords { get; set; }
        public DbSet<Sponsorship> Sponsorships { get; set; }
        public DbSet<SponsorshipSubscription> SponsorshipSubscriptions { get; set; }
        public DbSet<PaymentRequest> PaymentRequests { get; set; }
        public DbSet<GeneralDonation> GeneralDonations { get; set; }
        public DbSet<InKindDonation> InKindDonations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DeliveryArea> DeliveryAreas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            var adminRoleId = "admin-role-id";
            var receptionRoleId = "reception-role-id";
            var donorRoleId = "donor-role-id";
            var adminUserId = "admin-user-id";

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "1" },
                new IdentityRole { Id = receptionRoleId, Name = "Reception", NormalizedName = "RECEPTION", ConcurrencyStamp = "2" },
                new IdentityRole { Id = donorRoleId, Name = "Donor", NormalizedName = "DONOR", ConcurrencyStamp = "3" }
            );

            builder.Entity<ApplicationUser>().HasData(new ApplicationUser
            {
                Id = adminUserId,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@resala.org",
                NormalizedEmail = "ADMIN@RESALA.ORG",
                EmailConfirmed = true,
                IsActive = true,
                FirstName = "Admin",
                LastName = "Resala",
                PasswordHash = "AQAAAAIAAYagAAAAEDZffCZ1Jv0MApj6ocE4KMf3SPhwXC54xd93VsfFUTGo7wUq9IuNZL8SrGw7iMxqIg==",
                SecurityStamp = "admin-security",
                ConcurrencyStamp = "admin-concurrency",
                CreatedOn = new DateTime(2026, 1, 1)
            });

            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = adminUserId, RoleId = adminRoleId }
            );
        }

    }
}
```

# BackEnd.Infrastructure\Persistence\Repositories\DonorRepository.cs
```cs
﻿using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class DonorRepository : IDonorRepository
    {
        private readonly ApplicationDbContext _db;
        public DonorRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(Donor donor, CancellationToken ct)
            => await _db.Donors.AddAsync(donor, ct);

        public Task<int?> GetIdByUserIdAsync(string userId, CancellationToken ct)
            => _db.Donors
                .Where(d => d.UserId == userId)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync(ct);

        public Task SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
```

# BackEnd.Infrastructure\Persistence\Repositories\EntityStateRepository.cs
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

# BackEnd.Infrastructure\Persistence\Repositories\GenericRepository.cs
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

# BackEnd.Infrastructure\Persistence\Repositories\ReadRepository.cs
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

# BackEnd.Infrastructure\Persistence\Repositories\StaffRepository.cs
```cs
﻿using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Enums;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly ApplicationDbContext _db;
        public StaffRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(StaffMember staff, CancellationToken ct)
            => await _db.StaffMembers.AddAsync(staff, ct);

        public Task<int?> GetIdByUserIdAsync(string userId, CancellationToken ct)
            => _db.StaffMembers
                .Where(s => s.UserId == userId)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync(ct);

        public Task<AccountStatus?> GetStatusByIdAsync(int staffId, CancellationToken ct)
            => _db.StaffMembers
                .Where(s => s.Id == staffId)
                .Select(s => (AccountStatus?)s.AccountStatus)
                .FirstOrDefaultAsync(ct);

        public Task SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
```

# BackEnd.Infrastructure\Persistence\Repositories\UserRepository.cs
```cs
﻿using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public UserRepository(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public Task<ApplicationUser?> GetByPhoneAsync(string phone, CancellationToken ct)
            => _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone, ct);

        public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct)
            => _db.Users.FirstOrDefaultAsync(
                u => u.Email == email.ToLowerInvariant(), ct);

        public Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken ct)
            => _userManager.FindByNameAsync(username)!;

        public Task<bool> PhoneExistsAsync(string phone, CancellationToken ct)
            => _db.Users.AnyAsync(u => u.PhoneNumber == phone, ct);

        public Task<bool> EmailExistsAsync(string email, CancellationToken ct)
            => _db.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

        public async Task<string?> GetRoleAsync(ApplicationUser user, CancellationToken ct)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault();
        }
    }
}
```

# BackEnd.Infrastructure\Persistence\UnitOfWork.cs
```cs
﻿using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Infrastructure.Persistence.DbContext;

namespace BackEnd.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

# BackEnd.Infrastructure\Services\EmailService.cs
```cs
﻿using BackEnd.Application.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace BackEnd.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config) => _config = config;

        public async Task SendOtpEmailAsync(string toEmail, string otpCode,
            string purpose, CancellationToken ct = default)
        {
            var subject = purpose == "EmailVerification"
                ? "تأكيد البريد الإلكتروني — رسالة الخيرية"
                : "إعادة تعيين كلمة المرور — رسالة الخيرية";

            var purposeText = purpose == "EmailVerification"
                ? "التحقق من بريدك الإلكتروني"
                : "إعادة تعيين كلمة المرور";

            var body = $@"
<div dir='rtl' style='font-family:Arial,sans-serif;max-width:500px;margin:auto'>
  <div style='background:#1B4F72;padding:20px;border-radius:8px 8px 0 0'>
    <h2 style='color:#fff;margin:0'>🕌 رسالة الخيرية</h2>
  </div>
  <div style='background:#f9f9f9;padding:24px;border:1px solid #ddd'>
    <p>مرحباً،</p>
    <p>رمز {purposeText} الخاص بك هو:</p>
    <div style='background:#fff;border:2px dashed #1B4F72;border-radius:8px;
                padding:16px;text-align:center;margin:20px 0'>
      <span style='font-size:36px;letter-spacing:10px;
                   font-weight:bold;color:#1B4F72'>{otpCode}</span>
    </div>
    <p style='color:#e74c3c'>⏰ صالح لمدة 10 دقائق فقط</p>
    <p>إذا لم تطلب هذا الرمز، يُرجى تجاهل هذا البريد.</p>
  </div>
</div>";

            await SendAsync(toEmail, subject, body, ct);
        }

        private async Task SendAsync(string toEmail, string subject,
            string htmlBody, CancellationToken ct)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_config["Email:SenderEmail"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["Email:Host"],
                int.Parse(_config["Email:Port"] ?? "587"),
                SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(
                _config["Email:Username"],
                _config["Email:Password"], ct);
            await smtp.SendAsync(message, ct);
            await smtp.DisconnectAsync(true, ct);
        }
    }
}
```

# BackEnd.Infrastructure\Services\FileService.cs
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

# BackEnd.Infrastructure\Services\IdentityServies.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Domain.Entities.Identity.AuthenticationHepler;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace BackEnd.Infrastructure.Services
{
    public class IdentityServies : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSittings _jwt;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        public IdentityServies(UserManager<ApplicationUser> userManager,
            JwtSittings jwt, IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _jwt = jwt;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<Result<string>> CreateJwtToken(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                return Result<string>.Failure("User is null", ErrorType.BadRequest);

            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(role => new Claim("roles", role)).ToList();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("uid", user.Id),
            }
            .Union(userClaims)
            .Union(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddHours(_jwt.DurationInHours),
                signingCredentials: creds);

            return Result<string>.Success(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public async Task<Result<ResponseAuthModel>> RefreshTokenAsunc(string token)
        {
            var user = await _userManager.Users
                .Include(u => u.refreshTokens)
                .SingleOrDefaultAsync(u => u.refreshTokens.Any(t => t.token == token));

            if (user == null)
                return Result<ResponseAuthModel>.Failure("User Not Found", ErrorType.NotFound);

            var refreshToken = user.refreshTokens.Single(t => t.token == token);

            if (!refreshToken.IsActive)
                return Result<ResponseAuthModel>.Failure("Inactive Token", ErrorType.Unauthorized);

            refreshToken.RevokedOn = DateTime.UtcNow;
            var newRefreshToken = GenerateRefreshToken();
            user.refreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            var jwtToken = await CreateJwtToken(user);
            if (!jwtToken.IsSuccess)
                return Result<ResponseAuthModel>.Failure(jwtToken.Message, jwtToken.ErrorType);

            var roles = await _userManager.GetRolesAsync(user);

            return Result<ResponseAuthModel>.Success(new ResponseAuthModel
            {
                Token = jwtToken.Value,
                Roles = roles.ToList(),
                RefreshToken = newRefreshToken.token,
                RefreshTokenExpiration = newRefreshToken.ExpireOn,
                CookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = newRefreshToken.ExpireOn,
                    Path = "/"
                }
            });
        }

        public async Task<Result<bool>> RevokeRefreshTokenFromCookiesAsync()
        {
            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["RefreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Result<bool>.Failure("Refresh token not found in cookies", ErrorType.NotFound);

            var user = await _userManager.Users
                .Include(u => u.refreshTokens)
                .FirstOrDefaultAsync(u => u.refreshTokens.Any(t => t.token == refreshToken));

            if (user == null)
                return Result<bool>.Failure("User not found", ErrorType.NotFound);

            var token = user.refreshTokens.SingleOrDefault(t => t.token == refreshToken);

            if (token == null || !token.IsActive)
                return Result<bool>.Failure("Token not active", ErrorType.Unauthorized);

            token.RevokedOn = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Result<bool>.Failure("Failed to revoke token", ErrorType.InternalServerError);

            _httpContextAccessor.HttpContext.Response.Cookies.Delete("RefreshToken", new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Result<bool>.Success(true);
        }

        public RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNumber);

            return new RefreshToken
            {
                token = Convert.ToBase64String(randomNumber),
                CreatedOn = DateTime.UtcNow,
                ExpireOn = DateTime.UtcNow.AddDays(7)
            };
        }

        public async Task<Result<ResponseAuthModel>> GenerateRefreshTokenAsync(ApplicationUser user, bool rememberMe, CancellationToken cancellationToken = default)
        {
            if (user == null)
                return Result<ResponseAuthModel>.Failure("User is null", ErrorType.BadRequest);

            var jwtToken = await CreateJwtToken(user);
            if (!jwtToken.IsSuccess)
                return Result<ResponseAuthModel>.Failure(jwtToken.Message, jwtToken.ErrorType);

            RefreshToken refreshToken;
            var existingActiveToken = user.refreshTokens.FirstOrDefault(r => r.IsActive);

            if (existingActiveToken != null)
            {
                refreshToken = existingActiveToken;
            }
            else
            {
                refreshToken = GenerateRefreshToken();
                if (rememberMe)
                    refreshToken.ExpireOn = DateTime.UtcNow.AddDays(30);

                user.refreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Result<ResponseAuthModel>.Success(new ResponseAuthModel
            {
                Message = "Login successful.",
                Token = jwtToken.Value,
                Roles = roles.ToList(),
                RefreshToken = refreshToken.token,
                RefreshTokenExpiration = refreshToken.ExpireOn,
                CookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = refreshToken.ExpireOn,
                    Path = "/"
                }
            });
        }

        public async Task<Result<bool>> IsEmailExist(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return Result<bool>.Success(user != null);
        }

        public async Task<Result<bool>> IsInRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found", ErrorType.NotFound);

            var inRole = await _userManager.IsInRoleAsync(user, role);
            return Result<bool>.Success(inRole);
        }

        public async Task<Result<ApplicationUser>> IsUserExist(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<ApplicationUser>.Failure("User not found", ErrorType.NotFound);

            return Result<ApplicationUser>.Success(user);
        }

        public async Task<Result<IdentityResult>> CreateUserAsync(ApplicationUser user, string password,string Role ,CancellationToken cancellationToken = default)
        {
            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
                return Result<IdentityResult>.Failure("Failed to create user", ErrorType.BadRequest);

            var roleResult = await _userManager.AddToRoleAsync(user, Role);
            if (!roleResult.Succeeded)
                return Result<IdentityResult>.Failure("Failed to assign role", ErrorType.InternalServerError);

            return Result<IdentityResult>.Success(roleResult);
        }

        public async Task<Result<bool>> IsPasswordExist(ApplicationUser user, string Password, CancellationToken cancellationToken = default)
        {
            var result = await _userManager.CheckPasswordAsync(user, Password);
            return Result<bool>.Success(result);
        }

        public async Task<Result<ApplicationUser?>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var result = await _userManager.FindByEmailAsync(email);
            if (result == null)
                return Result<ApplicationUser?>.Failure("User not found", ErrorType.NotFound);

            return Result<ApplicationUser?>.Success(result);
        }

        public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }
        public string? GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("uid")?.Value;
        }

        public async Task<string> GetRestPasswordTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            return WebEncoders.Base64UrlEncode(tokenBytes);
        }

    }
}
```

# BackEnd.Infrastructure\Services\JwtService.cs
```cs
﻿using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BackEnd.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config) => _config = config;

        public string GenerateToken(
            ApplicationUser user, string role,
            int? donorId, int? staffId)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new(ClaimTypes.Role, role),
                new("fullName", $"{user.FirstName} {user.LastName}".Trim()),
                new("phoneNumber", user.PhoneNumber ?? ""),
            };

            if (donorId.HasValue)
                claims.Add(new("donorId", donorId.Value.ToString()));
            if (staffId.HasValue)
                claims.Add(new("staffId", staffId.Value.ToString()));

            var expireDays = int.Parse(_config["Jwt:ExpireDays"] ?? "7");

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expireDays),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
```

# BackEnd.Infrastructure\Services\OtpService.cs
```cs
﻿using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private readonly ApplicationDbContext _db;
        public OtpService(ApplicationDbContext db) => _db = db;

        public string GenerateOtp()
        {
            var bytes = new byte[4];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            var number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1_000_000;
            return number.ToString("D6");
        }

        public async Task SaveOtpAsync(string email, string code, string purpose,
            CancellationToken ct = default)
        {
            var oldOtps = await _db.OtpRecords
                .Where(o => o.Email == email.ToLower() &&
                            o.Purpose == purpose &&
                            !o.IsUsed)
                .ToListAsync(ct);

            foreach (var old in oldOtps)
                old.MarkAsUsed();

            var otp = OtpRecord.Create(email, code, purpose, expiryMinutes: 10);
            _db.OtpRecords.Add(otp);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> ValidateOtpAsync(string email, string code, string purpose,
            CancellationToken ct = default)
        {
            var otp = await _db.OtpRecords
                .Where(o => o.Email == email.ToLower() &&
                            o.Code == code &&
                            o.Purpose == purpose &&
                            !o.IsUsed)
                .OrderByDescending(o => o.CreatedOn)
                .FirstOrDefaultAsync(ct);

            if (otp is null || !otp.IsValid())
                return false;

            otp.MarkAsUsed();
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
```

# BackEndApi\Comman\ApiResponse.cs
```cs
﻿using System;
using System.Net;

namespace BackEnd.Api.Common
{
    public class ApiResponse<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public object Meta { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(T data, string message = null)
        {
            Succeeded = true;
            Data = data;
            Message = message;
            StatusCode = HttpStatusCode.OK;
        }

        public ApiResponse(string message, bool succeeded = false)
        {
            Succeeded = succeeded;
            Message = message;
            StatusCode = succeeded ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
        }

        public static ApiResponse<T> Success(T data, HttpStatusCode statusCode = HttpStatusCode.OK, string? message = null)
            => new ApiResponse<T> { Data = data, StatusCode = statusCode, Message = message, Succeeded = true };

        public static ApiResponse<T> Fail(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, List<string> errors = null) =>
            new ApiResponse<T>(message, false) { StatusCode = statusCode, Errors = errors, Succeeded = false };
    }

}
```

# BackEndApi\Comman\ApplicationControllerBase.cs
```cs
﻿using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Api.Comman
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApplicationControllerBase : ControllerBase
    {
        protected readonly IMediator _mediator;
        protected ApplicationControllerBase(IMediator mediator)
        {
            _mediator = mediator;
        }
    }
}
```

# BackEndApi\Comman\BaseCrudController.cs
```cs
﻿using BackEnd.Application.Common;
using BackEnd.Application.Common.ResponseFormat;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Api.Common
{
    public abstract class BaseCrudController<
    TCreateCommand, TCreateResponse,
    TUpdateCommand, TUpdateResponse,
    TDeleteCommand, TDeleteResponse,
    TGetByIdQuery, TGetByIdResponse,
    TGetAllQuery, TGetAllResponse> : ControllerBase
    where TCreateCommand : IRequest<Result<TCreateResponse>>
    where TUpdateCommand : IRequest<Result<TUpdateResponse>>
    where TDeleteCommand : IRequest<Result<TDeleteResponse>>
    where TGetByIdQuery : IRequest<Result<TGetByIdResponse>>
    where TGetAllQuery : IRequest<Result<TGetAllResponse>>
    {
        protected readonly IMediator _mediator;

        protected BaseCrudController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public virtual async Task<IActionResult> Create([FromBody] TCreateCommand command)
            => Ok(await _mediator.Send(command));

        [HttpPut]
        public virtual async Task<IActionResult> Update([FromBody] TUpdateCommand command)
            => Ok(await _mediator.Send(command));

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete([FromRoute] TDeleteCommand command)
            => Ok(await _mediator.Send(command));

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById([FromRoute] TGetByIdQuery query)
            => Ok(await _mediator.Send(query));

        [HttpGet]
        public virtual async Task<IActionResult> GetAll([FromQuery] TGetAllQuery query)
        {
            query ??= Activator.CreateInstance<TGetAllQuery>();
            return Ok(await _mediator.Send(query));
        }
    }

}
```

# BackEndApi\Controllers\AuthController.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.Auth.Commands;
using BackEnd.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Api.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuthController(IMediator mediator) => _mediator = mediator;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDonorCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            if (!result.IsSuccess) return StatusCode(MapError(result.ErrorType), result);
            return StatusCode(201, result);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            if (!result.IsSuccess) return StatusCode(MapError(result.ErrorType), result);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            if (!result.IsSuccess) return StatusCode(MapError(result.ErrorType), result);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            if (!result.IsSuccess) return StatusCode(MapError(result.ErrorType), result);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            if (!result.IsSuccess) return StatusCode(MapError(result.ErrorType), result);
            return Ok(result);
        }

        [HttpPost("register-donor")]
        [Authorize(Roles = "Reception,Admin")]
        public async Task<IActionResult> RegisterDonorByStaff(
            [FromBody] RegisterDonorByStaffCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            if (!result.IsSuccess) return StatusCode(MapError(result.ErrorType), result);
            return StatusCode(201, result);
        }

        [HttpPost("create-staff")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            if (!result.IsSuccess) return StatusCode(MapError(result.ErrorType), result);
            return StatusCode(201, result);
        }

        private static int MapError(ErrorType? type) => type switch
        {
            ErrorType.NotFound => 404,
            ErrorType.Unauthorized => 401,
            ErrorType.Forbidden => 403,
            ErrorType.Conflict => 409,
            ErrorType.BadRequest => 400,
            ErrorType.UnprocessableEntity => 422,
            _ => 500
        };
    }
}
```

# BackEndApi\Controllers\AuthenticationController.cs
```cs
﻿using Azure;
using BackEnd.Api.Comman;
using BackEnd.Application.Features.AuthenticationFeatures.LoginAndTokens.LoginUser.Command.Model;
using BackEnd.Application.Features.AuthenticationFeatures.LoginAndTokens.Logout.Command;
using BackEnd.Application.Features.AuthenticationFeatures.LoginAndTokens.RefreshToken.Model;
using BackEnd.Application.Features.AuthenticationFeatures.Password.RestPassword.Command;
using BackEnd.Application.Features.Login.Commands.Models;
using ECommerce.Application.Features.AuthenticationFeatures.Password.RestPassword.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ApplicationControllerBase
    {
        public AuthenticationController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost("Register-Admin")]
        public async Task<IActionResult> RegisterVolunteer([FromForm] AdminRegisterCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPost("Register-Employee")]
        public async Task<IActionResult> RegisterDeafUser([FromForm] EmployeeRegisterCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPost("Login")]
        public async Task<IActionResult> UserLogin([FromForm] UserLogInCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess &&
                result.Value?.RefreshToken != null &&
                result.Value.CookieOptions != null)
            {
                Response.Cookies.Append(
                    "RefreshToken",
                    result.Value.RefreshToken,
                    result.Value.CookieOptions
                );
            }

            return Ok(result);
        }

        [HttpPost("Generate-New-token-From-RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var result = await _mediator.Send(new RefreshTokenCommand());

            if (result.IsSuccess &&
                result.Value?.RefreshToken != null &&
                result.Value.CookieOptions != null)
            {
                Response.Cookies.Delete("RefreshToken");
                Response.Cookies.Append(
                    "RefreshToken",
                    result.Value.RefreshToken,
                    result.Value.CookieOptions
                );
            }

            return Ok(result);
        }

        [HttpPost("Rest-Password")]
        public async Task<IActionResult> RestPassword([FromForm] ResetPasswordCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPost("Forget-Password")]
        public async Task<IActionResult> ForgetPassword([FromForm] SendTokenToRestPasswordCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            return Ok(await _mediator.Send(new UserLogoutCommand()));
        }
    }
}
```

# BackEndApi\Filters\ResultActionFilter.cs
```cs
﻿using BackEnd.Application.Common.ResponseFormat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using IResult = BackEnd.Application.Common.ResponseFormat.IResult;

namespace BackEnd.Api.Filters
{
    public sealed class ResultActionFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(
            ResultExecutingContext context,
            ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult &&
                objectResult.Value is IResult result)
            {
                context.Result = CreateActionResult(result);
            }

            await next();
        }

        private IActionResult CreateActionResult(IResult result)
        {
            if (result.IsSuccess)
            {
                return new OkObjectResult(
                    ApiResponse<object>.Success(result.Value, HttpStatusCode.OK)
                );
            }

            var statusCode = result.ErrorType switch
            {
                ErrorType.NotFound => HttpStatusCode.NotFound,
                ErrorType.BadRequest => HttpStatusCode.BadRequest,
                ErrorType.Conflict => HttpStatusCode.Conflict,
                ErrorType.UnprocessableEntity => HttpStatusCode.UnprocessableEntity,
                ErrorType.Unauthorized => HttpStatusCode.Unauthorized,
                ErrorType.Forbidden => HttpStatusCode.Forbidden,
                _ => HttpStatusCode.InternalServerError
            };

            return new ObjectResult(
                ApiResponse<object>.Fail(result.Message, statusCode))
            {
                StatusCode = (int)statusCode
            };
        }
    }
}
```

# BackEndApi\Program.cs
```cs
using BackEnd.Api.Filters;
using BackEnd.Application.ALLApplicationDependencies;
using BackEnd.Infrastructure.AllInfrastructureDependencies;
using BackEnd.Infrastructure.InfrastructureDependencies;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResultActionFilter>();
});

builder.Services
    .AddApplicationDependencies()
    .AuthenticationServices(builder.Configuration)
    .AddInfrastructureDependencies(builder.Configuration);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BackEnd v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

