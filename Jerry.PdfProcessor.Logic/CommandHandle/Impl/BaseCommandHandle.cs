using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Jerry.Common.Interface;
using Jerry.Model;
using Newtonsoft.Json;

namespace Jerry.PdfProcessor.Logic.CommandHandle.Impl
{
    public abstract class BaseCommandHandle<TRequest,TResponse>:ICommandHandle 
        where TRequest:BaseRequest
        where TResponse:BaseResponse
    {
        internal abstract ILogManager _logManager { get; set; }

        public async Task<byte[]> HandleAsync(byte[] requestBuffer)
        {
            TRequest request = null;
            JsonSerializer serializer=new JsonSerializer();
            using(MemoryStream ms = new MemoryStream(requestBuffer))
            using (StreamReader sr = new StreamReader(ms))
            {
                string requestStr = sr.ReadToEnd();
                using (StringReader str = new StringReader(requestStr))
                {
                    request = (TRequest)serializer.Deserialize(str, typeof(TRequest));
                    _logManager.Info("Request:" + JsonConvert.SerializeObject(requestStr));
                }
            }

            TResponse res = await DoBusinessLogic(request);
            _logManager.Info("Response:" + JsonConvert.SerializeObject(res));

            using (MemoryStream ms = new MemoryStream())
            using (StreamWriter sw = new StreamWriter(ms))
            {
                serializer.Serialize(sw,res);
                sw.Flush();
                var resBuffer = ms.ToArray();
                return resBuffer;
            }

        }

        public abstract Task<TResponse> DoBusinessLogic(TRequest request);
    }
}
