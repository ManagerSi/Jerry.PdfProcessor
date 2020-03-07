using System;
using System.Collections.Generic;
using System.Text;

namespace Jerry.Model
{
    public class BaseRequest
    {
        public string ProgramCode { get; set; }

        public RequestHeader Header { get; set; }
    }

    public class RequestHeader
    {
        public Guid RequestId { get; set; }
        public Dictionary<string,string> Options { get; set; }
    }
}
