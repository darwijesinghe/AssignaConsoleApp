namespace ConsoleUI.Response
{
    // responses
    public class Result
    {
        public bool success { get; set; }
        public string? message { get; set; }
    }
 
    public class Result<T> where T : class
    {
        public bool success { get; set; }
        public string? message { get; set; }
        public T? data { get; set; }
    }
    public class AuthResult
    {
        public bool success { get; set; }
        public string? message { get; set; }
        public string? token { get; set; }
        public string? refresh_token { get; set; }
        public string? reset_token { get; set; }
    }

}
