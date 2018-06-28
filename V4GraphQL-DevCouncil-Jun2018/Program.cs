using System;
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
			await readAllTransactions();
		}

		private static async Task readAllTransactions()
		{
			try
			{
				Console.WriteLine("***Started Reading All Transactions***");
				string transactionsReadAllGraphQl = System.IO.File.ReadAllText("IntuitGraphQL/transactions-read-all.json");
				JsonValue jsonResponse = await executeIntuitGraphQLRequest(transactionsReadAllGraphQl, "");
				foreach (JsonObject transaction in jsonResponse["data"]["company"]["transactions"]["edges"]){
					Console.WriteLine($"Read a transaction with type {transaction["type"]}");
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
				var content = $"{{\"query\":{JsonConvert.ToString(graphQl)},\"variables\":null}}";
				var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
				var httpResponse = await httpClient.PostAsync(intuitGraphQlEndpoint, httpContent);
				if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK) { throw new Exception("Non-200 status code returned from API call"); }
				var httpResponseContent = await httpResponse.Content.ReadAsStringAsync();
				return JsonValue.Parse(httpResponseContent);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				Console.WriteLine("***Finished Executing Intuit GraphQL Request***");
			}
		}


		private static string getIntuitBearerToken()
		{
			return "eyJlbmMiOiJBMTI4Q0JDLUhTMjU2IiwiYWxnIjoiZGlyIn0..mO-WmCoPIkf2d0TZwk3Zfw.evZSbLUCoBEemZOPR3RxhHl3oOnr77_SkE700Ca0xW7zDh48oa45WCy4i_ZFcT-DQyoai5Mur-Z60911ttgj94hLDC7FUxDEi_MSYo28aurLQswAgYki6RXGFQmJDbDGIhBj8aCuXltQ1VT3R3KV03LsdKTcWXB_WQMKAWYNeq1_Zinb8P8umWv3I7eOuM9IGusrUWpKamN2eU3XYTJ9zrmI6YHIUjz9g2U8DM2JQn6VZ8huqyNyRwuOIasXMWPFBMIqQJOxfnD5KwQtmTGdjzm_QD7mJGySUlBoLmGZ2b2RQsfd9lye5pr6Z3uObc1a2WUK6lUFg2HyB8a2HcexKE64Hp6N1BqxejGar_62uISXicYz6vcBk_F8EpOHDiu-lGjdkIiES-kl_WHWqoEUNuGUj44NCk4LMxyjBHT99kyYFU8PBge1izl4dCa0gvdlhGwXToYJJi14IuVqoetjLwH8ZzKGUPlAyEmRJjab4iTKvN3bBE8ha3ZaaFqdeL2jcS4JxXhHCQ8us3XrWriShd5VpAxpFI9sqvRoq3QD90-Vr5pnh63nAvaodMaboPr8a5sL4bbfxzS6_X_XhBVEBywEEyLSjAucIEv4t4lTV-GhGotPnZOfPV0REXmhjybbiR-ulhJhjCD2CWfAapbqjl3Uydu4qJCY_aEd96LxYIckMgBY0tuzU_IR8BVJZ8-N.Xmy1SmOuffXPbsJWa4awpw";
		}




		private static async Task getEmployeeCompensation(HttpClient httpClient, String graphQlUrl)
		{
			try
			{
				Console.WriteLine("***Performing V4 Employee Compensation Query***");
				const string employeeCompensationQuery = "{\"query\":\"query {\\n  company {\\n    employeeContacts {\\n      edges {\\n        node {\\n          id\\n          person {\\n            givenName\\n          }\\n          profiles {\\n            employee {\\n              compensations {\\n                edges {\\n                  node {\\n                    id\\n                    amount\\n                    employerCompensation {\\n                      name\\n                    }\\n                  }\\n                }\\n              }\\n            }\\n          }\\n        }\\n      }\\n    }\\n  }\\n} \"}";
				StringContent queryString = new StringContent(employeeCompensationQuery, Encoding.UTF8, "application/json");
				var response = await httpClient.PostAsync(graphQlUrl, queryString);
				if (response.StatusCode != System.Net.HttpStatusCode.OK) { throw new Exception("Non-200 status code returned from API call"); }
				var responseContent = await response.Content.ReadAsStringAsync();
				JsonValue jsonResponse = JsonValue.Parse(responseContent);
				if (!jsonResponse.ContainsKey("data") || jsonResponse.ContainsKey("errors")) { throw new Exception("Error returned in JSON response"); }
				foreach (JsonObject employeeEdge in jsonResponse["data"]["company"]["employeeContacts"]["edges"])
				{
					JsonValue employeeNode = employeeEdge["node"];
					var employeeId = (string)employeeNode["id"];
					var employeeName = (string)employeeNode["person"]["givenName"];
					foreach (JsonObject compensationEdge in employeeNode["profiles"]["employee"]["compensations"]["edges"])
					{
						JsonValue compensationNode = compensationEdge["node"];
						var compensationId = (string)compensationNode["id"];
						var compensationName = (string)compensationNode["employerCompensation"]["name"];
						var compensationAmount = compensationNode["amount"] == null ? 0 : Double.Parse((string)compensationNode["amount"]);
						Console.WriteLine($"Employee {employeeName} with ID {employeeId} has the compensation {compensationName} with the ID {compensationId} and current amount of ${compensationAmount}");
					}
				}
				Console.WriteLine("***V4 Employee Compensation Query Success***");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"***Error calling Employee Compensation API: {ex.Message}***");
			}
			Console.WriteLine("");
		}


		private static async Task getEmployerCompensation(HttpClient httpClient, String graphQlUrl)
		{
			try
			{
				Console.WriteLine("***Performing V4 Employer Compensation Query***");
				const string employerCompensationQuery = "{\"query\":\"query {\\n company {\\n companyInfo {\\n employerInfo {\\n compensations {\\n edges {\\n node {\\n id\\n name\\n statutoryCompensationPolicy\\n            }\\n          }\\n        }\\n      }\\n    }\\n  }\\n} \",\"variables\":null}";
				StringContent queryString = new StringContent(employerCompensationQuery, Encoding.UTF8, "application/json");
				var response = await httpClient.PostAsync(graphQlUrl, queryString);
				if (response.StatusCode != System.Net.HttpStatusCode.OK) { throw new Exception("Non-200 status code returned from API call"); }
				var responseContent = await response.Content.ReadAsStringAsync();
				JsonValue jsonResponse = JsonValue.Parse(responseContent);
				if (!jsonResponse.ContainsKey("data") || jsonResponse.ContainsKey("errors")) { throw new Exception("Error returned in JSON response"); }
				foreach (JsonObject employeerCompensationEdge in jsonResponse["data"]["company"]["companyInfo"]["employerInfo"]["compensations"]["edges"])
				{
					JsonValue employeerCompensationNode = employeerCompensationEdge["node"];
					var compensationId = (string)employeerCompensationNode["id"];
					var compensationName = (string)employeerCompensationNode["name"];
					var compensationPolicy = (string)employeerCompensationNode["statutoryCompensationPolicy"];
					Console.WriteLine($"Employer has compensation {compensationName} with ID {compensationId} and policy {compensationPolicy}");
				}
				Console.WriteLine("***V4 Employer Compensation Query Success***");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"***Error calling Employer Compensation API: {ex.Message}***");
			}
			Console.WriteLine("");
		}

		static void Main()
		{
			MainAsync().Wait();
		}

	}
}