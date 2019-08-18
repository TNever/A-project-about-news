using System;
using System.Collections.Generic;
using System.Text;

namespace NewsPublish.Model.Response
{
    // 返回的统一格式
    public class ResponseModel
    {
        public int code { get; set; }
        public string result { get; set; }
        public dynamic data { get; set; }
    }
}
