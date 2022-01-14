using SuperSocket.SocketBase.Protocol;

namespace TechnicalAnalysisTools.DataObjects
{
    public class RequestDataObject : IRequestInfo
    {
        public RequestDataObject(byte[] inBody) : this(string.Empty, inBody)
        {

        }

        public RequestDataObject(string inKey, byte[] inBody)
        {
            Key = inKey;

            Body = inBody;
        }

        public string Key { set; get; }

        public byte[] Body { get; private set; }
    }
}
