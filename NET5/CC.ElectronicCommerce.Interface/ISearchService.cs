using CC.ElectronicCommerce.Model.Search;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Interface
{
	public interface ISearchService
	{
		SearchResult<Goods> search(SearchRequest searchRequest);
		public void ImpDataBySpu();
		public SearchResult<Goods> GetData(SearchRequest  searchRequest);
		public Goods GetGoodsBySpuId(long spuId);
	}
}
