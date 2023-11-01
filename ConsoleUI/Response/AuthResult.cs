namespace ConsoleUI.Response
{
    // responses
    public class Result
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
 
    public class Result<T> where T : class
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? ResetToken { get; set; }
    }

}
