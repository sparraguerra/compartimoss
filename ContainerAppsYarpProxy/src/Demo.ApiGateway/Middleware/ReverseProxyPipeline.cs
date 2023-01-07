namespace Demo.ApiGateway.Middleware;

public static class ReverseProxyPipeline
{
    public static void UseReverseProxyPipeline(this IReverseProxyApplicationBuilder pipeline)
    {
        pipeline.UseCors("CorsPolicy");
        pipeline.UseSessionAffinity();
        pipeline.UseLoadBalancing();
        pipeline.UsePassiveHealthChecks();
    }
}
