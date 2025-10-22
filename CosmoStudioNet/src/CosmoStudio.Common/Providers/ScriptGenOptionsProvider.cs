using CosmoStudio.Common.Interfaces;
using CosmoStudio.Common.Requests;
using Microsoft.Extensions.Options;

namespace CosmoStudio.Common.Providers
{

    public sealed class ScriptGenOptionsProvider : IScriptGenOptionsProvider
    {
        private readonly IOptionsMonitor<OllamaScriptGenRequest> _named;

        public ScriptGenOptionsProvider(IOptionsMonitor<OllamaScriptGenRequest> named)
            => _named = named;

        public OllamaScriptGenRequest Get(OllamaMode mode)
            => _named.Get(mode.ToString()); // "Borrador" o "Produccion"
    }
}
