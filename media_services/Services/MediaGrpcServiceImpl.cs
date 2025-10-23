using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using media_services.Contracts;
using MediaProto;
using Microsoft.AspNetCore.Mvc.Formatters;

public sealed class MediaGrpcServiceImpl : MediaGrpcService.MediaGrpcServiceBase
{
    private readonly IMediaService _repo; 

    public MediaGrpcServiceImpl(IMediaService repo) => _repo = repo;

    public override async Task<GetByAssetIdReply> GetByAssetIdGrpc(GetByAssetIdRequest request, ServerCallContext context)
    {
        var rows = await _repo.GetByAssetIdAsync(request.AssetId);
        try
        {
            var reply = new GetByAssetIdReply();
            foreach (var m in rows)
            {
                reply.Items.Add(new MediaItem
                {
                    MediaId = m.MediaId.ToString(),
                    AssetId = m.AssetId,
                    MediaUrl = m.MediaUrl,
                    MediaType = m.MediaType,
                    CreateAt = Timestamp.FromDateTime(m.CreateAt.ToUniversalTime()),  // UTC
                    Size = (ulong)m.Size,
                    ObjectKey = m.ObjectKey
                });
            }
            return reply;
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
            return new GetByAssetIdReply();
        }
    }
}
