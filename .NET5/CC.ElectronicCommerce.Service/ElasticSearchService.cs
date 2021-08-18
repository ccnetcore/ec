﻿using CC.ElectronicCommerce.Common.IOCOptions;
using CC.ElectronicCommerce.Interface;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Service
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly ElasticSearchOptions _elasticSearchOptions;

        public ElasticSearchService(IOptionsMonitor<ElasticSearchOptions> optionsMonitor)
        {
            _elasticSearchOptions = optionsMonitor.CurrentValue;
            var settings = new ConnectionSettings(new Uri(_elasticSearchOptions.Url))
    .DefaultIndex(this._elasticSearchOptions.IndexName);
            Client = new ElasticClient(settings);
        }
        private ElasticClient Client;
        public ElasticClient GetElasticClient()
        {
            return Client;
        }
        public void Send<T>(List<T> model) where T : class
        {
            Client.IndexMany(model);
        }

        public void InsertOrUpdata<T>(T model) where T : class
        {
            Client.IndexDocument(model);
        }

        public bool Delete<T>(string id) where T : class
        {

            var response = Client.Delete<T>(id);
            return response.IsValid;
        }
        public bool DropIndex(string indexName)
        {
            return Client.Indices.Delete(Indices.Parse(indexName)).IsValid;
        }
        public void CreateIndex(string indexName)
        {
            var settings = new ConnectionSettings(new Uri(_elasticSearchOptions.Url))
    .DefaultIndex(indexName);
            this.Client = new ElasticClient(settings);
        }

    }
}
