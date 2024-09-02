namespace EventBus.Events;

public class FriendAddEvent
{
    public string UserId { get; set; }
    public string FriendId { get; set; }


    public FriendAddEvent(string userId, string friendId)
    {
        UserId = userId;
        FriendId = friendId;
    }
}