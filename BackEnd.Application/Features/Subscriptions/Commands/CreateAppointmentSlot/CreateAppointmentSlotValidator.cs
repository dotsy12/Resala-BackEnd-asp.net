// CreateAppointmentSlotCommand.cs + Handler + Validator
using FluentValidation;

namespace BackEnd.Application.Features.Subscriptions.Commands.CreateAppointmentSlot
{
    public class CreateAppointmentSlotValidator
        : AbstractValidator<CreateAppointmentSlotCommand>
    {
        public CreateAppointmentSlotValidator()
        {
            RuleFor(x => x.Dto.SlotDate)
                .Must(d => d.Date >= DateTime.UtcNow.Date)
                .WithMessage("التاريخ يجب أن يكون اليوم أو في المستقبل.");

            RuleFor(x => x.Dto.OpenFrom)
                .LessThan(x => x.Dto.OpenTo)
                .WithMessage("وقت البداية يجب أن يكون قبل وقت النهاية.");

            RuleFor(x => x.Dto.MaxCapacity)
                .GreaterThan(0).WithMessage("السعة يجب أن تكون أكبر من صفر.")
                .LessThanOrEqualTo(100);
        }
    }
}