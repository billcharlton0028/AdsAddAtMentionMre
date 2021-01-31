using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace AdsAddAtMentionMre
{
    class ClsAdsComment
    {
        readonly string adsCollectionUrl;
        readonly string adsProjectName;

        public ClsAdsComment(string adsCollectionUrl, string adsProjectName)
        {
            this.adsCollectionUrl = adsCollectionUrl;
            this.adsProjectName = adsProjectName;
        }

        public bool Add(ClsUserStoryWorkIds.WorkItem workItem)
        {
            bool retVal = false;

            string httpPostRequest = string.Empty;
            string httpGetRequest = string.Empty;
            string json = string.Empty;

            string emailAddress = string.Empty;
            string emailAddressId = string.Empty;

            #region GET ASSIGNED TO METADATA BY GETTING WORK ITEM

            httpGetRequest = string.Format("{0}/{1}/_apis/wit/workitems/{2}?fields=System.AssignedTo&api-version=5.1", this.adsCollectionUrl, this.adsProjectName, workItem.Id);

            using (HttpClient httpClient = new HttpClient(new HttpClientHandler()
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

                using (HttpResponseMessage response = httpClient.GetAsync(httpGetRequest).Result)
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result;

                    ClsJsonResponse_GetWorkItem objJsonResponse_GetWorkItem = JsonConvert.DeserializeObject<ClsJsonResponse_GetWorkItem>(responseBody);

                    if (objJsonResponse_GetWorkItem.Fields.SystemAssignedTo == null)
                    {
                        // If there is not a assigned user, skip it
                        return retVal;
                    }

                    // FYI: Even if the A.D. user id that is in the assigned to field has been disabled or deleted
                    // in A.D., it will still show up ok. The @mention will be added and ADS will attempt to
                    // send the email notification
                    emailAddress = objJsonResponse_GetWorkItem.Fields.SystemAssignedTo.UniqueName;
                    emailAddressId = objJsonResponse_GetWorkItem.Fields.SystemAssignedTo.Id;
                }
            }

            #endregion GET ASSIGNED TO METADATA BY GETTING WORK ITEM

            #region ADD COMMENT

            StringBuilder sbComment = new StringBuilder();
            sbComment.Append(string.Format("<div><a href=\"#\" data-vss-mention=\"version:2.0,{0}\">@{1}</a>: This is a programatically added comment.</div>", emailAddressId, emailAddress));
            sbComment.Append("<br>");
            sbComment.Append(DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss tt"));

            httpPostRequest = string.Format("{0}/{1}/_apis/wit/workitems/{2}/comments?api-version=5.1-preview.3", this.adsCollectionUrl, this.adsProjectName, workItem.Id);

            ClsJsonRequest_AddComment objJsonRequestBody_AddComment = new ClsJsonRequest_AddComment
            {
                Text = sbComment.ToString()
            };

            json = JsonConvert.SerializeObject(objJsonRequestBody_AddComment);

            // Allowing Untrusted SSL Certificates with HttpClient
            // https://stackoverflow.com/questions/12553277/allowing-untrusted-ssl-certificates-with-httpclient

            using (HttpClient httpClient = new HttpClient(new HttpClientHandler()
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
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(new HttpMethod("POST"), httpPostRequest)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                using (HttpResponseMessage httpResponseMessge = httpClient.SendAsync(httpRequestMessage).Result)
                {
                    httpResponseMessge.EnsureSuccessStatusCode();
                    // Don't need the response, but get it anyway 
                    string jsonResponse = httpResponseMessge.Content.ReadAsStringAsync().Result;
                    retVal = true;
                }
            }

            #endregion ADD COMMENT

            return retVal;
        }

        // This is the json request body for "Add comment" as defined by 
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/comments/add?view=azure-devops-rest-5.1
        // Use https://json2csharp.com/ to create class from json body sample
        public class ClsJsonRequest_AddComment
        {
            [JsonProperty("text")]
            public string Text { get; set; }
        }

        /// <summary>
        /// <para>This is the json response body for the get work item query used in the Add method above.</para> 
        /// <para>This class was derived by capturing the string returned by: </para>
        /// <para>string responseBody = response.Content.ReadAsStringAsync().Result;</para>
        /// <para> in the Add method above and using https://json2csharp.com/ to create the ClsJsonResponse_GetWorkItem class.</para>
        /// </summary>
        public class ClsJsonResponse_GetWorkItem
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("rev")]
            public int Rev { get; set; }

            [JsonProperty("fields")]
            public Fields Fields { get; set; }

            [JsonProperty("_links")]
            public Links Links { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }
        }

        public class Avatar
        {
            [JsonProperty("href")]
            public string Href { get; set; }
        }

        public class Links
        {
            [JsonProperty("avatar")]
            public Avatar Avatar { get; set; }

            [JsonProperty("self")]
            public Self Self { get; set; }

            [JsonProperty("workItemUpdates")]
            public WorkItemUpdates WorkItemUpdates { get; set; }

            [JsonProperty("workItemRevisions")]
            public WorkItemRevisions WorkItemRevisions { get; set; }

            [JsonProperty("workItemComments")]
            public WorkItemComments WorkItemComments { get; set; }

            [JsonProperty("html")]
            public Html Html { get; set; }

            [JsonProperty("workItemType")]
            public WorkItemType WorkItemType { get; set; }

            [JsonProperty("fields")]
            public Fields Fields { get; set; }
        }

        public class SystemAssignedTo
        {
            [JsonProperty("displayName")]
            public string DisplayName { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("_links")]
            public Links Links { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("uniqueName")]
            public string UniqueName { get; set; }

            [JsonProperty("imageUrl")]
            public string ImageUrl { get; set; }

            [JsonProperty("descriptor")]
            public string Descriptor { get; set; }
        }

        public class Fields
        {
            [JsonProperty("System.AssignedTo")]
            public SystemAssignedTo SystemAssignedTo { get; set; }

            [JsonProperty("href")]
            public string Href { get; set; }
        }

        public class Self
        {
            [JsonProperty("href")]
            public string Href { get; set; }
        }

        public class WorkItemUpdates
        {
            [JsonProperty("href")]
            public string Href { get; set; }
        }

        public class WorkItemRevisions
        {
            [JsonProperty("href")]
            public string Href { get; set; }
        }

        public class WorkItemComments
        {
            [JsonProperty("href")]
            public string Href { get; set; }
        }

        public class Html
        {
            [JsonProperty("href")]
            public string Href { get; set; }
        }

        public class WorkItemType
        {
            [JsonProperty("href")]
            public string Href { get; set; }
        }
    }
}
