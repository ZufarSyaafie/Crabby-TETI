namespace CrabbyTETI.Models
{
    // Authentication result wrapper
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }

        public static AuthResult CreateSuccess(User user, string message = "Berhasil")
        {
            return new AuthResult
            {
                Success = true,
                Message = message,
                User = user
            };
        }

        public static AuthResult CreateFailure(string message)
        {
            return new AuthResult
            {
                Success = false,
                Message = message,
                User = null
            };
        }
    }
}
