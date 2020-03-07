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
    public class InvalidMessageCommandHandle : BaseCommandHandle<BaseRequest,BaseResponse>
    {
        public InvalidMessageCommandHandle(ILogManager logManager)
        {
            _logManager = logManager;
        }

        internal override ILogManager _logManager { get; set; }

        public override async Task<BaseResponse> DoBusinessLogic(BaseRequest request)
        {
            _logManager.Info($"{nameof(InvalidMessageCommandHandle) } DoBusinessLogic");
            var res = await Task.FromResult(new BaseResponse()
            {
                Header = new ResponseHeader()
                {
                    ResponseId = request.Header.RequestId,
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "type 不正确",
                    Details = "未能找到CommandHandle"
                }
            }).ConfigureAwait(false);
            return res;
        }
    }
}
