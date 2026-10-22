namespace BookingApi.Services;

public interface ICurrentUser
{
    string GetRequiredUserId();
}