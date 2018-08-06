using System;
using System.Collections.Generic;
using System.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreV4Samples
{
    class Program
    {
        static void Main()
        {
            MainAsync().Wait();
        }

        public static async Task MainAsync()
        {
            HttpClient client = new HttpClient();
            //const string accessToken = "add oauth2 access token";
            const string accessToken = "Intui1t_IAM_Authentication intuit_appid=Intuit.smallbusiness.v4testplatform.overwatchtestclient,intuit_app_secret=preprdE0GyKc0aiYkbfcZI5l9mn7HSa7ziQ3fu7Z,intuit_token_type=IAM-Ticket,intuit_token=V1-229-a36yj9muyseircr1jnrioc ,intuit_userid=219082149,intuit_realmid=123146712694899";
            const string graphQlUrl = "https://v4thirdparty-e2e.api.intuit.com/graphql";
            client.DefaultRequestHeaders.Add("provider_override_scheme", "payroll");
            //client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            client.DefaultRequestHeaders.Add("Authorization", $"{accessToken}");
            /*
            //Employee Compensation
            const string employeeCompensationQuery = "{\"query\":\"query {\\n  company {\\n    employeeContacts {\\n      edges {\\n        node {\\n          id\\n          person {\\n            givenName\\n          }\\n          profiles {\\n            employee {\\n              compensations {\\n                edges {\\n                  node {\\n                    id\\n                    amount\\n                    employerCompensation {\\n                      name\\n                    }\\n                  }\\n                }\\n              }\\n            }\\n          }\\n        }\\n      }\\n    }\\n  }\\n} \"}";
            StringContent queryString = new StringContent(employeeCompensationQuery, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(graphQlUrl, queryString);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Employee Compensation Response: {responseContent}");

            //Employer Compensation
            const string employerCompensationQuery = "{\"query\":\"query {\\n company {\\n companyInfo {\\n employerInfo {\\n compensations {\\n edges {\\n node {\\n id\\n name\\n statutoryCompensationPolicy\\n            }\\n          }\\n        }\\n      }\\n    }\\n  }\\n} \",\"variables\":null}";
            queryString = new StringContent(employerCompensationQuery, Encoding.UTF8, "application/json");
            response = await client.PostAsync(graphQlUrl, queryString);
            responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Employer Compensation Response: {responseContent}");
            */
            getEmployeeCompensation(client, graphQlUrl);
        }

        private static async void getEmployeeCompensation(HttpClient httpClient, String graphQlUrl)
        {
            try
            {
                Console.WriteLine("Performing V4 Employee Compensation Query");
                const string employeeCompensationQuery = "{\"query\":\"query {\\n  company {\\n    employeeContacts {\\n      edges {\\n        node {\\n          id\\n          person {\\n            givenName\\n          }\\n          profiles {\\n            employee {\\n              compensations {\\n                edges {\\n                  node {\\n                    id\\n                    amount\\n                    employerCompensation {\\n                      name\\n                    }\\n                  }\\n                }\\n              }\\n            }\\n          }\\n        }\\n      }\\n    }\\n  }\\n} \"}";
                StringContent queryString = new StringContent(employeeCompensationQuery, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(graphQlUrl, queryString);
                var responseContent = await response.Content.ReadAsStringAsync();
                JsonValue jsonResponse = JsonValue.Parse(responseContent);
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
                        Console.WriteLine($"{employeeName} with ID {employeeId} has the compensation {compensationName} with the ID {compensationId} and current amount of ${compensationAmount}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Employee Compensation API: {ex.Message}");
            }
        }
    }
}