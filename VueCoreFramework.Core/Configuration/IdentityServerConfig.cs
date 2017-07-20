using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace VueCoreFramework.Core.Configuration
{
    /// <summary>
    /// Provides configuration for IdentityServer.
    /// </summary>
    public static class IdentityServerConfig
    {
        /// <summary>
        /// The API name for IdentityServer.
        /// </summary>
        public const string apiName = "vcfapi";

        /// <summary>
        /// The API client name for IdentityServer.
        /// </summary>
        public const string apiClientName = "api.client";

        /// <summary>
        /// The default MVC client name for IdentityServer.
        /// </summary>
        public const string mvcClientName = "mvc.client";

        /// <summary>
        /// The default Vue client name for IdentityServer.
        /// </summary>
        public const string vueClientName = "vue.client";

        /// <summary>
        /// Obtains a list of <see cref="ApiResource"/> objects for IdentityServer.
        /// </summary>
        /// <returns>A list of <see cref="ApiResource"/> objects for IdentityServer.</returns>
        public static IEnumerable<ApiResource> GetApiResources(string secret)
            => new List<ApiResource>
            {
                new ApiResource
                {
                    Name = apiName,

                    ApiSecrets = { new Secret(secret.Sha256()) },

                    UserClaims = { JwtClaimTypes.Email }
                }
            };

        /// <summary>
        /// Obtains a list of <see cref="Client"/> objects for IdentityServer.
        /// </summary>
        /// <returns>A list of <see cref="Client"/> objects for IdentityServer.</returns>
        public static IEnumerable<Client> GetClients(string secret)
            => new List<Client>
            {
                new Client
                {
                    ClientId = apiClientName,
                    ClientName = "API Client",
                    RequireConsent = false,

                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets = { new Secret(secret.Sha256()) },

                    AllowedCorsOrigins = { URLs.ApiURL.Remove(URLs.ApiURL.Length - 1) },

                    AllowedScopes = { apiName }
                },
                new Client
                {
                    ClientId = mvcClientName,
                    ClientName = "MVC Client",
                    RequireConsent = false,

                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                    ClientSecrets = { new Secret(secret.Sha256()) },

                    RedirectUris = { $"{URLs.ClientURL}signin-oidc" },
                    PostLogoutRedirectUris = { $"{URLs.ClientURL}signout-callback-oidc" },
                    AllowedCorsOrigins = { URLs.ClientURL.Remove(URLs.ClientURL.Length - 1) },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Profile,
                        apiName
                    },
                    AllowOfflineAccess = true
                },
                new Client
                {
                    ClientId = vueClientName,
                    ClientName = "Vue Client",
                    ClientUri = URLs.ClientURL,
                    RequireConsent = false,

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{URLs.ClientURL}oidc/callback" },
                    PostLogoutRedirectUris = { URLs.ClientURL },
                    AllowedCorsOrigins = { URLs.ClientURL.Remove(URLs.ClientURL.Length - 1) },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Profile,
                        apiName
                    }
                }
            };

        /// <summary>
        /// Obtains a list of <see cref="IdentityResource"/> objects for IdentityServer.
        /// </summary>
        /// <returns>A list of <see cref="IdentityResource"/> objects for IdentityServer.</returns>
        public static IEnumerable<IdentityResource> GetIdentityResources()
            => new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Profile(),
            };
    }
}
