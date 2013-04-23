﻿using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestWrapper {
	public class Client {

		public string ApiRoot { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }

		public dynamic Execute(string resource, Method method, Action<RestRequest> decorator) {
			var client = GetRestClient();
			var request = new RestRequest(method);
			request.Resource = resource;
			decorator(request);
			var response = client.Execute(request);
			try {
				return JsonConvert.DeserializeObject<dynamic>(response.Content);
			} catch {
				throw new RestException { Response = response };
			}
		}

		public RestClient GetRestClient() {
			var client = new RestSharp.RestClient(ApiRoot);
			client.Authenticator = new HttpBasicAuthenticator(UserName, Password);
			return client;
		}

		async public Task<dynamic> ExecuteAsync(string resource, Method method, Action<RestRequest> decorator) {
			var client = GetRestClient();
			var request = new RestRequest(method);
			request.Resource = resource;
			decorator(request);
			return await ExecuteTaskAsync(client, request, new CancellationToken());
		}

		public class RestException : Exception {

			public IRestResponse Response { get; set; }
		}

		public async Task<dynamic> ExecuteTaskAsync(
RestClient client, RestRequest request, CancellationToken cancellationToken) {
			var taskCompletionSource = new TaskCompletionSource<dynamic>();

			var asyncHandle = client.ExecuteAsync(
					request, r => {
						try {
							if (r.ErrorException != null) {
								taskCompletionSource.SetException(r.ErrorException);
								return;
							}
							if (r.StatusCode != System.Net.HttpStatusCode.OK) {
								taskCompletionSource.SetException(new RestException { Response = r });
								return;
							}

							var result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(r.Content);
							taskCompletionSource.SetResult(result);
						} catch (Exception ex) {
							taskCompletionSource.SetException(ex);
						}
					});

			using (cancellationToken.Register(() => asyncHandle.Abort())) {
				return await taskCompletionSource.Task;
			}
		}

	}
}
