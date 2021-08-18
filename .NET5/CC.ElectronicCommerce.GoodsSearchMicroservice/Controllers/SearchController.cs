using CC.ElectronicCommerce.Common.Models;
using CC.ElectronicCommerce.Interface;
using CC.ElectronicCommerce.Model.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.GoodsSearchMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private ISearchService _searchService;
        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }
        [Route("page")]
        [HttpPost]
        public Result Search(SearchRequest searchRequest)
        {
            SearchResult<Goods> searchResult = this._searchService.GetData(searchRequest);

            return Result.Success("success").SetData(searchResult);
        }

        //测试单元使用
        //[Route("ImpData")]
        //[HttpGet]
        //public void ImpData()
        //{
        //    _searchService.ImpDataBySpu();
        //}


    }
}
