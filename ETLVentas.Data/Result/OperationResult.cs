namespace ETLVentas.Data.Result
{
    /// <summary>
    /// Representa el resultado de una operación.
    /// </summary>
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        
        public static OperationResult<T> Ok(T data) => new OperationResult<T> { Success = true, Data = data };
        public static OperationResult<T> Fail(string message) => new OperationResult<T> { Success = false, ErrorMessage = message };
    }
}
