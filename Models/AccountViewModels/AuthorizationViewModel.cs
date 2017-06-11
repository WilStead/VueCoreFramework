namespace MVCCoreVue.Models.AccountViewModels
{
    public class AuthorizationViewModel
    {
        public const string Authorized = "authorized";
        public const string Unauthorized = "unauthorized";

        public string Email { get; set; }

        public string Token { get; set; }

        public string Authorization { get; set; }
    }
}
