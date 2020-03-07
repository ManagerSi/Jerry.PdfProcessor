using System;
using System.Collections.Generic;
using System.Text;

namespace Jerry.Model
{
    public class BaseResponse
    {
        public ResponseHeader Header { get; set; }
    }

    public class ResponseHeader
    {
        public Guid ResponseId { get; set; }
        public int StatusCode { get; set; }
        public int SubStatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}
