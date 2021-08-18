using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model;
using CC.ElectronicCommerce.Model.Search;

namespace CC.ElectronicCommerce.Service
{
    public class SearchService : ISearchService
    {
        private OrangeContext _orangeContext;
        private IGoodsService _goodsService;
        private IBrandService _brandService;
        private ICategoryService _categoryService;
        private ISpecService _specService;
        private IElasticSearchService _elasticSearchService;
        public SearchService(IGoodsService goodsService, IBrandService brandService, ICategoryService categoryService, ISpecService specService, OrangeContext orangeContext, IElasticSearchService elasticSearchService)
        {
            _goodsService = goodsService;
            _brandService = brandService;
            _categoryService = categoryService;
            _specService = specService;
            _orangeContext = orangeContext;
            _elasticSearchService = elasticSearchService;
        }
        public SearchResult<Goods> search(SearchRequest searchRequest)
        {
            throw new NotImplementedException();
        }
        public void ImpDataBySpu()
        {
            /*var list = _orangeContext.TbSpu.ToList();
			ElasticSearchTool elasticSearchTool = new ElasticSearchTool();
			elasticSearchTool.Send<TbSpu>(list);*/
            ImportToEs();

        }

        /// <summary>
        /// Gerry 方法
        /// </summary>
        private void ImportToEs()
        {


            int page = 1;
            int size;
            int rows = 100;

            do
            {
                List<Goods> goodsList = new List<Goods>();
                // 上架商品
                PageResult<TbSpu> result = _goodsService.QuerySpuByPage(page, rows, null, true);
                List<TbSpu> spus = result.rows;
                size = spus.Count;
                foreach (TbSpu spu in spus)
                {
                    try
                    {
                        Goods g = BuildGoods(spu);
                        // 处理好的数据添加到集合中
                        goodsList.Add(g);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        continue;//部分数据不严格
                    }
                }
                // 存入es
                _elasticSearchService.Send(goodsList);
                page++;
            } while (size == 100);
        }

        public Goods GetGoodsBySpuId(long spuId)
        {
            var spu = this._goodsService.QuerySpuBySpuId(spuId);
            return this.BuildGoods(spu);
        }

