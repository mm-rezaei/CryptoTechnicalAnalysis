using SuperSocket.ProtoBase;

namespace TechnicalAnalysisTools.Ui.Win.DataObjects
{
    internal class RequestDataObject : IPackageInfo
    {
        public RequestDataObject(byte[] inBody)
        {
            Body = inBody;
        }

        public byte[] Body { get; private set; }
    }
}
