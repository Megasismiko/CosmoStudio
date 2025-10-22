using CosmoStudio.BLL.Clientes;
using CosmoStudio.Common;
using CosmoStudio.Common.Opciones;
using CosmoStudio.Common.Requests;
using CosmoStudio.Common.Responses;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace CosmoStudio.Infraestructura.LLM
{
    public sealed class OllamaClient : IOllamaClient
    {

        private readonly HttpClient _http;
        private readonly OllamaOptions _opt;

        private static readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        };

        // ========== Constructor ==========
        public OllamaClient(HttpClient http, IOptions<OllamaOptions> opciones)
        {
            _http = http;
            _opt = opciones.Value;
        }

        // ========== Métodos base (bajo nivel) ==========

        public async Task<string> GenerarTextoAsync(OllamaGenerateRequest solicitud, CancellationToken ct = default)
        {
            // POST directo a /api/generate (modo no streaming)
            using var response = await _http.PostAsJsonAsync("/api/generate", solicitud, _json, ct);
            response.EnsureSuccessStatusCode();

            // Deserializamos la respuesta JSON completa
            var json = await response.Content.ReadAsStringAsync(ct);

            try
            {
                var parsed = JsonSerializer.Deserialize<OllamaGenerateResponse>(json, _json);
                if (parsed is not null && !string.IsNullOrEmpty(parsed.Response))
                    return parsed.Response.Trim();
            }
            catch
            {
                // si no se puede deserializar, devolvemos el raw
            }

            return json.Trim();
        }

     
        // ========== Alto nivel: Outline ==========

        public async Task<string> GenerarTituloOutlineAsync(string tema, OllamaScriptGenRequest opciones, CancellationToken ct = default)
        {
            // Selección de modelo y parámetros base
            var modelo = SeleccionarModelo(opciones);
            var (numPredict, numCtx, numBatch) = Tuning(opciones);

            // Construimos el prompt
            var prompt = BuildPromptTituloOutline(tema, opciones);

            // Creamos la solicitud completa
            var req = new OllamaGenerateRequest
            {
                Model = modelo,
                Prompt = prompt,
                Stream = false,
                KeepAlive = 600,
                Options = new OllamaOptionsBlock
                {
                    NumPredict = numPredict,
                    NumCtx = numCtx,
                    NumBatch = numBatch,
                    Temperature = _opt.Temperature,
                    TopP = 0.9,
                    TopK = 40,
                    RepeatPenalty = 1.1
                }
            };

            // Llamada directa y limpieza del resultado
            var texto = await GenerarTextoAsync(req, ct);
            return texto.Trim();
        }

        public async Task<IReadOnlyList<string>> GenerarOutlineAsync(string tema, OllamaScriptGenRequest opciones, CancellationToken ct = default)
        {
            // Genera el outline completo de una vez (lista de secciones)
            var modelo = SeleccionarModelo(opciones);
            var (numPredict, numCtx, numBatch) = Tuning(opciones);
            var prompt = BuildPromptOutline(tema, opciones);

            var req = new OllamaGenerateRequest
            {
                Model = modelo,
                Prompt = prompt,
                Stream = false,
                KeepAlive = 600,
                Options = new OllamaOptionsBlock
                {
                    NumPredict = numPredict,
                    NumCtx = numCtx,
                    NumBatch = numBatch,
                    Temperature = _opt.Temperature,
                    TopP = 0.9,
                    TopK = 40,
                    RepeatPenalty = 1.1
                }
            };

            var texto = await GenerarTextoAsync(req, ct);

            // Parseamos el resultado a una lista de secciones
            // Ejemplo esperado: "1) Introducción\n2) Qué son los agujeros negros..."
            var lineas = texto.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                              .Select(l => l.Trim())
                              .Where(l => !string.IsNullOrWhiteSpace(l))
                              .ToList();

            return lineas;
        }

        public async Task<string> GenerarSeccionOutlineAsync(string tema, int indice, int total, OllamaScriptGenRequest opciones, CancellationToken ct = default)
        {
            var modelo = SeleccionarModelo(opciones);
            var (numPredict, numCtx, numBatch) = Tuning(opciones);
            var prompt = BuildPromptSeccionOutline(tema, indice, total, opciones);

            var req = new OllamaGenerateRequest
            {
                Model = modelo,
                Prompt = prompt,
                Stream = false,
                KeepAlive = 600,
                Options = new OllamaOptionsBlock
                {
                    NumPredict = numPredict,
                    NumCtx = numCtx,
                    NumBatch = numBatch,
                    Temperature = _opt.Temperature,
                    TopP = 0.9,
                    TopK = 40,
                    RepeatPenalty = 1.1
                }
            };

            var texto = await GenerarTextoAsync(req, ct);
            return texto.Trim();
        }


        // ========== Alto nivel: Guion por secciones ==========   
        public async Task<string> GenerarSeccionGuionAsync(string tituloSeccion, IReadOnlyList<string> bullets, int palabrasObjetivo, OllamaScriptGenRequest opciones, CancellationToken ct = default)
        {
            // Elegimos modelo y parámetros según modo (borrador/producción)
            var modelo = SeleccionarModelo(opciones);
            var (numPredict, numCtx, numBatch) = Tuning(opciones);

            // Creamos el prompt detallado
            var prompt = BuildPromptSeccionGuion(tituloSeccion, bullets, palabrasObjetivo, opciones);

            // Request completo
            var req = new OllamaGenerateRequest
            {
                Model = modelo,
                Prompt = prompt,
                Stream = false,
                KeepAlive = 600,
                Options = new OllamaOptionsBlock
                {
                    NumPredict = numPredict,
                    NumCtx = numCtx,
                    NumBatch = numBatch,
                    Temperature = _opt.Temperature,
                    TopP = 0.9,
                    TopK = 40,
                    RepeatPenalty = 1.1
                }
            };

            // Generamos el texto
            var texto = await GenerarTextoAsync(req, ct);
            return texto.Trim();
        }

        // ========== Alto nivel: Prompts de imagen ==========
        public async Task<string> GenerarPromptImagenDesdeSeccionAsync(string textoSeccion, CancellationToken ct = default)
        {
            // Modelo y prompt específico
            var modelo = _opt.DefaultModelDraft ?? "llama3.1:8b-instruct";
            var prompt = BuildPromptImagenDesdeSeccion(textoSeccion);

            var req = new OllamaGenerateRequest
            {
                Model = modelo,
                Prompt = prompt,
                Stream = false,
                KeepAlive = 600,
                Options = new OllamaOptionsBlock
                {
                    NumPredict = 400,
                    NumCtx = 1024,
                    NumBatch = 16,
                    Temperature = 0.6,
                    TopP = 0.9,
                    TopK = 40,
                    RepeatPenalty = 1.1
                }
            };

            var texto = await GenerarTextoAsync(req, ct);
            return texto.Trim();
        }

        // ========== Alto nivel: Revisión/Corrección de guion ==========
        public async Task<string> RevisarYCorregirGuionAsync(string guionMarkdown, string tema, CancellationToken ct = default)
        {
            var modelo = _opt.DefaultModel ?? "llama3.1:8b-instruct";
            var prompt = BuildPromptRevisionGuion(guionMarkdown, tema);

            var req = new OllamaGenerateRequest
            {
                Model = modelo,
                Prompt = prompt,
                Stream = false,
                KeepAlive = 600,
                Options = new OllamaOptionsBlock
                {
                    NumPredict = 2400,
                    NumCtx = 2048,
                    NumBatch = 32,
                    Temperature = 0.5,
                    TopP = 0.9,
                    TopK = 40,
                    RepeatPenalty = 1.1
                }
            };

            var texto = await GenerarTextoAsync(req, ct);
            return texto.Trim();
        }

        // ========== Alto nivel: traducir==========
        public async Task<string> TraducirAsync(string textoOrigen, string a = "Spanish (es-ES)", bool produccion = true, CancellationToken ct = default)
        {            
            var modelo = (produccion ? _opt.DefaultModel : _opt.DefaultModelDraft);

            var req = new OllamaGenerateRequest
            {
                Model = modelo,
                Prompt = BuildPromptTraducir(textoOrigen, a),
                Stream = false,
                KeepAlive = 600,
                Options = new OllamaOptionsBlock
                {
                    NumPredict = 2400,
                    NumCtx = 2048,
                    NumBatch = 32,
                    Temperature = 0.3, // más fiel
                    TopP = 0.9,
                    TopK = 40,
                    RepeatPenalty = 1.1
                }
            };

            var texto = await GenerarTextoAsync(req, ct);
            return texto.Trim();
        }

        // ========== Helpers privados ==========
        private string SeleccionarModelo(OllamaScriptGenRequest req)
        {
            // Si hay modos diferenciados, usa uno rápido para borradores y otro potente para producción.
            return req.Mode == OllamaMode.Borrador
                ? _opt.DefaultModelDraft ?? _opt.DefaultModel
                : _opt.DefaultModel;
        }

        private (int numPredict, int numCtx, int numBatch) Tuning(OllamaScriptGenRequest req)
        {
            // Ajustamos tokens generables según modo y longitud objetivo del guion.
            return req.Mode == OllamaMode.Borrador
                ? (numPredict: 220, numCtx: 1024, numBatch: 16)
                : (numPredict: 2400, numCtx: 2048, numBatch: 32);
        }


        // Construcción de prompts (outline, sección, imagen, revisión). Los iremos creando después.
        // TÍTULO (ahora en inglés)
        private string BuildPromptTituloOutline(string tema, OllamaScriptGenRequest req)
        {
            return $@"
You are a professional documentary writer for science communication.
Create a short, intriguing English title for a YouTube science episode.

Topic: {tema}

Rules:
- Natural, cinematic tone
- Max 12 words
- Output ONLY the title.";
        }

        // OUTLINE COMPLETO (en inglés)
        private string BuildPromptOutline(string tema, OllamaScriptGenRequest req)
        {
            var total = Math.Max(5, req.Secciones);
            return $@"
You are an expert script planner for educational science videos.
Write a numbered outline in English with {total} sections about:

Topic: {tema}

Each section must have:
- a short, descriptive title (one line)
- three concise bullet points

Output format:
1) Section title
- idea 1
- idea 2
- idea 3

2) Section title
- idea 1
- idea 2
- idea 3";
        }

        // SECCIÓN DEL OUTLINE (en inglés)
        private string BuildPromptSeccionOutline(string tema, int indice, int total, OllamaScriptGenRequest req)
        {
            return $@"
You are creating part {indice} of {total} of an English outline for a science documentary.
General topic: {tema}

Write a section title and three concise bullet points.
Format:
{indice}) [Section title]
- idea 1
- idea 2
- idea 3";
        }

        // SECCIÓN DE GUIÓN (en inglés)
        private string BuildPromptSeccionGuion(string titulo, IReadOnlyList<string> bullets, int palabrasObjetivo, OllamaScriptGenRequest req)
        {
            var points = string.Join("\n- ", bullets);
            var mode = req.Mode == OllamaMode.Borrador ? "draft" : "production";

            return $@"
You are writing an English voiceover script for a science video.
Mode: {mode}
Section title: {titulo}

Guidelines:
- Natural, cinematic narration in English
- Around {palabrasObjetivo} words
- Flowing paragraphs (no lists)
- Insert '(short pause)' where a change of idea happens
- Avoid hype; be clear and precise

Key points:
- {points}";
        }
        private string BuildPromptRevisionGuion(string guionMarkdown, string tema)
        {
            return $@"
You are an expert Spanish editor reviewing a science documentary script.
The topic is: {tema}

Tasks:
- Detect and correct characters or words from other languages.
- Fix grammatical or typographical errors.
- Remove meaningless or out-of-context phrases.
- Preserve the original structure and '(pausa breve)' markers.

Return the corrected script in Markdown, without additional commentary.

Script to review:
### START OF SCRIPT ###
{guionMarkdown}
### END OF SCRIPT ###";
        }
        private string BuildPromptImagenDesdeSeccion(string textoSeccion)
        {
            return $@"
You are a professional prompt engineer for Stable Diffusion XL.

Your task is to read the following Spanish text, which comes from a narrated science video section, 
and generate an English visual description suitable for creating a photorealistic image.

Focus only on **visual and physical elements** — avoid abstract ideas, emotions, or narration.
Use rich descriptive keywords related to **space, physics, astronomy, technology, or nature**.
Do not include text overlays or people talking.

Output rules:
- Write a single English sentence (max 60 words).
- Use commas to separate key concepts.
- Add photographic style modifiers (e.g., 'ultra realistic, detailed lighting, volumetric light, cinematic composition, 4k').
- Do NOT include quotation marks, commentary, or prefixes like 'Prompt:'.

Spanish source text:
###
{textoSeccion}
###";
        }
        private string BuildPromptTraducir(string sourceText, string targetLang)
        {
            return $@"
You are a professional literary translator specialized in voiceover scripts.
Translate the following text into {targetLang} with natural, fluent phrasing suitable for narration.
Preserve meaning and tone. Avoid literal calques. Keep markers like '(short pause)' or '(pausa breve)' if present.

Text to translate:
### START ###
{sourceText}
### END ###

Output: only the translated text.";
        }


    }
}


