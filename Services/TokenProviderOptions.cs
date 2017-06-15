using Microsoft.IdentityModel.Tokens;
using System;

namespace MVCCoreVue.Services
{
    /// <summary>
    /// Options configuration object containing information about JWT bearer tokens.
    /// </summary>
    public class TokenProviderOptions
    {
        /// <summary>
        /// The time until expiration of the token.
        /// </summary>
        public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(90);

        /// <summary>
        /// The signature validation properties of the token.
        /// </summary>
        public SigningCredentials SigningCredentials { get; set; }
    }
}
