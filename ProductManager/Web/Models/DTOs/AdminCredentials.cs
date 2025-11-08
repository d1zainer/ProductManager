namespace Web.Models;

/// <summary>
/// Админские учетные данные
/// </summary>
public class AdminCredentials
{
    /// <summary>
    /// Логин
    /// </summary>
    public string AdminLogin { get; set; } = default!;
    /// <summary>
    /// Пароль
    /// </summary>
    public string AdminPass { get; set; } = default!;
}