        /// <summary>
        /// 根据Spu构建Goods对象
        /// </summary>
        /// <param name="spu"></param>
        /// <returns></returns>
        private Goods BuildGoods(TbSpu spu)
        {
            Goods goods = new Goods();
            // 1、查询商品分类名称组成的集合
            List<TbCategory> lists = _categoryService.QueryCategoryByIds(new List<long>() { spu.Cid1, spu.Cid2, spu.Cid3 });
            var cnames = lists.Select(c => c.Name).ToList();
            // 2、根据品牌ID查询品牌信息
            var brand = _brandService.QueryBrandByBid(spu.BrandId);
            // 3、所有的搜索字段拼接到all中，all存入索引库，并进行分词处理，搜索时与all中的字段进行匹配查询
            Regex regex = new Regex(@"<[^>]+>|</[^>]+>");
            string subTitle = regex.Replace(spu.SubTitle, "");
            string all = subTitle + " " + string.Join(" ", cnames) + " " + brand.Name;
            // 4、根据spu查询所有的sku集合
            List<TbSku> skuList = _goodsService.QuerySkuBySpuId(spu.Id);
            if (skuList == null && skuList.Count > 0)
            {
                throw new Exception("查询商品对应sku不存在");
            }
            //4.1 存储price的集合
            HashSet<double> priceSet = new HashSet<double>();
            //4.2 设置存储skus的json结构的集合，用map结果转化sku对象，转化为json之后与对象结构相似（或者重新定义一个对象，存储前台要展示的数据，并把sku对象转化成自己定义的对象）
            List<Dictionary<string, object>> skus = new List<Dictionary<string, object>>();
            foreach (TbSku sku in skuList)
            {
                priceSet.Add(sku.Price);
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("id", sku.Id);
                dic.Add("title", sku.Title);
                //sku中有多个图片，只展示第一张
                dic.Add("image", sku.Images.Split(",")[0]);
                dic.Add("price", sku.Price);
                // 添加到字典中
                skus.Add(dic);
            }

            //查询规格参数，规格参数中分为通用规格参数和特有规格参数
            List<TbSpecParam> specParams = _specService.QuerySpecParams(null, spu.Cid3, true, null);
            if (specParams == null && specParams.Count > 0)
            {
                throw new Exception("规格参数不存在");
            }

            //查询商品详情
            TbSpuDetail spuDetail = _goodsService.QuerySpuDetailBySpuId(spu.Id);
            //获取通用规格参数
            Dictionary<long, string> genericSpec = JsonConvert.DeserializeObject<Dictionary<long, string>>(spuDetail.GenericSpec);
            //获取特有规格参数
            Dictionary<long, List<string>> specialSpec = JsonConvert.DeserializeObject<Dictionary<long, List<string>>>(spuDetail.SpecialSpec);
            //定义spec对应的map
            Dictionary<string, object> specDic = new Dictionary<string, object>();
            //对规格进行遍历，并封装spec，其中spec的key是规格参数的名称，值是商品详情中的值
            foreach (TbSpecParam param in specParams)
            {
                //key是规格参数的名称
                string key = param.Name;
                object value = "";

                if (param.Generic == true)
                {
                    //参数是通用属性，通过规格参数的ID从商品详情存储的规格参数中查出值
                    value = genericSpec[param.Id];
                    if (param.Numeric == true)
                    {
                        //参数是数值类型，处理成段，方便后期对数值类型进行范围过滤
                        value = ChooseSegment(value.ToString(), param);
                    }
                }
                else
                {
                    //参数不是通用类型
                    value = specialSpec[param.Id];
                }
                value ??= "其他";
                //存入map
                specDic.Add(key, value);
            }

            // 封装商品对象
            goods.id = spu.Id;
            goods.brandId = spu.BrandId;
            goods.cid1 = spu.Cid1;
            goods.cid2 = spu.Cid2;
            goods.cid3 = spu.Cid3;
            goods.createTime = spu.CreateTime;
            goods.all = all;
            goods.price = priceSet;
            goods.subtitle = spu.SubTitle;
            goods.specs = specDic;
            goods.skus = JsonConvert.SerializeObject(skus);

            return goods;
        }

