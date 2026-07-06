using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Infrastructure.Services
{
    public class SubscriptionReminderJob
    {
        private readonly ISponsorshipSubscriptionRepository _repository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SubscriptionReminderJob> _logger;

        public SubscriptionReminderJob(
            ISponsorshipSubscriptionRepository repository,
            INotificationService notificationService,
            ILogger<SubscriptionReminderJob> logger)
        {
            _repository = repository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task ProcessRemindersAsync()
        {
            _logger.LogInformation(">>> Starting SubscriptionReminderJob: ProcessRemindersAsync <<<");

            var subscriptions = await _repository.GetAllAsync();
            var activeSubs = subscriptions.Where(s => s.Status == SubscriptionStatus.Active).ToList();

            _logger.LogInformation("Found {Count} active subscriptions to check.", activeSubs.Count);

            foreach (var sub in activeSubs)
            {
                try
                {
                    var totalDays = (sub.NextPaymentDate - sub.LastPaymentDate).TotalDays;
                    if (totalDays <= 0)
                    {
                        _logger.LogWarning("Subscription {Id} has invalid date range (Next: {Next}, Last: {Last}). Skipping.", sub.Id, sub.NextPaymentDate, sub.LastPaymentDate);
                        continue;
                    }

                    var daysPassed = (DateTime.UtcNow.Date - sub.LastPaymentDate.Date).TotalDays;
                    if (daysPassed < 0) daysPassed = 0;

                    var percentage = (daysPassed / totalDays) * 100.0;
                    var today = DateTime.UtcNow.Date;
                    var dueDate = sub.NextPaymentDate.Date;

                    _logger.LogInformation("Sub {Id}: Cycle={TotalDays} days, Passed={DaysPassed} days, Percent={Percent:F2}%. NextPaymentDate={NextPaymentDate}",
                        sub.Id, totalDays, daysPassed, percentage, dueDate);

                    // 1. 50% Reminder
                    if (percentage >= 50.0 && percentage < 75.0 && !sub.Sent50PercentReminder)
                    {
                        var title = "تذكير بقرب موعد التجديد (50%)";
                        var msg = $"عزيزي المتبرع، لقد مضى نصف مدة الاشتراك الخاص بك. يرجى الاستعداد للتجديد في تاريخ {dueDate:yyyy-MM-dd}.";
                        await _notificationService.SendToUserAsync(sub.DonorId, title, msg, NotificationType.PaymentReminder, sub.Id);
                        
                        sub.Sent50PercentReminder = true;
                        _repository.Update(sub);
                    }
                    // 2. 75% Reminder
                    else if (percentage >= 75.0 && percentage < 100.0 && !sub.Sent75PercentReminder)
                    {
                        var title = "تذكير بقرب موعد التجديد (75%)";
                        var msg = $"عزيزي المتبرع، يرجى العلم بأن موعد تجديد اشتراكك يقترب (متبقي ربع المدة). تاريخ التجديد: {dueDate:yyyy-MM-dd}.";
                        await _notificationService.SendToUserAsync(sub.DonorId, title, msg, NotificationType.PaymentReminder, sub.Id);

                        sub.Sent75PercentReminder = true;
                        _repository.Update(sub);
                    }
                    // 3. 100% Reminder (Due Date reached/passed)
                    else if (today >= dueDate && !sub.Sent100PercentReminder)
                    {
                        var title = "موعد تجديد الاشتراك مستحق";
                        var msg = "عزيزي المتبرع، يرجى العلم بأن اشتراكك مستحق الدفع اليوم. يرجى التجديد لضمان استمرار الدعم.";
                        await _notificationService.SendToUserAsync(sub.DonorId, title, msg, NotificationType.PaymentReminder, sub.Id);

                        sub.Sent100PercentReminder = true;
                        _repository.Update(sub);
                    }
                    // 4. Overdue Reminders (+1 and +2 days)
                    else if (today >= dueDate.AddDays(1))
                    {
                        if (today == dueDate.AddDays(1) && sub.PostDueRemindersCount == 0)
                        {
                            var title = "تنبيه متأخر لتجديد الاشتراك (+1)";
                            var msg = "تنبيه متأخر: يرجى تجديد اشتراكك المستحق منذ أمس.";
                            await _notificationService.SendToUserAsync(sub.DonorId, title, msg, NotificationType.LatePayment, sub.Id);

                            sub.PostDueRemindersCount = 1;
                            _repository.Update(sub);
                        }
                        else if (today >= dueDate.AddDays(2) && sub.PostDueRemindersCount == 1)
                        {
                            var title = "تنبيه متأخر لتجديد الاشتراك (+2)";
                            var msg = "تنبيه متأخر جداً: نرجو منكم الإسراع في تجديد الاشتراك لضمان عدم توقف الكفالة.";
                            await _notificationService.SendToUserAsync(sub.DonorId, title, msg, NotificationType.LatePayment, sub.Id);

                            sub.PostDueRemindersCount = 2;
                            _repository.Update(sub);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing reminders for subscription {Id}", sub.Id);
                }
            }

            await _repository.SaveChangesAsync();
            _logger.LogInformation(">>> Finished SubscriptionReminderJob: ProcessRemindersAsync <<<");
        }
    }
}
