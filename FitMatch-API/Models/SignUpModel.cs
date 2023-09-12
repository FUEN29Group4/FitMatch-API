namespace FitMatch_API.Models
{
    public class SignUpModel
    {
        public string Name { get; set; }
        public bool Gender { get; set; }
        public string Birth { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string AccountType { get; set; }  // 教練 或 會員
    }
}
