using BackEnd.Application.Common.ResponseFormat;
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
        // 1. ✅ التحقق من عدم التكرار أولاً قبل أي عملية
        if (await _userRepo.PhoneExistsAsync(request.PhoneNumber, ct))
            return Result<RegisterDonorResponse>.Failure(
                "رقم الهاتف مسجّل مسبقاً.", ErrorType.Conflict);

        if (await _userRepo.EmailExistsAsync(request.Email, ct))
            return Result<RegisterDonorResponse>.Failure(
                "البريد الإلكتروني مسجّل مسبقاً.", ErrorType.Conflict);

        // 2. ✅ تحقق من الـ Domain Data قبل لمس الـ DB
        var nameParts = request.Name.Trim().Split(' ', 2);
        var firstName = nameParts[0];
        var lastName = nameParts.Length > 1 ? nameParts[1] : "";

        // هذا السطر بيتحقق من صحة الهاتف والإيميل عبر Domain Exceptions
        // لو في مشكلة هيرمي Exception هنا قبل ما ننشئ الـ User
        Donor donor;
        try
        {
            donor = Donor.Create(
                userId: "temp",   // مؤقت — هنبدّله بعد إنشاء الـ User
                firstName: firstName,
                lastName: lastName,
                email: request.Email,
                phoneNumber: request.PhoneNumber,
                job: request.Job,
                landline: request.Landline
            );
        }
        catch (Exception ex)
        {
            return Result<RegisterDonorResponse>.Failure(
                ex.Message, ErrorType.BadRequest);
        }

        // 3. ✅ بعد التحقق من كل شيء — أنشئ الـ User
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
            var errors = createResult.Errors
                .ToDictionary(e => e.Code, e => new[] { e.Description });
            return Result<RegisterDonorResponse>.Failure(
                "فشل انشاءالحساب", ErrorType.BadRequest, errors);
        }

        await _userManager.AddToRoleAsync(user, "Donor");

        // 4. ✅ ربط الـ Donor بالـ User الحقيقي + حفظه
        try
        {
            donor.SetUserId(user.Id);  // method بسيطة نضيفها في الـ Donor entity
            await _donorRepo.AddAsync(donor, ct);
            await _donorRepo.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // ✅ Rollback — لو الـ Donor فشل احذف الـ User
            await _userManager.DeleteAsync(user);
            return Result<RegisterDonorResponse>.Failure(
                ex.Message, ErrorType.BadRequest);
        }

        // 5. ✅ OTP
        var otpCode = _otpService.GenerateOtp();
        await _otpService.SaveOtpAsync(user.Email!, otpCode, "EmailVerification", ct);

        try
        {
            await _emailService.SendOtpEmailAsync(user.Email!, otpCode, "EmailVerification", ct);
        }
        catch
        {
            // الـ User والـ Donor اتحفظوا — الـ OTP اتحفظ
            // بس الإيميل مش اشتغل — يرجع نجاح مع رسالة مختلفة
            return Result<RegisterDonorResponse>.Success(
                new RegisterDonorResponse(donor.Id,
                    "تم انشاءالحساب,لكن لم يتم ارسال رمز التحقق ,قم بأعادة ارسال الرمز"));
        }

        return Result<RegisterDonorResponse>.Success(
            new RegisterDonorResponse(donor.Id,
                "تم انشاء الحساب ,راجع البريدالخاص بك للحصول علي رمزالتحقق"));
    }
}