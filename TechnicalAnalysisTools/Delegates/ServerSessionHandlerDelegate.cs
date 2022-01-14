using SuperSocket.SocketBase;

namespace TechnicalAnalysisTools.Delegates
{
    public delegate void ServerSessionHandler<TAppSession>(TAppSession session) where TAppSession : IAppSession;

    public delegate void ServerSessionHandler<TAppSession, TParam>(TAppSession session, TParam value) where TAppSession : IAppSession;
}
