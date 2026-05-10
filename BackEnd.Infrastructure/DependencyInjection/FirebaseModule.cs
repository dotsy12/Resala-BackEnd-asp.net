using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackEnd.Infrastructure.DependencyInjection
{
    public static class FirebaseModule
    {
        public static IServiceCollection AddFirebase(this IServiceCollection services, IConfiguration configuration)
        {
            var configPath = configuration["Firebase:CredentialsPath"];
            
            if (FirebaseApp.DefaultInstance == null)
            {
                if (string.IsNullOrEmpty(configPath))
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.GetApplicationDefault(),
                    });
                }
                else
                {
                    // Fix: Ensure path works in production (relative to base directory)
                    // If the path in config is "BackEnd.Infrastructure/Secrets/...", we might need to be careful.
                    // Usually in production, secrets are in a known location or relative to the app.
                    
                    string fullPath = Path.IsPathRooted(configPath) 
                        ? configPath 
                        : Path.Combine(AppContext.BaseDirectory, configPath);

                    // Fallback search for development environments
                    if (!File.Exists(fullPath))
                    {
                        var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
                        while (currentDir != null && !File.Exists(fullPath))
                        {
                            var candidatePath = Path.Combine(currentDir.FullName, configPath);
                            if (File.Exists(candidatePath))
                            {
                                fullPath = candidatePath;
                                break;
                            }
                            currentDir = currentDir.Parent;
                        }
                    }

                    if (File.Exists(fullPath))
                    {
                        FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile(fullPath),
                        });
                    }
                    else
                    {
                        // Fail-fast if credentials are expected but missing
                        throw new FileNotFoundException($"Firebase credentials file not found at: {fullPath}");
                    }
                }
            }

            return services;
        }
    }
}
