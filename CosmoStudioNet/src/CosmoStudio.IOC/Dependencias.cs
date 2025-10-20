using CosmoStudio.BLL.Kokoro;
using CosmoStudio.BLL.Ollama;
using CosmoStudio.BLL.Servicios.Implementaciones;
using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.BLL.StableDifussion;
using CosmoStudio.Common;
using CosmoStudio.Infraestructura.DAL.Repos.Implementaciones;
using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Infraestructura.DAL.Scaffolding;
using CosmoStudio.Infraestructura.Files;
using CosmoStudio.Infraestructura.LLM;
using CosmoStudio.Infraestructura.SD;
using CosmoStudio.Infraestructura.TTS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;



namespace CosmoStudio.IOC
{
    public static class Dependencias
    {

        public static void InyectarDependencias(this IServiceCollection servicios, IConfiguration config)
        {
            // CONTEXT DAL
            servicios.AddDbContext<CosmoDbContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("sqlConnection"));
            });
            
            servicios.Configure<StorageOptions>(config.GetSection("Storage"));
            servicios.Configure<OllamaOptions>(config.GetSection("Ollama"));
            servicios.Configure<KokoroOptions>(config.GetSection("Kokoro"));
            servicios.Configure<StableDiffusionOptions>(config.GetSection("StableDiffusion"));

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()  
                .Or<TaskCanceledException>() 
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5)
                });

            servicios.AddHttpClient<IOllamaClient, OllamaClient>((sp, http) =>
            {
                var opt = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
                http.BaseAddress = new Uri(opt.BaseUrl);
                http.Timeout = TimeSpan.FromMinutes(20); 
            }).AddPolicyHandler(retryPolicy);       

            servicios.AddHttpClient<IkokoroClient, KokoroClient>((sp, http) =>
            {
                var opt = sp.GetRequiredService<IOptions<KokoroOptions>>().Value;
                http.BaseAddress = new Uri(opt.BaseUrl);
                http.Timeout = TimeSpan.FromMinutes(20);
            }).AddPolicyHandler(retryPolicy);
                        
            servicios.AddHttpClient<IStableDifusionClient, StableDifusionClient>((sp, http) =>
            {
                var opt = sp.GetRequiredService<IOptions<StableDiffusionOptions>>().Value;
                http.BaseAddress = new Uri(opt.BaseUrl);
                http.Timeout = TimeSpan.FromMinutes(5);
            });


            servicios.AddScoped<IFileStorage, LocalFileStorage>();
            servicios.AddScoped<IProyectoRepositorio, ProyectoRepositorio>();
            servicios.AddScoped<IGuionRepositorio, GuionRepositorio>();
            servicios.AddScoped<IRecursoRepositorio, RecursoRepositorio>();
            servicios.AddScoped<ITareaRenderRepositorio, TareaRenderRepositorio>();
            servicios.AddScoped<ILogRepositorio, LogRepositorio>();

            servicios.AddScoped<IProyectoServicio, ProyectoServicio>();
            servicios.AddScoped<IGuionServicio, GuionServicio>();
            servicios.AddScoped<IRecursoServicio, RecursoServicio>();
            servicios.AddScoped<IRenderServicio, RenderServicio>();
            servicios.AddScoped<IVozServicio, VozServicio>();
            servicios.AddScoped<IImagenService, ImagenService>();
        }
    }
}
