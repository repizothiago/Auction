using Auction.Domain.Entities.Base;

namespace Auction.Domain.Entities;

public class Category : BaseEntity
{
    protected Category() { }

    private Category(string name, string? description) : base(Guid.NewGuid(), DateTime.UtcNow, null)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; private set; }
    public string? Description { get; private set; }

    public static Category Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Category name cannot exceed 100 characters", nameof(name));

        return new Category(name, description);
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }
}
