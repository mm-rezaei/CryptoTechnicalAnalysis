using SuperSocket.SocketBase.Logging;
using TechnicalAnalysisTools.Delegates;

namespace TechnicalAnalysisTools.Auxiliaries
{
    public class ServerLogFactoryAuxiliary : ILogFactory
    {
        public ServerLogFactoryAuxiliary(LogReceivedHandler logReceivedHandler)
        {
            LogReceived = logReceivedHandler;
        }

        private LogReceivedHandler LogReceived { get; }

        public ILog GetLog(string name)
        {
            var result = new ServerLogAuxiliary(name);

            result.LogReceived += LogReceived;

            return result;
        }
    }
}
