
using CosmoStudio.Common.Requests;
using CosmoStudio.Common.Responses;

namespace CosmoStudio.BLL.Clientes;

public interface IStableDifusionClient
{       
    Task<Txt2ImgResponse> Txt2ImgAsync(Txt2ImgRequest req, CancellationToken ct = default);      
}
