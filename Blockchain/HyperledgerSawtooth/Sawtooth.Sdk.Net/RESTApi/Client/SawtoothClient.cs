using Sawtooth.Sdk.Net.RESTApi.Payload;
using System.Net.Http.Headers;
using System.Text.Json;

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

        private async Task<RESPONSE_TYPE?> PostRequestAsync<REQUEST_TYPE, RESPONSE_TYPE>(string requestUti, REQUEST_TYPE request_object, int sucess_code, params int[] failure_codes)
        {

            MemoryStream ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<REQUEST_TYPE>(ms, request_object);
            ms.Seek(0, SeekOrigin.Begin);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUti);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using (StreamContent requestContent = new StreamContent(ms))
            {
                request.Content = requestContent;
                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    return await ConvertToReponseObjectAsync<RESPONSE_TYPE>(response, sucess_code, failure_codes);
                }
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

            return new PageOf<T> { List = response.Data, Next = response.Paging?.Next };

        }

        private async Task<List<T?>> GetAllListAsync<T>(string uri, params (string Name, string? Value)[] query_params)
        {
            List<T?> list = new List<T?>();
            string? start_id = null;
            do
            {
                PagedJsonResponse<T>? response = await GetRequestAsync<PagedJsonResponse<T>>(uri, FormPagingQueryString(start_id, query_params), 200, 400, 500, 503);

                if (response == null) break;

                list.AddRange(response.Data);

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

        
        public async Task<PageOf<Batch>> GetBatchesAsync(string? start = null)
        {
            return await GetPagedListAsync<Batch>("batches", start);
        }
        public async Task<List<Batch?>> GetBatchesAsync()
        {
            return await GetAllListAsync<Batch>("batches");
        }
        public async Task<Batch?> GetBatchAsync(string batch_id)
        {
            return await GetAsync<Batch>($"batches/{batch_id}");
        }

        public async Task<List<BatchStatus>?> GetBatchStatusesAsync(params string[] batch_id)
        {
            return await GetAsync<List<BatchStatus>>("batch_statuses", "?id=" + string.Join(",", batch_id));
        }

        public async Task<PageOf<StateItem>> GetStatesAsync(string? start = null)
        {
            return await GetStatesWithFilterAsync(start,null);
        }

        public async Task<PageOf<StateItem>> GetStatesWithFilterAsync(string? start = null, string? address_filter = null)
        {
            return await GetPagedListAsync<StateItem>("state", start, ("address_filter", address_filter));
        }
        public async Task<List<StateItem?>> GetStatesWithFilterAsync(string? address_filter)
        {
            return await GetAllListAsync<StateItem>("state", ("address_filter", address_filter));
        }
        public async Task<List<StateItem?>> GetStatesAsync()
        {
            return await GetStatesWithFilterAsync(null);
        }

        public async Task<State?> GetStateAsync(string address)
        {
            return await GetRequestAsync<State>($"state/{address}", "", 200, 400, 404, 500, 503);
        }


        public async Task<PageOf<Block>> GetBlocksAsync(string? start = null)
        {
            return await GetPagedListAsync<Block>("blocks", start);
        }
        public async Task<List<Block?>> GetBlocksAsync()
        {
            return await GetAllListAsync<Block>("blocks");
        }
        public async Task<Block?> GetBlockAsync(string block_id)
        {
            return await GetAsync<Block>($"blocks/{block_id}");
        }

        public async Task<PageOf<Transaction>> GetTransactionsAsync(string? start = null)
        {
            return await GetPagedListAsync<Transaction>("transactions", start);
        }
        public async Task<List<Transaction?>> GetTransactionsAsync()
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
