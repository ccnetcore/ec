using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Interface
{
    public interface IElasticSearchService
    {

        public ElasticClient GetElasticClient();
        public void Send<T>(List<T> model) where T : class;

		public void InsertOrUpdata<T>(T model) where T : class;

		public bool Delete<T>(string id) where T : class;
		public bool DropIndex(string indexName);
		public void CreateIndex(string indexName);
	}
}
