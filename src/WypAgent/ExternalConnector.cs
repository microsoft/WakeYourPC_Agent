using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WakeYourPC.WakeUpAgent
{
    // Used to make REST calls to the azure service
    internal static class ExternalConnector
    {
        private static readonly string BASE_URL = "http://wakeyourpc.cloudapp.net/v1/";
        private static readonly string WAKEUP_SUFFIX = "Users/{0}/Wakeup";
        private static readonly string MACHINE_SUFFIX = "Users/{0}/Machines";
        private static readonly string USER_SUFFIX = "Users";

        private static HttpClient restClient;

        static ExternalConnector()
        {
            restClient = new HttpClient();
            restClient.BaseAddress = new Uri(BASE_URL);
            restClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static UserEntity GetUser(string userName)
        {
            return GetResource<UserEntity>(USER_SUFFIX, userName);
        }

        public static List<MachineEntity> GetMachinesToAwake()
        {
            UserEntity user = UserEntitySingletonFactory.GetInstance();
            if(user == null)
            {
                Console.WriteLine("Cannot Get machines as the User information is not present.");
                return new List<MachineEntity>();
            }

            return GetAllResource<MachineEntity>(string.Format(WAKEUP_SUFFIX, user.Username));
        }

        public static void UpdateMachineInfo(MachineEntity machineToUpdate)
        {
            UserEntity user = UserEntitySingletonFactory.GetInstance();
            if (user == null)
            {
                Console.WriteLine("Cannot Update machine as the User information is not present.");
                return;
            } 

            // TODO check if i should use GUID as resource identifier.
            PutResource(machineToUpdate, string.Format(MACHINE_SUFFIX, user.Username), machineToUpdate.MachineName);
        }

        private static List<T> GetAllResource<T>(string uriSuffix) where T: class
        {
            List<T> outputResources = new List<T>();
            string json = restClient.GetStringAsync(uriSuffix).Result.Trim();

            if(!string.IsNullOrEmpty(json))
            {
                outputResources.AddRange(JsonConvert.DeserializeObject<List<T>>(json));
            }

            return outputResources;
        }

        private static T GetResource<T>(string uriSuffix, string resourceName) where T: class
        {
            string formattedRequestUriSuffix = string.Join("/", new string[] {uriSuffix, resourceName});
            string outputJson = restClient.GetStringAsync(formattedRequestUriSuffix).Result.Trim();

            if(!string.IsNullOrEmpty(outputJson))
            {
                return JsonConvert.DeserializeObject<T>(outputJson);
            }

            return default(T);
        }

        private static void PutResource<T>(T resource, string uriSuffix, string resourceName)
        {
            string formattedRequestUriSuffix = string.Join("/", new string[] { uriSuffix, resourceName });
            StringContent jsonContent = new StringContent(JsonConvert.SerializeObject(resource), Encoding.Unicode, "application/json");

            var response = restClient.PutAsync(formattedRequestUriSuffix, jsonContent).Result;

            if(response.IsSuccessStatusCode)
            {
                Console.WriteLine("Put resource succeeded.");
            }
            else
            {
                Console.WriteLine("Put Resource failed with error code:" + response.StatusCode);
            }
        }

    }
}
