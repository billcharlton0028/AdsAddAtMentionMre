using System;
using System.Net;
using System.Text;

namespace AdsAddAtMentionMre
{
    class Program
    {
        // This MRE was tested using a "free" ($150/month credit) Microsoft Azure environment provided by my Visual Studio Enterprise Subscription.
        // I estabished a Windows Active Directory Domain in my Microsoft Azure environment and then installed and configured ADS on-prem.
        // The domain is composed of a domain controller server, an ADS application server, and an ADS database server.

        // enter your collection url, i.e. http://##.##.###.###/your%20collection%20name
        const string ADS_COLLECTION_NAME_URL = "http://##.##.###.#/your%20collection%20name";
        const string ADS_PROJECT_NAME = "Your Project Name";

        static void Main(string[] args)
        {
            try
            {
                if (!TestEndPoint())
                {
                    Environment.Exit(99);
                }

                // GET RELEVANT USER STORY WORK IDS

                ClsUserStoryWorkIds objUserStoryWorkIds = new ClsUserStoryWorkIds(ADS_COLLECTION_NAME_URL, ADS_PROJECT_NAME);

                // FOR EACH USER STORY ID RETRIEVED, ADD @MENTION COMMENT TO ASSIGNED PERSON

                if (objUserStoryWorkIds.IdList.WorkItems.Count > 0)
                {
                    ClsAdsComment objAdsComment = new ClsAdsComment(ADS_COLLECTION_NAME_URL, ADS_PROJECT_NAME);

                    foreach (ClsUserStoryWorkIds.WorkItem workItem in objUserStoryWorkIds.IdList.WorkItems)
                    {
                        if (objAdsComment.Add(workItem))
                        {
                            Console.WriteLine(string.Format("Comment added to ID {0}", workItem.Id));
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Comment NOT added to ID {0}", workItem.Id));
                        }
                    }
                }

                Console.ReadKey();
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                StringBuilder msg = new StringBuilder();

                Exception innerException = e.InnerException;

                msg.AppendLine(e.Message);
                msg.AppendLine(e.StackTrace);

                while (innerException != null)
                {
                    msg.AppendLine("");
                    msg.AppendLine("InnerException:");
                    msg.AppendLine(innerException.Message);
                    msg.AppendLine(innerException.StackTrace);
                    innerException = innerException.InnerException;
                }

                Console.Error.WriteLine(string.Format("An exception occured:\n{0}", msg.ToString()));
                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        private static bool TestEndPoint()
        {
            bool retVal = false;

            // This is a just a quick and dirty way to test the ADS collection endpoint. 
            // No authentication is attempted.
            // The exception "The remote server returned an error: (401) Unauthorized." 
            // represents success because it means the endpoint is responding

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ADS_COLLECTION_NAME_URL);
                request.AllowAutoRedirect = false; // find out if this site is up and BTW, don't follow a redirector
                request.Method = System.Net.WebRequestMethods.Http.Head;
                request.Timeout = 30000;
                WebResponse response = request.GetResponse();
            }
            catch (Exception e1)
            {
                if (!e1.Message.Equals("The remote server returned an error: (401) Unauthorized."))
                {
                    throw;
                }

                retVal = true;
            }

            return retVal;
        }
    }
}
