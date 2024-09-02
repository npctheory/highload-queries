namespace EventBus.Events;

public class FriendDeleteEvent
{
    public string UserId { get; set; }
    public string FriendId { get; set; }


    public FriendDeleteEvent(string userId, string friendId)
    {
        UserId = userId;
        FriendId = friendId;
    }
}