using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Application.Features.Auth.Commands;
using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

public class RegisterDonorHandler
     : IRequestHandler<RegisterDonorCommand, Result<RegisterDonorResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDonorRepository _donorRepo;
    private readonly IUserRepository _userRepo;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterDonorHandler> _logger;

    public RegisterDonorHandler(
        UserManager<ApplicationUser> userManager,
        IDonorRepository donorRepo,
        IUserRepository userRepo,
        IOtpService otpService,
        IEmailService emailService,
        ILogger<RegisterDonorHandler> logger)
    {
        _userManager = userManager;
        _donorRepo = donorRepo;
        _userRepo = userRepo;
        _otpService = otpService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<RegisterDonorResponse>> Handle(
        RegisterDonorCommand request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Register donor started: {PhoneNumber}, {Email}",
            request.PhoneNumber, request.Email);

        // 1. Check duplicates
        if (await _userRepo.PhoneExistsAsync(request.PhoneNumber, ct))
        {
            _logger.LogWarning(
                "Register donor failed — phone already exists: {PhoneNumber}",
                request.PhoneNumber);

            return Result<RegisterDonorResponse>.Failure(
                "رقم الهاتف مسجّل مسبقاً.", ErrorType.Conflict);
        }

        if (await _userRepo.EmailExistsAsync(request.Email, ct))
        {
            _logger.LogWarning(
                "Register donor failed — email already exists: {Email}",
                request.Email);

            return Result<RegisterDonorResponse>.Failure(
                "البريد الإلكتروني مسجّل مسبقاً.", ErrorType.Conflict);
        }

        var nameParts = request.Name.Trim().Split(' ', 2);
        var firstName = nameParts[0];
        var lastName = nameParts.Length > 1 ? nameParts[1] : "";

        // 2. Domain validation
        Donor donor;
        try
        {
            donor = Donor.Create(
                userId: "temp",
                firstName: firstName,
                lastName: lastName,
                email: request.Email,
                phoneNumber: request.PhoneNumber,
                job: request.Job,
                landline: request.Landline
            );

            _logger.LogInformation(
                "Donor domain validation passed: {PhoneNumber}",
                request.PhoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Register donor failed — domain validation error: {PhoneNumber}",
                request.PhoneNumber);

            return Result<RegisterDonorResponse>.Failure(
                ex.Message, ErrorType.BadRequest);
        }

        // 3. Create user
        var user = new ApplicationUser
        {
            UserName = request.PhoneNumber,
            Email = request.Email.ToLowerInvariant().Trim(),
            PhoneNumber = request.PhoneNumber,
            FirstName = firstName,
            LastName = lastName,
            IsActive = false,
            EmailConfirmed = false,
            CreatedOn = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            _logger.LogError(
                "Register donor failed — identity creation error: {PhoneNumber}, Errors: {@Errors}",
                request.PhoneNumber,
                createResult.Errors);

            var errors = createResult.Errors
                .ToDictionary(e => e.Code, e => new[] { e.Description });

            return Result<RegisterDonorResponse>.Failure(
                "فشل انشاءالحساب", ErrorType.BadRequest, errors);
        }

        await _userManager.AddToRoleAsync(user, "Donor");

        _logger.LogInformation(
            "User created successfully: {UserId}", user.Id);

        // 4. Save donor
        try
        {
            donor.SetUserId(user.Id);

            await _donorRepo.AddAsync(donor, ct);
            await _donorRepo.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Donor saved successfully: {DonorId}, UserId: {UserId}",
                donor.Id, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Register donor failed — saving donor error, rolling back user: {UserId}",
                user.Id);

            await _userManager.DeleteAsync(user);

            return Result<RegisterDonorResponse>.Failure(
                ex.Message, ErrorType.BadRequest);
        }

        // 5. OTP
        var otpCode = _otpService.GenerateOtp();

        await _otpService.SaveOtpAsync(user.Email!, otpCode, "EmailVerification", ct);

        _logger.LogInformation(
            "OTP generated and saved for email: {Email}",
            user.Email);

        try
        {
            await _emailService.SendOtpEmailAsync(user.Email!, otpCode, "EmailVerification", ct);

            _logger.LogInformation(
                "OTP email sent successfully: {Email}",
                user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send OTP email: {Email}",
                user.Email);

            return Result<RegisterDonorResponse>.Success(
                new RegisterDonorResponse(donor.Id,
                    "تم انشاءالحساب,لكن لم يتم ارسال رمز التحقق ,قم بأعادة ارسال الرمز"));
        }

        _logger.LogInformation(
            "Register donor completed successfully: {DonorId}",
            donor.Id);

        return Result<RegisterDonorResponse>.Success(
            new RegisterDonorResponse(donor.Id,
                "تم انشاء الحساب ,راجع البريدالخاص بك للحصول علي رمزالتحقق"));
    }
}