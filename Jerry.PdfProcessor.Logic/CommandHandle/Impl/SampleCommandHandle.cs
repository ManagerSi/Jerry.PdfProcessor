using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Jerry.Common.Interface;
using Jerry.Model;
using Newtonsoft.Json;

namespace Jerry.PdfProcessor.Logic.CommandHandle.Impl
{
    public class SampleCommandHandle:BaseCommandHandle<BaseRequest,BaseResponse>
    {
        internal override ILogManager _logManager { get; set; }

        public SampleCommandHandle(ILogManager logManager)
        {
            _logManager = logManager;
        }
        public override async Task<BaseResponse> DoBusinessLogic(BaseRequest request)
        {
            _logManager.Info($"{nameof(SampleCommandHandle) } DoBusinessLogic");
            var res = await Task.FromResult(new BaseResponse()
            {
                Header = new ResponseHeader()
                {
                    ResponseId = request.Header.RequestId,
                    StatusCode = (int)HttpStatusCode.OK
                }
            }).ConfigureAwait(false);
            return res;
        }
    }
}
