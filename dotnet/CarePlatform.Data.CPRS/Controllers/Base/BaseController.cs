using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    [SessionFilter]
    public class BaseController : ECSecureApiController
    {
        public ISession Session { get; set; }

        public BaseController() { }

        public BaseController(ISession Session) {
            this.Session = Session;
        }

        [NonAction]
        public async Task<string> getVariableValue(string arg)
        {
            VistaQuery request = buildGetVariableValueRequest(arg);
            string response = "";
            try
            {
                response = await this.Session.query(request);
                return response;
            }
            catch (Exception exc)
            {
                throw new ApplicationException(string.Format("Query exception: {0} - {1} - {2}",request, response, exc));
            }
        }

        [NonAction]
        public static VistaQuery buildGetVariableValueRequest(string arg)
        {
            if (String.IsNullOrEmpty(arg))
            {
                throw new ArgumentException("Argument is null");
            }
            VistaQuery vq = new VistaQuery("XWB GET VARIABLE VALUE");
            vq.addParameter(VistaQuery.REFERENCE, arg);
            return vq;
        }

        [NonAction]
        internal async Task<string> getCurrentTime()
        {
            var vq = new VistaQuery("ORWU DT");
            vq.addParameter(VistaQuery.LITERAL, "NOW");
            return await this.Session.query(vq);

        }

    }
}
