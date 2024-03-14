using System.ComponentModel.DataAnnotations;

namespace Auth;

public class AuthModels
{
  public string Token { get; set; }
  public DateTime ExpirationDate { get; set; }
}

public class AuthUser
{
  [Required]
  public string Username { get; set; }
  [Required]
  public string Password { get; set; }
}
