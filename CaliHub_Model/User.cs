using System;
using System.Collections.Generic;
using System.Text;

namespace CaliHub_Model
{
    public class User : BaseEntity
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // כרגע נשמר כטקסט רגיל עד לשלב ההצפנה
        public int RoleId { get; set; }
        public bool IsActive { get; set; }

        // הסימן ? מגדיר שהשדה יכול להיות null כאשר שם התפקיד טרם נשלף
        public string? RoleName { get; set; }

        public User()
        {
        }

        // בנאי 1: למשתמש חדש שעוד לא נשמר בבסיס הנתונים
        public User(string username, string email, string passwordHash, int roleId) : base()
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            RoleId = roleId;
            IsActive = true;
            RoleName = null;
        }

        // בנאי 2: למשתמש שנקרא מטבלת בסיס הנתונים (כולל Id)
        public User(int id, string username, string email, string passwordHash, int roleId, bool isActive, string? roleName = null) : base(id)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            RoleId = roleId;
            IsActive = isActive;
            RoleName = roleName;
        }

        // דריסת הפעולה כדי להציג את פרטי המשתמש בקלות
        public override string ToString()
        {
            return $"{Username} ({RoleName ?? "ללא תפקיד"})";
        }
    }
}
