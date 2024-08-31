using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using StockTracker.Client.Services;
using StockTracker.Client.Services.AuthService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;

namespace StockTracker.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;


            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
            builder.Services.AddSingleton<CookieOidcRefresher>();
            // Configure logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Trace);

            // Add services to the container
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Configure Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = async context =>
                    {
                        var refresher = context.HttpContext.RequestServices.GetRequiredService<CookieOidcRefresher>();
                        await refresher.ValidateOrRefreshCookieAsync(context, OpenIdConnectDefaults.AuthenticationScheme);
                    }
                };
            })
             .AddOpenIdConnect(options =>
             {
                 options.Authority = configuration.GetSection("OpenIdConnect:Authority").Value;
                 options.ClientId = configuration.GetSection("OpenIdConnect:ClientId").Value;

                 options.ResponseType = OpenIdConnectResponseType.Code;
                 options.ResponseMode = OpenIdConnectResponseMode.FormPost;
                 options.SaveTokens = true;
                 options.GetClaimsFromUserInfoEndpoint = true;
                 options.RequireHttpsMetadata = false; 
                 options.UsePkce = true;
                 options.CallbackPath = "/signin-oidc";
                 options.SignedOutCallbackPath = "/signout-callback-oidc";
                 options.RefreshInterval = TimeSpan.FromMinutes(30);
                 options.Scope.Clear();
                 foreach (var scope in configuration.GetSection("OpenIdConnect:Scopes").Get<List<string>>())
                 {
                     options.Scope.Add(scope);
                 }

                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuerSigningKey = true,
                     ValidateIssuer = true,
                     ValidIssuer = configuration.GetSection("OpenIdConnect:Authority").Value,
                     ValidateAudience = false,
                     ValidateLifetime = true,
                     ClockSkew = TimeSpan.Zero
                 };

                 // Configure key set retrieval
                 var httpClient = new HttpClient(new HttpClientHandler
                 {
                     ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                 });
                 options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                     "https://localhost:7031/.well-known/openid-configuration",
                     new OpenIdConnectConfigurationRetriever(),
                     new HttpDocumentRetriever(httpClient) { RequireHttps = false }
                 );

                 options.Events = new OpenIdConnectEvents
                 {
                     OnRedirectToIdentityProvider = context =>
                     {
                         Console.WriteLine($"Redirecting to: {context.ProtocolMessage.CreateAuthenticationRequestUrl()}");
                         return Task.CompletedTask;
                     },
                     OnAuthorizationCodeReceived = context =>
                     {
                         Console.WriteLine($"Authorization code received: {context.ProtocolMessage.Code}");
                         return Task.CompletedTask;
                     },
                     OnTokenValidated = async context =>
                     {
                         // Vérification essentielle pour .NET 8
                         if (context.SecurityToken is SecurityToken securityToken)
                         {
                             Console.WriteLine($"Token type: {securityToken.GetType().Name}");
                             if (securityToken is JsonWebToken jsonWebToken)
                             {
                                 Console.WriteLine($"Token claims: {string.Join(", ", jsonWebToken.Claims.Select(c => $"{c.Type}={c.Value}"))}");
                                 // Utilisez jsonWebToken pour extraire les informations nécessaires
                                 var accessToken = context.TokenEndpointResponse.AccessToken;
                                 var refreshToken = context.TokenEndpointResponse.RefreshToken;
                                 var expiresAt = DateTime.UtcNow.AddSeconds(jsonWebToken.ValidTo.Subtract(DateTime.UtcNow).TotalSeconds);


                                 var identity = (ClaimsIdentity)context.Principal.Identity;
                                 foreach (var claim in jsonWebToken.Claims)
                                 {
                                     identity.AddClaim(new Claim(claim.Type, claim.Value));
                                 }
                             }
                             else if (securityToken is JwtSecurityToken jwtSecurityToken)
                             {
                                 Console.WriteLine($"Token claims: {string.Join(", ", jwtSecurityToken.Claims.Select(c => $"{c.Type}={c.Value}"))}");
                                 // Utilisez jwtSecurityToken pour extraire les informations nécessaires
                                 var accessToken = context.TokenEndpointResponse.AccessToken;
                                 var refreshToken = context.TokenEndpointResponse.RefreshToken;
                                 var expiresAt = DateTime.UtcNow.AddSeconds(jwtSecurityToken.ValidTo.Subtract(DateTime.UtcNow).TotalSeconds);


                                 var identity = (ClaimsIdentity)context.Principal.Identity;
                                 foreach (var claim in jwtSecurityToken.Claims)
                                 {
                                     identity.AddClaim(new Claim(claim.Type, claim.Value));
                                 }
                             }
                         }
                         else
                         {
                             Console.WriteLine($"Unexpected token type: {context.SecurityToken?.GetType().Name ?? "null"}");
                             // Gérez ce cas inattendu, peut-être en lançant une exception ou en journalisant l'erreur
                         }
                     },
                     OnTokenResponseReceived = context =>
                     {
                         Console.WriteLine("Token response received.");
                         return Task.CompletedTask;
                     },
                     OnRemoteFailure = context =>
                     {
                         Console.WriteLine($"Remote failure: {context.Failure}");
                         if (context.Failure.InnerException != null)
                         {
                             Console.WriteLine($"Inner exception: {context.Failure.InnerException.Message}");
                         }
                         return Task.CompletedTask;
                     },
                     OnAuthenticationFailed = context =>
                     {
                         Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                         if (context.Exception.InnerException != null)
                         {
                             Console.WriteLine($"Inner exception: {context.Exception.InnerException.Message}");
                         }
                         return Task.CompletedTask;
                     }
                 };
             });

            // Configure Authorization
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("InventoryRead", policy => policy.RequireClaim("scope", "inventory.read"));
                options.AddPolicy("InventoryDelete", policy => policy.RequireClaim("scope", "inventory.delete"));
                options.AddPolicy("InventoryUpdate", policy => policy.RequireClaim("scope", "inventory.update"));
                options.AddPolicy("InventoryCreate", policy => policy.RequireClaim("scope", "inventory.create"));
            });
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<AuthenticationDelegatingHandler>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // Configure HttpClient for API
            builder.Services.AddHttpClient("API", client =>
            {
                client.BaseAddress = new Uri(configuration.GetSection("StockTracker.API:BaseURL").Value);
            })
            .AddHttpMessageHandler<AuthenticationDelegatingHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
            });

            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddMudServices();

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder.WithOrigins(configuration.GetSection("StockTracker.API:BaseURL").Value)
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());
            });

            var app = builder.Build();
            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true;
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("AllowSpecificOrigin");

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}