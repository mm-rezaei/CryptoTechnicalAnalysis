using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace TechnicalAnalysisTools.Delegates
{
    public delegate void ServerRequestHandler<TAppSession, TRequestInfo>(TAppSession session, TRequestInfo requestInfo) where TAppSession : IAppSession where TRequestInfo : IRequestInfo;
}
