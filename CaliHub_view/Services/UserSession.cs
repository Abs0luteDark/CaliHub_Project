using CaliHub_Model;

namespace CaliHub_view.Services
{
    public class UserSession
    {
        // פרטי המשתמש המחובר. קוראים מכל דף, משנים רק דרך הפעולות של המחלקה
        public int UserId { get; private set; }
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public int RoleId { get; private set; }
        public string RoleName { get; private set; } = string.Empty;

        public bool IsLoggedIn => UserId > 0;

        // אירוע שמודיע שהמשתמש המחובר השתנה
        public event Action? OnChange;

        public void Login(User user)
        {
            UserId = user.Id;
            Username = user.Username;
            Email = user.Email;
            RoleId = user.RoleId;
            RoleName = user.RoleName ?? string.Empty;

            if (OnChange != null)
            {
                OnChange.Invoke();
            }
        }

        public void Logout()
        {
            UserId = 0;
            Username = string.Empty;
            Email = string.Empty;
            RoleId = 0;
            RoleName = string.Empty;

            if (OnChange != null)
            {
                OnChange.Invoke();
            }
        }
    }
}