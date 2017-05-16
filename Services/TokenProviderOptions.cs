using Microsoft.IdentityModel.Tokens;
using System;

namespace MVCCoreVue.Services
{
    public class TokenProviderOptions
    {
        public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(90);
        public SigningCredentials SigningCredentials { get; set; }
    }
}
