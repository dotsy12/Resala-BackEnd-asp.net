using BackEnd.Domain.Common;
using System;

namespace BackEnd.Domain.Entities
{
    public sealed class SuccessStory : BaseEntity<int>
    {
        public string Title { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public string ImageUrl { get; private set; } = null!;
        public string ImagePublicId { get; private set; } = null!;

        private SuccessStory() { }

        public static SuccessStory Create(string title, string description, string imageUrl, string imagePublicId)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentNullException(nameof(title));
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentNullException(nameof(description));

            return new SuccessStory
            {
                Title = title.Trim(),
                Description = description.Trim(),
                ImageUrl = imageUrl,
                ImagePublicId = imagePublicId,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };
        }
    }
}
