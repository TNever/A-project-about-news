using Microsoft.EntityFrameworkCore;
using NewsPublish.Model.Entity;
using NewsPublish.Model.Request;
using NewsPublish.Model.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NewsPublish.Service
{
    public class NewsService
    {
        private Db _db;
        public NewsService(Db db)
        {
            this._db = db;
        }

        #region 新闻类别的服务内容 增加 查看内容 修改 查看类别单子
        /// <summary>
        /// 添加一个新闻类别
        /// </summary>
        public ResponseModel AddNewsClassify(AddNewsClassify newsClassify)
        {
            // 添加内容与数据库内容对比
            var exit = _db.NewsClassify.FirstOrDefault(c => c.Name == newsClassify.Name) != null;
            // 重复添加操作
            if (exit)
                return new ResponseModel { code = 0, result = "该类别已存在" };
            // 非重复添加操作
            // 添加内容
            var classify = new NewsClassify { Name = newsClassify.Name, Sort = newsClassify.Sort, Remark = newsClassify.Remark };
            // 存入数据库
            _db.NewsClassify.Add(classify);
            int i = _db.SaveChanges();
            if (i > 0)
                return new ResponseModel { code = 200, result = "新闻类别添加成功" };
            return new ResponseModel { code = 0, result = "新闻类别添加失败" };
        }

        /// <summary>
        /// 根据id值获取一个新闻类别
        /// </summary>
        public ResponseModel GetOneNewsClassify(int id)
        {
            // 直接在数据库获取id
            var classify = _db.NewsClassify.Find(id);
            // 空值操作
            if (classify == null)
                return new ResponseModel { code = 0, result = "该类别不存在" };
            // 非空值操作
            return new ResponseModel
            {
                code = 200,
                result = "新闻类别获取成功",
                // 返回的内容为Response文件夹下的..
                data = new NewsClassifyModel
                {
                    Id = classify.Id,
                    Sort = classify.Sort,
                    Name = classify.Name,
                    Remark = classify.Remark
                }
            };
        }

        /// <summary>
        /// 根据条件获取一个新闻类别，这里的where就是个输入的变量而已，返回的类型是实体类Entity文件夹下的NewsClassify
        /// 目的是给下面编辑类别用的
        /// </summary>
        private NewsClassify GetOneNewsClassify(Expression<Func<NewsClassify, bool>> where)
        {
            return _db.NewsClassify.FirstOrDefault(where);
        }

        /// <summary>
        /// 编辑一个新闻类别
        /// </summary>
        public ResponseModel EditNewsClassify(EditNewsClassify newsClassify)
        {
            // 这里的GetOneNewsClassify调用的即是根据条件获取新闻类别的方法，将数据库新闻类型的id与获取到的id对比再做判断
            var classify = this.GetOneNewsClassify(c => c.Id == newsClassify.Id);
            // 空值判断及操作
            if (classify == null)
                return new ResponseModel { code = 0, result = "该类别不存在" };
            // 非空值下进行赋值，赋值类型来源于将EditNewsClassify类
            classify.Name = newsClassify.Name;
            classify.Sort = newsClassify.Sort;
            classify.Remark = newsClassify.Remark;
            // 赋值完成即可更新数据库
            _db.NewsClassify.Update(classify);
            int i = _db.SaveChanges();
            if (i > 0)
                return new ResponseModel { code = 200, result = "新闻类别编辑成功" };
            return new ResponseModel { code = 0, result = "新闻类别编辑失败" };
        }

        /// <summary>
        /// 获取新闻类别集合
        /// </summary>
        public ResponseModel GetNewsClassifyList()
        {
            // 获取新闻类别后使用OrderByDescending进行降序排序
            var classifys = _db.NewsClassify.OrderByDescending(c => c.Sort).ToList();
            // 这条可不写，我们主要是要ResponseModel里面的data
            var response = new ResponseModel { code = 200, result = "新闻类别集合获取成功" };
            // data是一个链表，内容为实例类里面的数据类型是NewsClassifyModel
            response.data = new List<NewsClassifyModel>();
            // 遍历这个排序后的classifys，把他们以NewsClassifyModel类型添加在response里面
            foreach (var classify in classifys)
            {
                response.data.Add(new NewsClassifyModel
                {
                    Id = classify.Id,
                    Name = classify.Name,
                    Sort = classify.Sort,
                    Remark = classify.Remark
                });
            }
            return response;
        }
        #endregion

        #region 新闻内容的服务：添加一个新闻、查看一个新闻、删除、分页查询、查询新闻列表（局部主页）、获取评论最多列表（副页）、搜索一个新闻（搜索框）、获取新闻总数量（展示框）、获取推荐新闻列表（文章内容的推荐）
        /// <summary>
        /// 添加新闻
        /// </summary>
        public ResponseModel AddNews(AddNews news)
        {
            // 添加新闻要选择类别
            var classify = this.GetOneNewsClassify(c => c.Id == news.NewsClassifyId);
            // 类别不存在的处理方法
            if (classify == null)
                return new ResponseModel { code = 0, result = "该类别不存在" };
            // 类别存在，进行内容添加，然后保存至数据库
            var n = new News
            {
                NewsClassifyId = news.NewsClassifyId,
                Title = news.Title,
                Image = news.Image,
                Contents = news.Contents,
                PublishDate = DateTime.Now,
                Remark = news.Remark
            };
            _db.News.Add(n);
            int i = _db.SaveChanges();
            if (i > 0)
                return new ResponseModel { code = 200, result = "新闻添加成功" };
            return new ResponseModel { code = 0, result = "新闻添加失败" };
        }

        /// <summary>
        /// 获取一个新闻
        /// </summary>
        public ResponseModel GetoneNews(int id)
        {
            // 根据id从数据库获取新闻，加.Include("NewsClassify").Include("NewsComment")这一段的原因是Entity文件夹下的new在定义的时候包含这两个属性，若不加，则在数据库内无法查找
            var news = _db.News.Include("NewsClassify").Include("NewsComment").FirstOrDefault(c => c.Id == id);
            // 空值操作
            if (news == null)
                return new ResponseModel { code = 0, result = "该新闻不存在" };
            // 非空值返回ResponseModel类
            return new ResponseModel
            {
                code = 200,
                result = "新闻获取成功",
                // data类型为Response文件下的NewsModel
                data = new NewsModel
                {
                    Id = news.Id,
                    ClassifyName = news.NewsClassify.Name,
                    Title = news.Title,
                    Image = news.Image,
                    Contents = news.Contents,
                    PublishDate = news.PublishDate.ToString("yyyy-MM-dd"),
                    CommentCount = news.NewsComment.Count(),
                    Remark = news.Remark
                }
            };
        }

        /// <summary>
        /// 删除一个新闻
        /// 没什么好说的，很简单
        /// </summary>
        public ResponseModel DelOneNews(int id)
        {
            var news = _db.News.FirstOrDefault(c => c.Id == id);
            if (news == null)
                return new ResponseModel { code = 0, result = "该新闻不存在" };
            _db.News.Remove(news);
            int i = _db.SaveChanges();
            if (i > 0)
                return new ResponseModel { code = 200, result = "新闻删除成功" };
            return new ResponseModel { code = 0, result = "新闻删除失败" };
        }

        /// <summary>
        /// 分页查询新闻
        /// 输入参数分别为一页有多少数据、第几页、总数据、条件集合
        /// </summary>
        public ResponseModel NewsPageQuery(int pageSize, int pageIndex, out int total, List<Expression<Func<News, bool>>> where) 
        {
            // 定义一个list，为News的信息，包含下面两个
            var list = _db.News.Include("NewsClassify").Include("NewsComment");
            // 参数里面的条件过滤
            foreach (var item in where)
            {
                list = list.Where(item);
            }
            // 总数据
            total = list.Count();
            // 分页方法（先根据发布日期排个序，过滤掉每一页的个数乘以对应的页码，后使用take方法抓取一页的数量.ToList）
            var pageData = list.OrderByDescending(c => c.PublishDate).Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
            // 完成分页返回response
            var response = new ResponseModel
            {
                code = 200,
                result = "分页新闻获取成功"
            };
            // 这里的data不是一条，而是一个链表
            response.data = new List<NewsModel>();
            foreach (var news in pageData)
            {
                response.data.Add(new NewsModel
                {
                    Id = news.Id,
                    ClassifyName = news.NewsClassify.Name,
                    Title = news.Title,
                    Image = news.Image,
                    // 这里的Contents是新闻内容，因为新闻内容很多，但是在标题业不需全部显示，所以后面的操作是已50个字为区间进行操作。
                    // 操作方法为>50的话截取50个字+...，小于等于50就直接放上就行。
                    Contents = news.Contents.Length>50?news.Contents.Substring(0,50)+"...":news.Contents,
                    PublishDate = news.PublishDate.ToString("yyyy-MM-dd"),
                    CommentCount = news.NewsComment.Count(),
                    Remark = news.Remark
                });
            }
            return response;
        }

        /// <summary>
        /// 查询新闻列表，主页内容
        /// 根据条件获取新闻列表，int topCount表示只需要前几条
        /// </summary>
        public ResponseModel GetNewsList(Expression<Func<News, bool>> where, int topCount)
        {
            var list = _db.News.Include("NewsClassify").Include("NewsComment").Where(where).OrderByDescending(c => c.PublishDate).Take(topCount);
            var response = new ResponseModel
            {
                code = 200,
                result = "新闻列表获取成功"
            };
            response.data = new List<NewsModel>();
            foreach (var news in list)
            {
                response.data.Add(new NewsModel
                {
                    Id = news.Id,
                    ClassifyName = news.NewsClassify.Name,
                    Title = news.Title,
                    Image = news.Image,
                    Contents = news.Contents.Length > 50 ? news.Contents.Substring(0, 50) : news.Contents,
                    PublishDate = news.PublishDate.ToString("yyyy-MM-dd"),
                    CommentCount = news.NewsComment.Count(),
                    Remark = news.Remark
                });
            }
            return response;
        }

        /// <summary>
        /// 获取最新评论的新闻集合
        /// 副页内容
        /// </summary>
        public ResponseModel GetNewCommentNewsList(Expression<Func<News, bool>> where, int topCount)
        {
            // 获取Newsid的集合 流程：获取最新添加的评论、过滤掉评论相同ID的新闻并排序、查询出对应ID的新闻、释放前几条置顶
            var newsIds = _db.NewsComment.OrderByDescending(c => c.AddTime).GroupBy(c => c.NewsId).Select(c => c.Key).Take(topCount);
            var list = _db.News.Include("NewsClassify").Include("NewsComment").Where(c => newsIds.Contains(c.Id)).Where(where
                ).OrderByDescending(c => c.PublishDate);
            // 获取后的返回操作
            var response = new ResponseModel
            {
                code = 200,
                result = "最新评论的新闻获取成功"
            };
            response.data = new List<NewsModel>();
            foreach (var news in list)
            {
                response.data.Add(new NewsModel
                {
                    Id = news.Id,
                    ClassifyName = news.NewsClassify.Name,
                    Title = news.Title,
                    Image = news.Image,
                    Contents = news.Contents.Length > 50 ? news.Contents.Substring(0, 50) : news.Contents,
                    PublishDate = news.PublishDate.ToString("yyyy-MM-dd"),
                    CommentCount = news.NewsComment.Count(),
                    Remark = news.Remark
                });
            }
            return response;
        }

        /// <summary>
        /// 搜索一个新闻
        /// </summary>
        public ResponseModel GetSearchOneNews(Expression<Func<News, bool>> where)
        {
            var news = _db.News.Where(where).FirstOrDefault();
            if (news == null)
                return new ResponseModel { code = 0, result = "新闻搜索失败" };
            return new ResponseModel { code = 200, result = "新闻搜索成功", data = news.Id };
        }

        /// <summary>
        /// 获取新闻数量
        /// </summary>
        public ResponseModel GetNewsCount(Expression<Func<News, bool>> where)
        {
            var count = _db.News.Where(where).Count();
            return new ResponseModel {code=200,result="新闻数量获取成功",data=count };
        }

        /// <summary>
        /// 获取推荐新闻列表
        /// 推荐方式：以当前类别下的最多评论为根据
        /// </summary>
        public ResponseModel GetRecommendNewsList(int newsId)
        {
            var news = _db.News.FirstOrDefault(c=>c.Id==newsId);
            if (news == null)
                return new ResponseModel { code=0,result="新闻不存在"};
            //首先获取当前类的新闻ID同时过滤掉自己，然后先根据发布日期排序、后根据评论数量排序，然后使用take选择前6条。
            var newsList = _db.News.Include("NewsComment").Where(c => c.NewsClassifyId == news.NewsClassifyId && c.Id != newsId).OrderByDescending(c => c.PublishDate).OrderByDescending(c => c.NewsComment.Count).Take(6).ToList();
            var response = new ResponseModel
            {
                code = 200,
                result = "最新评论的新闻获取成功"
            };
            response.data = new List<NewsModel>();
            foreach (var n in newsList)
            {
                response.data.Add(new NewsModel
                {
                    Id = n.Id,
                    ClassifyName = n.NewsClassify.Name,
                    Title = n.Title,
                    Image = n.Image,
                    Contents = n.Contents.Length > 50 ? n.Contents.Substring(0, 50) : n.Contents,
                    PublishDate = n.PublishDate.ToString("yyyy-MM-dd"),
                    CommentCount = n.NewsComment.Count(),
                    Remark = n.Remark
                });
            }
            return response;
        }
    }
    #endregion
}
