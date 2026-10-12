using FirebaseAdmin;
using FloraPointAPI.Services;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FloraPointAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string projectId = "florapoint-6390f";

            //inicjalizacja firebase Admin SDK
            var keyPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase-key.json");
            if (File.Exists(keyPath))
            {
                var credential = CredentialFactory.FromFile<ServiceAccountCredential>(keyPath).ToGoogleCredential();

                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential
                });
            }

            //rejestracja instancji bazy danych
            builder.Services.AddSingleton(provider =>
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);
                return FirestoreDb.Create(projectId);
            });

            //autentyfikacja
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://securetoken.google.com/{projectId}";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = $"https://securetoken.google.com/{projectId}",
                        ValidateAudience = true,
                        ValidAudience = projectId,
                        ValidateLifetime = true
                    };
                });

            //rejestracja serwisów
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IPlantService, PlantService>();
            builder.Services.AddScoped<IPhotoService, PhotoService>();
            builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();


            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}