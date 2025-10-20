using CosmoStudio.BLL.Ollama;
using CosmoStudio.Common;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CosmoStudio.Infraestructura.LLM;



public class OllamaClient : IOllamaClient
{
    private readonly HttpClient _http;
    private readonly OllamaOptions _opt;

    public OllamaClient(HttpClient http, IOptions<OllamaOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
        
    }

    public async Task<string> GenerateAsync(string? model, string prompt, bool stream, CancellationToken ct)
    {
        var body = new
        {
            model = string.IsNullOrWhiteSpace(model) ? _opt.DefaultModel : model,
            prompt,
            stream,
            options = new { 
                temperature = _opt.Temperature ,
                num_ctx = 8192,
                top_p = 0.9,
                repeat_penalty = 1.1
            }
        };

        try
        {
            using var resp = await _http.PostAsJsonAsync("/api/generate", body, cancellationToken: ct);
            resp.EnsureSuccessStatusCode();

            // Cuando stream=false, la respuesta trae { "response": "..." }
            var json = await resp.Content.ReadAsStringAsync(ct);

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("response", out var responseProp))
                return responseProp.GetString() ?? string.Empty;

            // Fallback por si Ollama cambia el shape
            return json;
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            // Timeout (ya hay reintentos por Polly; esto es el mensaje final)
            throw new TimeoutException("La solicitud a Ollama ha superado el tiempo de espera.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("No se pudo contactar con Ollama en local. Verifica que el servicio esté en http://localhost:11434 y que el modelo esté cargado.", ex);
        }
    }

    public Task<string> GenerateOutlineAsync(string tema, ScriptGenOptions opt, CancellationToken ct)
    {
        var model = string.IsNullOrWhiteSpace(opt.Modelo) ? _opt.DefaultModel : opt.Modelo!;
        var prompt = BuildOutlinePrompt(tema, opt);
        return GenerateAsync(model, prompt, stream: false, ct);
    }

    public Task<string> GenerateScriptFromOutlineAsync(string outline, ScriptGenOptions opt, CancellationToken ct)
    {
        var model = string.IsNullOrWhiteSpace(opt.Modelo) ? _opt.DefaultModel : opt.Modelo!;
        var prompt = BuildScriptPromptFromOutline(outline, opt);
        return GenerateAsync(model, prompt, stream: false, ct);
    }

    public Task<string> GenerateSectionAsync(string sectionTitle, IReadOnlyList<string> bullets, int targetWords, ScriptGenOptions opt, CancellationToken ct)
    {
        var model = string.IsNullOrWhiteSpace(opt.Modelo) ? _opt.DefaultModel : opt.Modelo!;
        var prompt = BuildSectionPrompt(sectionTitle, bullets, targetWords, opt);
        return GenerateAsync(model, prompt, stream: false, ct);
    }

  
    // ---------- Prompt builders ----------
    private static string BuildOutlinePrompt(string tema, ScriptGenOptions opt)
    {
        // calcula secciones por modo
        var secciones = opt.Mode == ScriptMode.Produccion ? Math.Max(opt.Secciones, 40) : Math.Max(opt.Secciones, 10);
        var minutos = Math.Max(opt.MinutosObjetivo, 10);
        var estilo = string.IsNullOrWhiteSpace(opt.Estilo) ? "tono calmado y divulgativo" : opt.Estilo;

        var sb = new StringBuilder();
        sb.AppendLine($"Genera un OUTLINE muy detallado en español, {estilo}.");
        sb.AppendLine($"Tema: \"{tema}\"");
        sb.AppendLine($"Duración objetivo: ~{minutos} minutos.");
        sb.AppendLine();
        sb.AppendLine("Requisitos:");
        sb.AppendLine($"- Divide en {secciones} secciones numeradas (1..{secciones}), de ~{minutos / secciones:0} minutos cada una.");
        sb.AppendLine("- Cada sección debe tener un título y 2-3 bullets con subtemas (no guion completo aún).");
        sb.AppendLine("- Progresión lógica: introducción, desarrollo, ejemplos/metáforas, cierre.");
        sb.AppendLine("- Sé coherente y evita saltos temáticos.");
        sb.AppendLine();
        sb.AppendLine("Formato EXACTO:");
        sb.AppendLine("Título del episodio");
        sb.AppendLine();
        sb.AppendLine("1) Título de sección");
        sb.AppendLine("- punto 1");
        sb.AppendLine("- punto 2");
        sb.AppendLine("- punto 3");
        sb.AppendLine("2) Título de sección");
        sb.AppendLine("- ...");
        sb.AppendLine("...");
        sb.AppendLine();
        sb.AppendLine("Cierra con 5-8 FUENTES divulgativas.");

        return sb.ToString();
    }

    private static string BuildScriptPromptFromOutline(string outline, ScriptGenOptions opt)
    {
        var minutos = Math.Max(opt.MinutosObjetivo, 10);
        var wpm = Math.Clamp(opt.PalabrasPorMinuto, 90, 180);
        var targetWords = minutos * wpm;
        var parrafos = opt.Mode == ScriptMode.Produccion ? Math.Max(opt.ParrafosPorSeccion, 2) : Math.Max(opt.ParrafosPorSeccion, 1);
        var estilo = string.IsNullOrWhiteSpace(opt.Estilo) ? "tono calmado, claro, pausado" : opt.Estilo;

        return $@"
A partir del OUTLINE proporcionado, escribe el GUIÓN COMPLETO en español, {estilo}.
Duración objetivo: ~{minutos} minutos (~{targetWords} palabras aprox).

OUTLINE:
<<OUTLINE>>
{outline}
<<FIN OUTLINE>>

Reglas:
- Genera {parrafos} párrafos por sección, con frases suaves y transiciones.
- Inserta marcas de tiempo aproximadas [mm:00] cada minuto.
- Añade (pausa breve) donde convenga para respiración del oyente.
- Evita tecnicismos innecesarios; explica con metáforas cuando ayude.
- Cierra con resumen y despedida muy suave.
";
    }

    private static string BuildSectionPrompt(string title, IReadOnlyList<string> bullets, int targetWords, ScriptGenOptions opt)
    {
        var bulletsText = string.Join(Environment.NewLine, bullets.Select(b => $"- {b}"));
        var estilo = string.IsNullOrWhiteSpace(opt.Estilo) ? "tono calmado, claro y pausado (para dormir)" : opt.Estilo;

        return $@"
Escribe la sección titulada: ""{title}"" en español, {estilo}.
Objetivo: ~{targetWords} palabras (no menos de {(int)(targetWords * 0.9)}).
Usa 2–3 párrafos con transiciones suaves. Inserta (pausa breve) donde convenga.

Puntos a cubrir:
{bulletsText}

Reglas:
- Explica con metáforas sencillas cuando ayude.
- Evita tecnicismos innecesarios; si aparecen, explícalos.
- Mantén coherencia con una narración relajada (estilo documental).
- No introduzcas temas fuera de los bullets.
- No repitas texto de otras secciones.

Devuelve SOLO el texto de la sección, sin títulos adicionales ni secciones nuevas.
";
    }

}

