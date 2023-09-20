using Grpc.Core.Interceptors;
using Grpc.Core;

namespace Redis.gRPCService.Interceptors
{
    public class ServerExceptionInterceptor : Interceptor
    {
        private readonly ILogger<ServerExceptionInterceptor> _logger;

        public ServerExceptionInterceptor(ILogger<ServerExceptionInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await continuation(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error thrown by {method}, Stack Trace: {stackTrace}", context.Method, ex.ToString());
                throw new RpcException(new Status(StatusCode.Internal, ex.ToString())); ;
            }
        }
    }
}