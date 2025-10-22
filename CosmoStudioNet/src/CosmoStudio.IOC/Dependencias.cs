using CosmoStudio.BLL.Clientes;
using CosmoStudio.BLL.Servicios.Implementaciones;
using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Common.Interfaces;
using CosmoStudio.Common.Opciones;
using CosmoStudio.Common.Providers;
using CosmoStudio.Common.Requests;
using CosmoStudio.Infraestructura.DAL.Repos.Implementaciones;
using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Infraestructura.DAL.Scaffolding;
using CosmoStudio.Infraestructura.Files;
using CosmoStudio.Infraestructura.LLM;
using CosmoStudio.Infraestructura.T2I;
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
            servicios.AddDbContext<CosmoDbContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("sqlConnection"));
            });

            servicios.InyectarOpcionesConfiguracion(config);

            servicios.InyectarClientesHttp();          

            servicios.InyectarRepositorios();

            servicios.InyectarServicios();
        }

        internal static void InyectarRepositorios(this IServiceCollection servicios)
        {
            servicios.AddScoped<ILocalFileStorage, LocalFileStorage>();
            servicios.AddScoped<IProyectoRepositorio, ProyectoRepositorio>();
            servicios.AddScoped<IGuionRepositorio, GuionRepositorio>();
            servicios.AddScoped<IRecursoRepositorio, RecursoRepositorio>();
            servicios.AddScoped<ITareaRenderRepositorio, TareaRenderRepositorio>();
            servicios.AddScoped<ILogRepositorio, LogRepositorio>();
            servicios.AddScoped<IGuionAudioRepositorio, GuionAudioRepositorio>();
            servicios.AddScoped<IGuionImagenRepositorio, GuionImagenRepositorio>();
            servicios.AddScoped<IGuionVersionRepositorio, GuionVersionRepositorio>();
        }

        internal static void InyectarServicios(this IServiceCollection servicios)
        {
            servicios.AddScoped<IProyectoServicio, ProyectoServicio>();
            servicios.AddScoped<IGuionServicio, GuionServicio>();
            servicios.AddScoped<IRecursoServicio, RecursoServicio>();
            servicios.AddScoped<IRenderServicio, RenderServicio>();
            servicios.AddScoped<IAudioServicio, AudioServicio>();
            servicios.AddScoped<IImagenServicio, ImagenServicio>();
        }

        internal static void InyectarOpcionesConfiguracion(this IServiceCollection servicios, IConfiguration config)
        {
            servicios.Configure<StorageOptions>(config.GetSection("Storage"));            
            servicios.Configure<KokoroOptions>(config.GetSection("Kokoro"));
            servicios.Configure<StableDiffusionOptions>(config.GetSection("StableDiffusion"));

            servicios.Configure<OllamaOptions>(config.GetSection("Ollama"));
            servicios.AddOptions<OllamaScriptGenRequest>("Borrador")
                .Bind(config.GetSection("ScriptGen:Borrador"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            servicios.AddOptions<OllamaScriptGenRequest>("Produccion")
                .Bind(config.GetSection("ScriptGen:Produccion"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            servicios.AddSingleton<IScriptGenOptionsProvider, ScriptGenOptionsProvider>();
        }

        internal static void InyectarClientesHttp(this IServiceCollection servicios)
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                [
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5)
                ]);

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
        }






    }
}
