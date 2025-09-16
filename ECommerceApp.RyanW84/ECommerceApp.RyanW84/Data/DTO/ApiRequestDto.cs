namespace ECommerceApp.RyanW84.Data.DTO;

public class ApiRequestDto<T>
{
    public bool IsFailure { get; set; }
    public string Message { get; set; } = null!;
    public object Data { get; set; } = null!;
}
