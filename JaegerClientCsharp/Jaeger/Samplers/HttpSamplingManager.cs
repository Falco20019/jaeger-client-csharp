using System;
using System.Globalization;
using System.Net;
using Jaeger.Exceptions;
using Jaeger.Thrift.Agent;
using Newtonsoft.Json;

namespace Jaeger.Samplers
{
    public class HttpSamplingManager : ISamplingManager
    {
        public const string DEFAULT_HOST_PORT = "localhost:5778";
        private readonly string hostPort;

        /// <summary>
        /// This constructor expects running sampling manager on <value>DEFAULT_HOST_PORT</value>
        /// </summary>
        public HttpSamplingManager() : this(DEFAULT_HOST_PORT)
        {
        }

        public HttpSamplingManager(String hostPort)
        {
            this.hostPort = hostPort ?? DEFAULT_HOST_PORT;
        }


        public SamplingStrategyResponse GetSamplingStrategy(string serviceName)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var response = @"{
  ""strategyType"": 1,
  ""probabilisticSampling"": null,
  ""rateLimitingSampling"": {
    ""maxTracesPerSecond"": 2.1
  }
}";//client.DownloadString(new UriBuilder("http", this.hostPort){Query = $"service={serviceName}"}.Uri);
                    return JsonConvert.DeserializeObject<SamplingStrategyResponse>(response, new JsonSerializerSettings{Culture = CultureInfo.InvariantCulture});
                }
            }
            catch (Exception e)
            {
                throw new SamplingStrategyErrorException(
                    "http call to get sampling strategy from local agent failed.", e);
            }
        }
    }
}
