namespace GymManagement.Domain.Subscriptions;

public class Subscription
{
    public Guid Id { get; set; }
    public string SubscriptionType { get; set; } = null!;
}