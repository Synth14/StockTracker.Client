using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using StockTracker.Client.Services;
using MudBlazor.Services;
using System.Security.Authentication;
using Microsoft.IdentityModel.Logging;
using System.Net.Http;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.JsonWebTokens;

namespace StockTracker.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
     .AddCookie()
     .AddOpenIdConnect(options =>
     {
         options.Authority = "https://localhost:7031";
         options.ClientId = "A7B29F3D12E654C8F0932167D4E8A0B1";
         options.ResponseType = OpenIdConnectResponseType.Code;
         options.ResponseMode = OpenIdConnectResponseMode.FormPost;
         options.SaveTokens = true;
         options.GetClaimsFromUserInfoEndpoint = true;
         options.RequireHttpsMetadata = false; // For development only
         options.UsePkce = true;

         options.CallbackPath = "/signin-oidc";
         options.SignedOutCallbackPath = "/signout-callback-oidc";

         options.Scope.Clear();
         options.Scope.Add("openid");
         options.Scope.Add("profile");
         options.Scope.Add("email");
         options.Scope.Add("inventory.read");
         options.Scope.Add("inventory.delete");
         options.Scope.Add("inventory.update");
         options.Scope.Add("inventory.create");

         options.TokenValidationParameters = new TokenValidationParameters
         {
             ValidateIssuerSigningKey = true,
             ValidateIssuer = true,
             ValidIssuer = "https://localhost:7031",
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
                 if (context.SecurityToken is SecurityToken securityToken)
                 {
                     Console.WriteLine($"Token type: {securityToken.GetType().Name}");
                     if (securityToken is JsonWebToken jsonWebToken)
                     {
                         Console.WriteLine($"Token claims: {string.Join(", ", jsonWebToken.Claims.Select(c => $"{c.Type}={c.Value}"))}");
                     }
                     else if (securityToken is JwtSecurityToken jwtSecurityToken)
                     {
                         Console.WriteLine($"Token claims: {string.Join(", ", jwtSecurityToken.Claims.Select(c => $"{c.Type}={c.Value}"))}");
                     }
                 }
                 else
                 {
                     Console.WriteLine($"Unexpected token type: {context.SecurityToken?.GetType().Name ?? "null"}");
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

            // Configure HttpClient for API
            builder.Services.AddHttpClient("API", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7141/");
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
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
                    builder => builder.WithOrigins("https://localhost:7031")
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