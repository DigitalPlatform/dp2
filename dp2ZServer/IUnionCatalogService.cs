using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace dp2ZServer
{
    [ServiceContract(
        Name = "UnionCatalogService",
        Namespace = "http://dp2003.com/unioncatalog/",
        SessionMode = SessionMode.NotAllowed)]
    public interface IUnionCatalogService
    {
        [OperationContract()]
        int UpdateRecord(
    string strAuthString,
    string strAction,
    string strRecPath,
    string strFormat,
    string strRecord,
    string strTimestamp,
            out string strOutputRecPath,
            out string strOutputTimestamp,
            out string strError);
    }

}
