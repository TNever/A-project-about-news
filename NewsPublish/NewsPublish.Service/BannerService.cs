using NewsPublish.Model.Entity;
using NewsPublish.Model.Request;
using NewsPublish.Model.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewsPublish.Service
{
    /// <summary>
    /// Banner服务
    /// 服务对应着请求与返回分别在类Request和Response
    /// </summary>
    public class BannerService
    {
        private Db _db;
        public BannerService(Db db)
        {
            this._db = db;
        }
        /// <summary>
        /// 添加Banner
        /// </summary>
        public ResponseModel AddBanner(AddBanner banner)
        {
            // new一个Banner
            var ba = new Banner { AddTime = DateTime.Now, Image = banner.Image, Url = banner.Url, Remark = banner.Remark };
            // 添加到数据库保存
            _db.Banner.Add(ba);
            int i = _db.SaveChanges();
            if (i > 0)
                return new ResponseModel { code = 200, result = "Banner添加成功" };
            return new ResponseModel { code = 0, result = "Banner添加失败" };
        }
        /// <summary>
        /// 获取Banner集合，上面是添加，这里是查看
        /// </summary>
        public ResponseModel GetBannerList()
        {
            var banners = _db.Banner.ToList().OrderByDescending(c => c.AddTime);
            var response = new ResponseModel();
            response.code = 200;
            response.result = "Banner集合获取成功";
            response.data = new List<BannerModel>();
            foreach (var banner in banners)
            {
                response.data.Add(new BannerModel {
                   Id=banner.Id,
                    Image = banner.Image,
                    Url = banner.Url,
                    Remark = banner.Remark
                });
            }
            return response;
        }
        /// <summary>
        /// 删除banner
        /// </summary>
        public ResponseModel DeleteBanner(int bannerId)
        {
            var banner = _db.Banner.Find(bannerId);
            if (banner == null)
                return new ResponseModel { code=0,result="Banner不存在"};
            _db.Banner.Remove(banner);
            int i = _db.SaveChanges();
            if (i > 0)
                return new ResponseModel { code = 200, result = "Banner删除成功" };
            return new ResponseModel { code = 0, result = "Banner删除失败" };
        }
    }
}
