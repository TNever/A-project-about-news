using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsPublish.Model.Entity;
using NewsPublish.Model.Request;
using NewsPublish.Model.Response;
using NewsPublish.Service;

namespace NewsPublish.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NewsController : Controller
    {
        private NewsService _newsService;
        private IHostingEnvironment _host;
        public NewsController(NewsService newsService, IHostingEnvironment host)
        {
            this._newsService = newsService;
            this._host = host;
        }
        // 直接怼进视图 -- 获取新闻类别，试下一个小弹窗
        public ActionResult Index()
        {
            var newClassifys = _newsService.GetNewsClassifyList();
            return View(newClassifys);
        }

        // 获取新闻类别的实现方法（Get约束，仅可查看，对应着下面有Post约束） 
        [HttpGet]
        // 对应参数含义：分页、每页的条数、新闻类别、关键字查询
        public JsonResult GetNews(int pageIndex, int pageSize, int classifyId, string keyword)
        {
            // 定义一个表达式确认类别和表达式是否存在
            List<Expression<Func<News, bool>>> wheres = new List<Expression<Func<News, bool>>>();
            if (classifyId > 0)
            {
                wheres.Add(c => c.NewsClassifyId == classifyId);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                wheres.Add(c => c.Title.Contains(keyword));
            }
            // 输出json
            int total = 0;
            var news = _newsService.NewsPageQuery(pageSize, pageIndex, out total, wheres);
            return Json(new { total = total, data = news.data });
        }

        // 添加新闻
        public ActionResult NewsAdd()
        {
            var newClassifys = _newsService.GetNewsClassifyList();
            return View(newClassifys);
        }

        // 添加新闻的方法，IFormCollection collection是获取图片用的
        [HttpPost]
        public async Task<JsonResult> AddNews(AddNews news,IFormCollection collection)
        {
            if (news.NewsClassifyId <= 0 || string.IsNullOrEmpty(news.Title) || string.IsNullOrEmpty(news.Contents))
                return Json(new ResponseModel { code=0,result="参数有误"});
            var files = collection.Files;
            if(files.Count>0)
            {
                // 图片地址
                string webRootPath = _host.WebRootPath;
                string relativeDirPath = "\\NewsPic";
                string absolutePath = webRootPath + relativeDirPath;
                string[] fileTypes = new string[] {".gif",".jpg",".jpeg",".png",".bmp" };//定义允许上传的图片格式
                string extension = Path.GetExtension(files[0].FileName);
                // 实现图片上传
                if (fileTypes.Contains(extension))
                {
                    if (!Directory.Exists(absolutePath)) Directory.CreateDirectory(absolutePath);
                    string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                    var filePath = absolutePath + "\\" + fileName;
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await files[0].CopyToAsync(stream);
                    }
                    news.Image = "/NewsPic/" + fileName;
                    return Json(_newsService.AddNews(news));
                }
                return Json(new ResponseModel { code = 0, result = "图片格式有误" });
            }
            return  Json(new ResponseModel { code = 0, result = "请上传图片文件" });
        }

        // 删除新闻
        [HttpPost]
        public JsonResult DelNews(int id)
        {
            if (id < 0)
                return Json(new ResponseModel { code=0,result="新闻不存在"});
            return Json(_newsService.DelOneNews(id));
        }

        #region 新闻类别
        // 查看新闻类别表单
        public ActionResult NewsClassify()
        {
            var newsClassifys = _newsService.GetNewsClassifyList();
            return View(newsClassifys);
        }
        // 添加新闻类别
        public ActionResult NewsClassifyAdd()
        {
            return View();
        }
        // 添加新闻类别的方法
        [HttpPost]
        public JsonResult NewsClassifyAdd(AddNewsClassify newsClassify)
        {
            if (string.IsNullOrEmpty(newsClassify.Name))
                return Json(new ResponseModel { code = 0, result = "请输入新闻类别名称" });
            return Json(_newsService.AddNewsClassify(newsClassify));
        }

        // 编辑新闻类别
        public ActionResult NewsClassifyEdit(int id)
        {
            return View(_newsService.GetOneNewsClassify(id));
        }

        // 编辑新闻类别的方法
        [HttpPost]
        public JsonResult NewsClassifyEdit(EditNewsClassify newsClassify)
        {
            if (string.IsNullOrEmpty(newsClassify.Name))
                return Json(new ResponseModel { code = 0, result = "请输入新闻类别名称" });
            return Json(_newsService.EditNewsClassify(newsClassify));
        }
        #endregion
    }
}