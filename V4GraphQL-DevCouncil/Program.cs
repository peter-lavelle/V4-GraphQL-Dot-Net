using System;
using System.IO;
using System.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NetCoreV4Samples
{
	class IntuitV4Samples
	{

		const string intuitGraphQlEndpoint = "https://v4thirdparty-e2e.api.intuit.com/graphql";

		public static async Task MainAsync()
		{
			try
			{
				await readAllTransactions();
				await readOneTransaction("djQuMToxMjMxNDY3MTI2OTQ4OTk6ODAyNzFlZGQ4YQ:2");
				await readAllBillTransactions();
				await createBillTransaction("djQuMToxMjMxNDY3MTI2OTQ4OTk6OWQ2OTllOTYwOA:1608e4bc80eb340668e2765d486d971b1", "3");
				await updateBillTransaction("djQuMToxMjMxNDY3MTI2OTQ4OTk6ODAyNzFlZGQ4YQ:9", "djQuMToxMjMxNDY3MTI2OTQ4OTk6OWQ2OTllOTYwOA:1608e4bc80eb340668e2765d486d971b1", "3");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error calling Intuit APIs: {ex.Message}");
			}
			finally { }
		}

		private static async Task readAllTransactions()
		{
			try
			{
				Console.WriteLine("***Started Reading All Transactions***");
				var transactionsReadAllQuery = File.ReadAllText("IntuitGraphQL/transactions-read-all-query.graphql");
				JsonValue jsonResponse = await executeIntuitGraphQLRequest(transactionsReadAllQuery, "");
				foreach (JsonObject transaction in jsonResponse["data"]["company"]["transactions"]["edges"])
				{
					var transactionsNode = transaction["node"];
					var transactionsBalance = transactionsNode["traits"]["balance"];
					Console.WriteLine($"Read a transaction with type {transactionsNode["type"]} " +
									  $"with a balance of {transactionsBalance["balance"]} due on {transactionsBalance["dueDate"]}");
				}
			}
			catch (Exception ex) { throw ex; }
			finally
			{
				Console.WriteLine("***Finished Reading All Transactions***");
			}
		}

		private static async Task<JsonValue> executeIntuitGraphQLRequest(string graphQl, string variables)
		{
			try
			{
				Console.WriteLine("***Started Executing Intuit GraphQL Request***");
				var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {getIntuitBearerToken()}");
				var content = $"{{\"query\":{encodeGraphPayload(graphQl)},\"variables\":{encodeGraphPayload(variables)}}}";
				var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
				var httpResponse = await httpClient.PostAsync(intuitGraphQlEndpoint, httpContent);
				if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK) { throw new Exception("Non-200 status code returned from API call"); }
				var httpResponseContent = await httpResponse.Content.ReadAsStringAsync();
				var jsonResponse = JsonValue.Parse(httpResponseContent);
				if (!jsonResponse.ContainsKey("data") || jsonResponse.ContainsKey("errors")) { throw new Exception("Error returned in JSON response"); }
				return jsonResponse;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error calling Intuit GraphQL API: {ex.Message}");
			}
			finally
			{
				Console.WriteLine("***Finished Executing Intuit GraphQL Request***");
			}
		}

		private static async Task readOneTransaction(string id)
		{
			try
			{
				Console.WriteLine("***Started Reading One Transaction***");
				var transactionsReadOneQuery = File.ReadAllText("IntuitGraphQL/transactions-read-one-query.graphql");
				var transactionsBillFieldsFragment = File.ReadAllText("IntuitGraphQL/transactions-bill-fields-fragment.graphql");
				JsonValue jsonResponse = await executeIntuitGraphQLRequest(transactionsReadOneQuery + transactionsBillFieldsFragment, $"{{\"id\": \"{id}\"}}");
				var transactionNode = jsonResponse["data"]["node"];
				var vendorId = transactionNode["header"]["contact"]["profiles"]["vendor"]["contact"]["id"];
				var balance = transactionNode["traits"]["balance"]["balance"];
				var dueDate = transactionNode["traits"]["balance"]["dueDate"];
				Console.WriteLine($"Read Bill with an ID of {id}, and found a balance of {balance} due on {dueDate}");
			}
			catch (Exception ex) { throw ex; }
			finally
			{
				Console.WriteLine("***Finished Reading One Transaction***");
			}
		}

		private static async Task readAllBillTransactions()
		{
			try
			{
				Console.WriteLine("***Started Reading All Transactions***");
				var transactionsReadAllQuery = File.ReadAllText("IntuitGraphQL/transactions-read-all-bills-query.graphql");
				JsonValue jsonResponse = await executeIntuitGraphQLRequest(transactionsReadAllQuery, "");
				foreach (JsonObject transaction in jsonResponse["data"]["company"]["transactions"]["edges"])
				{
					var transactionsNode = transaction["node"];
					var transactionsBalance = transactionsNode["traits"]["balance"];
					var vendorId = transactionsNode["header"]["contact"]["profiles"]["vendor"]["contact"]["id"];
					Console.WriteLine($"Read a transaction with type {transactionsNode["type"]} " +
									  $"with a balance of {transactionsBalance["balance"]} due on {transactionsBalance["dueDate"]}" +
									  $"for a vendor with an ID of {vendorId}");
				}
			}
			catch (Exception ex) { throw ex; }
			finally
			{
				Console.WriteLine("***Finished Reading All Transactions***");
			}
		}

		private static async Task createBillTransaction(string contactId, string itemId)
		{
			try
			{
				Console.WriteLine("***Started Creating Bill Transaction***");
				var transactionsCreateMutation = File.ReadAllText("IntuitGraphQL/transactions-create-mutation.graphql");
				var transactionMutationInput = getBillCreateMutationInputJson(contactId, itemId);
				JsonValue jsonResponse = await executeIntuitGraphQLRequest(transactionsCreateMutation, transactionMutationInput);
				var transactionsNode = jsonResponse["data"]["createTransactions_Transaction"]["transactionsTransactionEdge"]["node"];
				Console.WriteLine($"Created a bill for a transaction '{transactionsNode["id"]}' on {transactionsNode["header"]["txnDate"]} for the amount of {transactionsNode["header"]["amount"]}");
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				Console.WriteLine("***Finished Creating Bill Transaction***");
			}
		}

		private static string getBillCreateMutationInputJson(string contactId, string itemId)
		{
			var clientMutationId = System.Guid.NewGuid().ToString();
			StringBuilder stringBuilder = new StringBuilder();
			StringWriter stringWriter = new StringWriter(stringBuilder);
			using (JsonWriter writer = new JsonTextWriter(stringWriter))
			{
				writer.Formatting = Formatting.Indented;
				writer.WriteStartObject();
				writer.WritePropertyName("transactions_create");
				writer.WriteStartObject();
				writer.WritePropertyName("clientMutationId");
				writer.WriteValue(clientMutationId);
				writer.WritePropertyName("transactionsTransaction");
				writer.WriteStartObject();
				writer.WritePropertyName("type");
				writer.WriteValue("PURCHASE_BILL");
				writer.WritePropertyName("header");
				writer.WriteStartObject();
				writer.WritePropertyName("privateMemo");
				writer.WriteValue("Vendor rep: Paul C.");
				writer.WritePropertyName("referenceNumber");
				writer.WriteValue("87y234587gh48057y");
				writer.WritePropertyName("amount");
				writer.WriteValue("999.00");
				writer.WritePropertyName("txnDate");
				writer.WriteValue("2018-09-20");
				writer.WritePropertyName("contact");
				writer.WriteStartObject();
				writer.WritePropertyName("id");
				writer.WriteValue(contactId);
				writer.WriteEndObject();
				writer.WriteEndObject();
				writer.WritePropertyName("lines");
				writer.WriteStartObject();
				writer.WritePropertyName("itemLines");
				writer.WriteStartObject();
				writer.WritePropertyName("amount");
				writer.WriteValue("999.00");
				writer.WritePropertyName("description");
				writer.WriteValue("Hardware");
				writer.WritePropertyName("traits");
				writer.WriteStartObject();
				writer.WritePropertyName("item");
				writer.WriteStartObject();
				writer.WritePropertyName("quantity");
				writer.WriteValue("1");
				writer.WritePropertyName("rate");
				writer.WriteValue("999.00");
				writer.WritePropertyName("item");
				writer.WriteStartObject();
				writer.WritePropertyName("id");
				writer.WriteValue(itemId);
				writer.WriteEndObject();
				writer.WriteEndObject();
				writer.WriteEndObject();
				writer.WriteEndObject();
				writer.WriteEndObject();
				writer.WriteEndObject();
			}
			return stringBuilder.ToString();
		}

		private static async Task updateBillTransaction(string id, string contactId, string itemId)
		{
			try
			{
				Console.WriteLine("***Started Updating Bill Transaction***");
				var transactionsUpdateMutation = File.ReadAllText("IntuitGraphQL/transactions-update-mutation.graphql");
				var transactionMutationInput = getBillUpdateMutationInputJson(id, contactId, itemId);
				JsonValue jsonResponse = await executeIntuitGraphQLRequest(transactionsUpdateMutation, transactionMutationInput);
				var transactionsNode = jsonResponse["data"]["updateTransactions_Transaction"]["transactionsTransaction"];
				Console.WriteLine($"Updated a bill for a transaction on {transactionsNode["header"]["txnDate"]} for the amount of {transactionsNode["header"]["amount"]}");
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				Console.WriteLine("***Finished Updating Bill Transaction***");
			}
		}

		private static string getBillUpdateMutationInputJson(string id, string contactId, string itemId)
		{
			var clientMutationId = System.Guid.NewGuid().ToString();
			StringBuilder stringBuilder = new StringBuilder();
			StringWriter stringWriter = new StringWriter(stringBuilder);
			using (JsonWriter writer = new JsonTextWriter(stringWriter))
			{
				writer.Formatting = Formatting.Indented;
				writer.WriteStartObject();
				writer.WritePropertyName("transactions_update");
				writer.WriteStartObject();
				writer.WritePropertyName("clientMutationId");
				writer.WriteValue(clientMutationId);
				writer.WritePropertyName("transactionsTransaction");
				writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue(id);
				writer.WritePropertyName("type");
				writer.WriteValue("PURCHASE_BILL");
				writer.WritePropertyName("header");
				writer.WriteStartObject();
				writer.WritePropertyName("privateMemo");
				writer.WriteValue("Vendor rep: Paul C.");
				writer.WritePropertyName("referenceNumber");
				writer.WriteValue("87y234587gh48057y");
				writer.WritePropertyName("amount");
				writer.WriteValue("1099.00");
				writer.WritePropertyName("txnDate");
				writer.WriteValue("2018-09-20");
				writer.WritePropertyName("contact");
				writer.WriteStartObject();
				writer.WritePropertyName("id");
				writer.WriteValue(contactId);
				writer.WriteEndObject();
				writer.WriteEndObject();
				writer.WritePropertyName("lines");
				writer.WriteStartObject();
				writer.WritePropertyName("itemLines");
				writer.WriteStartObject();
				writer.WritePropertyName("amount");
				writer.WriteValue("1099.00");
				writer.WritePropertyName("description");
				writer.WriteValue("Hardware");
				writer.WritePropertyName("traits");
				writer.WriteStartObject();
				writer.WritePropertyName("item");
				writer.WriteStartObject();
				writer.WritePropertyName("quantity");
				writer.WriteValue("1");
				writer.WritePropertyName("rate");
				writer.WriteValue("1099.00");
				writer.WritePropertyName("item");
				writer.WriteStartObject();
				writer.WritePropertyName("id");
				writer.WriteValue(itemId);
				writer.WriteEndObject();
				writer.WriteEndObject();
				writer.WriteEndObject();
				writer.WriteEndObject();
				writer.WriteEndObject();
				writer.WriteEndObject();
			}
			return stringBuilder.ToString();
		}

		private static string getIntuitBearerToken()
		{
			return File.ReadAllText("Auth/danger-insecure-sample-only.txt"); ;
		}


		static void Main()
		{
			MainAsync().Wait();
		}

		private static string encodeGraphPayload(string payload){
			return JsonConvert.ToString(payload);
		}

	}
}