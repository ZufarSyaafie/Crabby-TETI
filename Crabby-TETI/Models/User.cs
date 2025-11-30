using System;

namespace CrabbyTETI.Models
{
    // User model representing database user entity
    public class User
    {
        private string _name = string.Empty;
        private string _email = string.Empty;

        public int Id { get; set; }

        public string Name 
        { 
            get => _name;
            set => _name = value ?? string.Empty;
        }

        public string Email 
        { 
            get => _email;
            set => _email = value ?? string.Empty;
        }

        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;

        // Validate email format
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return email.Contains("@") && email.Contains(".");
        }

        public User()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public User(string name, string email, string passwordHash)
        {
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        // Get display name
        public string GetDisplayName()
        {
            return !string.IsNullOrWhiteSpace(_name) ? _name : _email;
        }

        // Validate user data
        internal bool ValidateUserData()
        {
            if (string.IsNullOrWhiteSpace(_name) || _name.Length < 2)
                return false;

            if (!IsValidEmail(_email))
                return false;

            return true;
        }

        // Read-only properties
        public string UserInfo => $"{_name} ({_email})";
        public bool IsNewUser => (DateTime.UtcNow - CreatedAt).TotalDays < 1;
    }
}
