using CosmoStudio.Common.Requests;

namespace CosmoStudio.Common.Interfaces;

public interface IScriptGenOptionsProvider
{
    OllamaScriptGenRequest Get(OllamaMode mode);
}