        private static string ChooseSegment(string value, TbSpecParam p)
        {
            try
            {
                double val = double.Parse(value);
                string result = "其它";
                // 保存数值段
                foreach (string segment in p.Segments.Split(","))
                {
                    string[] segs = segment.Split("-");
                    // 获取数值范围
                    double begin = double.Parse(segs[0]);
                    double end = double.MaxValue;
                    if (segs.Length == 2)
                    {
                        end = double.Parse(segs[1]);
                    }
                    // 判断是否在范围内
                    if (val >= begin && val < end)
                    {
                        if (segs.Length == 1)
                        {
                            result = segs[0] + p.Unit + "以上";
                        }
                        else if (begin == 0)
                        {
                            result = segs[1] + p.Unit + "以下";
                        }
                        else
                        {
                            result = segment + p.Unit;
                        }
                        break;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

        }

        public SearchResult<Goods> GetData(SearchRequest searchRequest)
        {

            var client = _elasticSearchService.GetElasticClient();
            var list = client.Search<Goods>(s => s
                .From((searchRequest.getPage() - 1) * searchRequest.getSize())
                .Size(searchRequest.getSize())
                .Query(q => q
                     .Match(m => m
                        .Field(f => f.all)
                        .Query(searchRequest.key)
                     )
                )).Documents.ToList();

            var total = client.Search<Goods>(s => s
                .Query(q => q
                     .Match(m => m
                        .Field(f => f.all)
                        .Query(searchRequest.key)
                     )
                )).Documents.Count();

            var cid3s = list.Select(m => m.cid3).Distinct().ToList();
            var brandIds = list.Select(m => m.brandId).Distinct().ToList();

            List<TbCategory> tbCategories = _categoryService.QueryCategoryByIds(cid3s);
            List<TbBrand> tbBrands = _brandService.QueryBrandByIds(brandIds);

            List<Dictionary<string, object>> specs = null;

            if (tbCategories != null && tbCategories.Count == 1)
            {
                specs = HandleSpecs(tbCategories[0].Id, list);
            }
            foreach (var item in list)
            {
                item.specs = null;
            }
            int page = total % searchRequest.getSize() == 0 ? total / searchRequest.getSize() : (total / searchRequest.getSize()) + 1;
            return new SearchResult<Goods>(total, page, list, tbCategories, tbBrands, specs);
        }

        private List<Dictionary<string, object>> HandleSpecs(long id, List<Goods> goods)
        {
            List<Dictionary<string, object>> specs = new List<Dictionary<string, object>>();

            //查询可过滤的规格参数

            List<TbSpecParam> tbSpecParams = _specService.QuerySpecParams(null, id, true, null);
            //基本查询条件
            foreach (TbSpecParam param in tbSpecParams)
            {
                //聚合
                string name = param.Name;
                // 如果对keyword类型的字符串进行搜索必须是精确匹配terms
                //queryBuilder.addAggregation(AggregationBuilders.terms(name).field("specs." + name + ".keyword"));
                Dictionary<string, object> map = new Dictionary<string, object>();
                map.Add("k", name);
                var dicspec = goods.Select(m => m.specs).Where(m => m.Keys.Contains(name));

                var options = new List<string>();
                foreach (var item in dicspec)
                {
                    options.Add(item[name].ToString());
                }
                map.Add("options", options.Distinct().ToList());

                specs.Add(map);
            }
            return specs;
        }

        #region delete
        //public Hashtable GetData2(string key)
        //{
        //	Hashtable hashtable = new Hashtable();
        //	var client = new ElasticSearchTool().GetElasticClient();
        //	var list = client.Search<TbSpu>(s => s
        //		//.From(0)
        //		//.Size(10)
        //		.Query(q => q
        //			 .Match(m => m
        //				.Field(f => f.SubTitle)
        //				.Query(key)
        //			 )
        //		)).Documents;
        //	foreach (var item in list)
        //	{
        //		var setting = new JsonSerializerSettings
        //		{
        //			ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        //		};
        //		item.Skusstr = Newtonsoft.Json.JsonConvert.SerializeObject(_orangeContext.TbSku.Where(m => m.SpuId == item.Id).ToList(), Formatting.None, setting);
        //	}
        //	var brandIds = list.Select(m => m.BrandId).ToList();
        //	var cs3 = list.Select(m => m.Cid3).ToList();
        //	var ids = list.Select(m => m.Id).ToList();
        //	List<TbBrand> tbBrands = _orangeContext.TbBrand.Where(m => brandIds.Contains(m.Id)).ToList();
        //	List<TbCategory> categorys = _orangeContext.TbCategory.Where(m => cs3.Contains(m.Id)).ToList();

        //	List<TbSpuDetail> gsps = _orangeContext.TbSpuDetail.Where(m => ids.Contains(m.SpuId)).ToList();

        //	// 获取一般的规格参数
        //	Dictionary<string, string> gspecdics = new Dictionary<string, string>();
        //	//获取特殊规格参数
        //	Dictionary<string, List<string>> sspecdics = new Dictionary<string, List<string>>();
        //	foreach (TbSpuDetail item in gsps)
        //	{
        //		var gspec = item.GenericSpec;
        //		var sspec = item.SpecialSpec;
        //		var gspecdic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(gspec);
        //		foreach (var gspecitem in gspecdic)
        //		{
        //			if (!gspecdics.ContainsKey(gspecitem.Key))
        //			{
        //				gspecdics.Add(gspecitem.Key, gspecitem.Value);
        //			}
        //		}

        //		var sspecdic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(sspec);
        //		foreach (var sspecitem in sspecdic)
        //		{
        //			if (!sspecdics.ContainsKey(sspecitem.Key))
        //			{
        //				sspecdics.Add(sspecitem.Key, sspecitem.Value);
        //			}
        //		}
        //	}

        //	// 处理一般的规格参数
        //	hashtable.Add("total", list.Count);
        //	hashtable.Add("totalPages", 10);
        //	hashtable.Add("rows", list.ToList());
        //	hashtable.Add("brands", tbBrands);
        //	hashtable.Add("categories", categorys);

        //	//hashtable.Add("specs");

        //	return hashtable;


        //} 
        #endregion

    }
}
