using System;
using System.Collections.Generic;
using System.Text;

namespace NewsPublish.Model.Request
{
    // 添加新闻类别
    public class AddNewsClassify
    {
        // 类别的名称、排序及备注
        public string Name { get; set; }
        public int Sort { get; set; }
        public string Remark { get; set; }
    }
}
