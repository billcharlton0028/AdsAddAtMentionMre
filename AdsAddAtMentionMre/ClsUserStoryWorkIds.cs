using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AdsAddAtMentionMre
{
    public class ClsUserStoryWorkIds
    {
        ClsResponse idList = null;

        /// <summary>
        /// Get all the users story ids for user stories that match the wiql query criteria
        /// </summary>
        /// <param name="adsCollectionUrl"></param>
        /// <param name="adsProjectName"></param>
        public ClsUserStoryWorkIds(string adsCollectionUrl, string adsProjectName)
        {
            string httpPostRequest = string.Format("{0}/{1}/_apis/wit/wiql?api-version=5.1", adsCollectionUrl, adsProjectName);

            // In my case, I'm working with an ADS project that is based on a customized Agile process template.
            // I used the ADS web portal to create a customized process inherited from the standard ADS Agile process.
            // The customization includes custom fields added to the user story:
            // [Category for DC and MR] (picklist)
            // [Recurrence] (picklist)

            ClsRequest objJsonRequestBody_WiqlQuery = new ClsRequest
            {
                Query = string.Format("Select [System.Id] From WorkItems Where [System.WorkItemType] = 'User Story' and [System.TeamProject] = '{0}' and [Category for DC and MR] = 'Data Call' and [Recurrence] = 'Monthly' and [System.State] = 'Active'", adsProjectName)
            };

            string json = JsonConvert.SerializeObject(objJsonRequestBody_WiqlQuery);

            // ServerCertificateCustomValidationCallback: In my environment, we use self-signed certs, so I 
            // need to allow an untrusted SSL Certificates with HttpClient
            // https://stackoverflow.com/questions/12553277/allowing-untrusted-ssl-certificates-with-httpclient
            //
            // UseDefaultCredentials = true: Before running the progran as the domain admin, I use Windows Credential
            // Manager to create a Windows credential for the domain admin:
            // Internet address: IP of the ADS app server
            // User Name: Windows domain + Windows user account, i.e., domainName\domainAdminUserName
            // Password: password for domain admin's Windows user account

            using (HttpClient HttpClient = new HttpClient(new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    }
            }))
            {
                HttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //todo I guess I should make this a GET, not a POST, but the POST works
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(new HttpMethod("POST"), httpPostRequest)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                using (HttpResponseMessage httpResponseMessage = HttpClient.SendAsync(httpRequestMessage).Result)
                {
                    httpResponseMessage.EnsureSuccessStatusCode();

                    string jsonResponse = httpResponseMessage.Content.ReadAsStringAsync().Result;

                    this.IdList = JsonConvert.DeserializeObject<ClsResponse>(jsonResponse);
                }
            }
        }

        public ClsResponse IdList { get => idList; set => idList = value; }

        /// <summary>
        /// <para>This is the json request body for a WIQL query as defined by</para>
        /// <para>https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/wiql/query%20by%20wiql?view=azure-devops-rest-5.1</para>
        /// <para>Use https://json2csharp.com/ to create class from json request body sample</para>
        /// </summary>
        public class ClsRequest
        {
            [JsonProperty("query")]
            public string Query { get; set; }
        }

        /// <summary>
        /// <para>This is the json response body for the WIQL query used in this class.</para> 
        /// <para>This class was derived by capturing the string returned by: </para>
        /// <para>httpResponseMessage.Content.ReadAsStringAsync().Result</para>
        /// <para> in the CTOR above and using https://json2csharp.com/ to create the ClsResponse class.</para>
        /// </summary>
        public class ClsResponse
        {
            [JsonProperty("queryType")]
            public string QueryType { get; set; }

            [JsonProperty("queryResultType")]
            public string QueryResultType { get; set; }

            [JsonProperty("asOf")]
            public DateTime AsOf { get; set; }

            [JsonProperty("columns")]
            public List<Column> Columns { get; set; }

            [JsonProperty("workItems")]
            public List<WorkItem> WorkItems { get; set; }
        }

        public class Column
        {
            [JsonProperty("referenceName")]
            public string ReferenceName { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }
        }

        public class WorkItem
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }
        }
    }
}
