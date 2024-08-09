using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<CertificateValidationService>();

builder.Services.AddControllers();

builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate(options =>
    {
        //options.AllowedCertificateTypes = CertificateTypes.All;
        //options.ValidateCertificateUse = false;
        //options.ValidateValidityPeriod = false;
        options.RevocationMode = X509RevocationMode.NoCheck;
        options.ChainTrustValidationMode = X509ChainTrustMode.CustomRootTrust;
        options.CustomTrustStore = [];

        // Get all certificates and put them to custom trust store
        var files = Directory.GetFiles(
            @"C:\GitHub\JanneMattila\certificate-demos",
            "*RootCA.crt");

        foreach (var item in files)
        {
            options.CustomTrustStore.Add(new X509Certificate2(item));
        }

        options.Events = new CertificateAuthenticationEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Set breakpoint here and inspect context
                context.Fail("Failed to authenticate.");
                return Task.CompletedTask;
            },
            OnCertificateValidated = context =>
            {
                // Set breakpoint here and inspect context
                var validationService = context.HttpContext.RequestServices.GetRequiredService<CertificateValidationService>();

                if (validationService.ValidateCertificate(context.ClientCertificate))
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier,
                                context.ClientCertificate.Subject,
                                ClaimValueTypes.String, context.Options.ClaimsIssuer),
                        new Claim(ClaimTypes.Name,
                                context.ClientCertificate.Subject,
                                ClaimValueTypes.String, context.Options.ClaimsIssuer)
                    };

                    context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                    context.Success();
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.ConfigureHttpsDefaults(options =>
    {
        options.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
        options.ClientCertificateValidation = (certificate, chain, sslPolicyErrors) =>
        {
            // Set breakpoint here and inspect context
            return true;
        };
        //options.AllowAnyClientCertificate();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
