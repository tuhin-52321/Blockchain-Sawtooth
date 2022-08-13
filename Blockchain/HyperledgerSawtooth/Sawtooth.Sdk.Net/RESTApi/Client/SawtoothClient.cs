using ProtoBuf;
using Sawtooth.Sdk.Net.RESTApi.Payload;
using Sawtooth.Sdk.Net.RESTApi.Payload.Json;
using BatchList = Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf.BatchList;
using System.Net.Http.Headers;
using System.Text.Json;
using Sawtooth.Sdk.Net.Utils;

namespace Sawtooth.Sdk.Net.RESTApi.Client
{
    public class SawtoothClient
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly JsonSerializerOptions _options;

        public SawtoothClient(string url)
        {
            _httpClient.BaseAddress = new Uri(url);
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<CommonJsonResponse?> PostBatchListAsync(BatchList batch_list)
        {
            byte[] data = batch_list.ToProtobufByteArray();
            var content = new ByteArrayContent(data);
            content.Headers.Add("Content-Type", "application/octet-stream");
            using (var response = await _httpClient.PostAsync("batches", content))
            {
                return await ConvertToReponseObjectAsync<CommonJsonResponse>(response, 202, new[] { 400, 429, 500, 503 });
            }
        }
        private async Task<RESPONSE_TYPE?> GetRequestAsync<RESPONSE_TYPE>(string requestUti, string query_string, int sucess_code, params int[] failure_codes)
        {
            using (HttpResponseMessage response = await _httpClient.GetAsync(requestUti + query_string))
            {
                return await ConvertToReponseObjectAsync<RESPONSE_TYPE>(response, sucess_code, failure_codes);
            }
        }

        private async Task<RESPONSE_TYPE?> ConvertToReponseObjectAsync<RESPONSE_TYPE>(HttpResponseMessage response, int success_code, params int[] failure_codes)
        {
            Stream responseStream = await response.Content.ReadAsStreamAsync();
            if ((int)response.StatusCode == success_code)
            {
                return await JsonSerializer.DeserializeAsync<RESPONSE_TYPE>(responseStream, _options);
            }
            if (failure_codes.Contains((int)response.StatusCode))
            {
                ErrorResponse? error = await JsonSerializer.DeserializeAsync<ErrorResponse>(responseStream, _options);
                throw new HttpException(error);
            }
            using (StreamReader reader = new StreamReader(responseStream))
            {
                throw new HttpException(response.StatusCode, reader.ReadToEnd());
            }
        }

        private string FormPagingQueryString(string? start_id, params (string Name, string? Value)[] query_params)
        {
            string query_string = string.Empty;

            if (start_id != null) query_string += "start_id=" + start_id;

            foreach(var param in query_params)
            {
                if (param.Value != null) query_string += param.Name + "=" + param.Value;
            }

            if (query_string.Length > 0) query_string = "?" + query_string;

            return query_string;
            
        }


        private async Task<PageOf<T>> GetPagedListAsync<T>(string uri, string? start_id = null, params (string Name,string? Value)[] query_params)
        {
            PagedJsonResponse<T>? response = await GetRequestAsync<PagedJsonResponse<T>>(uri, FormPagingQueryString(start_id, query_params), 200, 400, 500, 503);

            if (response == null) return new PageOf<T> { List = new List<T?>(), Next = null };

            return new PageOf<T> { List = response.Data, Head = response.Head,  Next = response.Paging?.Next };

        }

        private async Task<FullList<T>> GetAllListAsync<T>(string uri, params (string Name, string? Value)[] query_params)
        {
            FullList<T> list = new FullList<T>();
            string? start_id = null;
            do
            {
                PagedJsonResponse<T>? response = await GetRequestAsync<PagedJsonResponse<T>>(uri, FormPagingQueryString(start_id, query_params), 200, 400, 500, 503);

                if (response == null) break;

                list.List.AddRange(response.Data);

                if(list.Head == null)
                {
                    list.Head = response.Head;
                }

                if (response.Paging?.Next == null) break;

                start_id = response.Paging?.Next;


            } while (true);

            return list;
        }


        private async Task<T?> GetAsync<T>(string uri , string request_params = "")
        {
            SingleJsonResponse<T>? response = await GetRequestAsync<SingleJsonResponse<T>>(uri, request_params, 200, 400, 404, 500, 503);

            if(response == null) return default(T?);

            return response.Data;

        }

        
        public async Task<PageOf<Batch>> GetBatchesAsync(string? start)
        {
            return await GetPagedListAsync<Batch>("batches", start);
        }
        public async Task<FullList<Batch>> GetBatchesAsync()
        {
            return await GetAllListAsync<Batch>("batches");
        }
        public async Task<Batch?> GetBatchAsync(string batch_id)
        {
            return await GetAsync<Batch>($"batches/{batch_id}");
        }

        public async Task<List<BatchStatus>?> GetBatchStatusesAsync(params string?[] batch_id)
        {
            return await GetAsync<List<BatchStatus>>("batch_statuses", "?id=" + string.Join(",", batch_id));
        }

        public async Task<List<BatchStatus>?> GetBatchStatusUsingLinkAsync(string url)
        {
            return await GetAsync<List<BatchStatus>>(url);
        }

        public async Task<PageOf<StateItem>> GetStatesAsync(string? start)
        {
            return await GetStatesWithFilterAsync(start,null);
        }

        public async Task<PageOf<StateItem>> GetStatesWithFilterAsync(string? start, string? address_filter)
        {
            return await GetPagedListAsync<StateItem>("state", start, ("address", address_filter));
        }
        public async Task<FullList<StateItem>> GetStatesWithFilterAsync(string? address_filter)
        {
            return await GetAllListAsync<StateItem>("state", ("address", address_filter));
        }
        public async Task<FullList<StateItem>> GetStatesAsync()
        {
            return await GetStatesWithFilterAsync(null);
        }

        public async Task<State?> GetStateAsync(string address)
        {
            return await GetRequestAsync<State>($"state/{address}", "", 200, 400, 404, 500, 503);
        }


        public async Task<PageOf<Block>> GetBlocksAsync(string? start)
        {
            return await GetPagedListAsync<Block>("blocks", start);
        }
        public async Task<FullList<Block>> GetBlocksAsync()
        {
            return await GetAllListAsync<Block>("blocks");
        }
        public async Task<Block?> GetBlockAsync(string block_id)
        {
            return await GetAsync<Block>($"blocks/{block_id}");
        }

        public async Task<PageOf<Transaction>> GetTransactionsAsync(string? start)
        {
            return await GetPagedListAsync<Transaction>("transactions", start);
        }
        public async Task<FullList<Transaction>> GetTransactionsAsync()
        {
            return await GetAllListAsync<Transaction>("transactions");
        }
        public async Task<Transaction?> GetTransactionAsync(string transaction_id)
        {
            return await GetAsync<Transaction>($"transactions/{transaction_id}");
        }

        public async Task<List<TransactionReceipt>?> GetTransactionReceiptsAsync(params string[] transaction_id)
        {
            return await GetAsync<List<TransactionReceipt>>("receipts", "?id=" + string.Join(",", transaction_id));
        }

        public async Task<List<string>?> GetPeersAsync()
        {
            return await GetAsync<List<string>>("peers");
        }

        public async Task<Status?> GetStatusAsync()
        {
            return await GetAsync<Status>("status");
        }



    }
}
