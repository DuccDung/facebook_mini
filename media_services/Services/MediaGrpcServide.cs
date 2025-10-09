using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using media_services.Contracts;
using MediaProto;
using Microsoft.AspNetCore.Mvc.Formatters;

public sealed class MediaGrpcServiceImpl : MediaGrpcService.MediaGrpcServiceBase
{
    private readonly IMediaService _repo; // tự thay bằng service của bạn

    public MediaGrpcServiceImpl(IMediaService repo) => _repo = repo;

    public override async Task<GetByAssetIdReply> GetByAssetId(
        GetByAssetIdRequest request, ServerCallContext context)
    {
        var rows = await _repo.GetByAssetIdAsync(request.AssetId, context.CancellationToken);

        var reply = new GetByAssetIdReply();
        foreach (var m in rows)
        {
            reply.Items.Add(new MediaItem
            {
                MediaId = m.MediaId.ToString(),
                AssetId = m.AssetId,
                MediaUrl = m.MediaUrl,
                MediaType = m.MediaType,
                CreateAt = Timestamp.FromDateTime(m.CreateAt), // UTC
                Size = (ulong)m.Size,
                ObjectKey = m.ObjectKey
            });
        }
        return reply;
    }
}
