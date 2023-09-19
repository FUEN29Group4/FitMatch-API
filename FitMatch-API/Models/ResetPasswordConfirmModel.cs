namespace FitMatch_API.Models
{
    public class ResetPasswordConfirmModel
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
