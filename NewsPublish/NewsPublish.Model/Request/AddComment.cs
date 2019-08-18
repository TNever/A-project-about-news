using System;
using System.Collections.Generic;
using System.Text;

namespace NewsPublish.Model.Request
{
    public class AddComment
    {
        // 发评论也仅需要个ID和评论的内容即可
        public int NewsId { get; set; }
        public string Contents { get; set; }
    }
}