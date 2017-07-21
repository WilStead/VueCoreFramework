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
        /// The default Vue client name for IdentityServer.
        /// </summary>
        public const string vueClientName = "vue.client";

        /// <summary>
        /// Obtains a list of <see cref="IdentityResource"/> objects for IdentityServer.
        /// </summary>
        /// <returns>A list of <see cref="IdentityResource"/> objects for IdentityServer.</returns>
        public static IEnumerable<IdentityResource> GetIdentityResources()
            => new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Profile()
            };

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

                    Scopes = { new Scope() { Name = apiName, DisplayName = "VueCoreFramework API" } }
                }
            };

        /// <summary>
        /// Obtains a list of <see cref="Client"/> objects for IdentityServer.
        /// </summary>
        /// <returns>A list of <see cref="Client"/> objects for IdentityServer.</returns>
        public static IEnumerable<Client> GetClients(string secret, URLOptions urls)
            => new List<Client>
            {
                new Client
                {
                    ClientId = apiClientName,
                    ClientName = "API Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets = { new Secret(secret.Sha256()) },

                    AllowedCorsOrigins = { urls.ApiURL.TrimEnd('/') },

                    AllowedScopes = { apiName }
                },
                new Client
                {
                    ClientId = vueClientName,
                    ClientName = "VueCoreFramework Client",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,

                    RedirectUris = { $"{urls.ClientURL}oidc/callback" },
                    PostLogoutRedirectUris = { urls.ClientURL },
                    AllowedCorsOrigins = { urls.ClientURL.TrimEnd('/') },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Profile,
                        apiName
                    }
                }
            };
    }
}
