using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreVerifiableCredentials
{
    [Route("api/[controller]/[action]")]
    public class VerifierController : Controller
    {
        const string PRESENTATIONPAYLOAD = "presentation_request_config.json";
        //        const string PRESENTATIONPAYLOAD = "presentation_request_config - TrueIdentitySample.json";

        protected readonly AppSettingsModel AppSettings;
        protected IMemoryCache _cache;
        protected readonly ILogger<VerifierController> _log;
        private IHttpClientFactory _httpClientFactory;
        private string _apiKey;
        public VerifierController(IOptions<AppSettingsModel> appSettings, IMemoryCache memoryCache, ILogger<VerifierController> log, IHttpClientFactory httpClientFactory)
        {
            this.AppSettings = appSettings.Value;
            _cache = memoryCache;
            _log = log;
            _httpClientFactory = httpClientFactory;
            _apiKey = System.Environment.GetEnvironmentVariable("API-KEY");
        }


        /// <summary>
        /// This method creates a session which can then be used for the rest of the calls
        /// </summary>
        /// <returns>JSON object with a GUID representing the state, which is effectively a session ID</returns>
        [HttpGet("/api/verifier/create-session")]
        public ActionResult CreateSession()
        {
            try
            {
                string state = Guid.NewGuid().ToString();

                JObject returnContent = new JObject();
                returnContent.Add(new JProperty("id", state));
                var jsonString = JsonConvert.SerializeObject(returnContent);
                _cache.Set(state, jsonString);

                return new ContentResult { ContentType = "application/json", Content = jsonString };
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "400", error_description = ex.Message });
            }
        }


        /// <summary>
        /// This method is called from VerifierBio UI after it has finished capturing an image. It stores the image and returns a state.
        /// </summary>
        /// <returns>JSON object with the state, which can be used in /presengtation-request to reload the image from cache</returns>
        [HttpPost("/api/verifier/submit-image")]
        public async Task<ActionResult> SubmitImage([FromForm] Microsoft.AspNetCore.Http.IFormFile image, [FromForm] string id)
        {
            try
            {
                // convert image to Base64
                byte[] imageByteContent = null;
                MemoryStream ms = new MemoryStream();
                image.CopyTo(ms);
                imageByteContent = ms.ToArray();
                var imageBase64 = System.Convert.ToBase64String(imageByteContent);

                // check if there is a state, otherwise create one
                string state = Request.Query["id"];
                if (state == null)
                {
                    return BadRequest(new { error = "400", error_description = "Parameter 'id' is missing from call"});
                }

                // store image in cache
                JObject toCache = new JObject();
                toCache.Add(new JProperty("id", state));
                toCache.Add(new JProperty("image", imageBase64));
                var jsonString = JsonConvert.SerializeObject(toCache);
                _cache.Set(state, jsonString);
                return new ContentResult { ContentType = "application/json", Content = jsonString };
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "400", error_description = ex.Message });
            }
        }

        /// <summary>
        /// This method is called from the UI to initiate the presentation of the verifiable credential
        /// </summary>
        /// <returns>JSON object with the address to the presentation request and optionally a QR code and a state value which can be used to check on the response status</returns>
        [HttpGet("/api/verifier/presentation-request")]
        public async Task<ActionResult> PresentationRequest(string id)
        {
            try
            {

                string jsonString = null;
                string state = Request.Query["id"];
                if (string.IsNullOrEmpty(state)) {
                    return BadRequest(new { error = "400", error_description = "Missing argument 'id'" });
                }

                // load image from cache
                byte[] imageByteContent = null;
                if (_cache.TryGetValue(state, out string buf))
                {
                    var value = JObject.Parse(buf);
                    imageByteContent = Convert.FromBase64String(value["image"].ToString());
                }
                else
                {
                    return BadRequest(new { error = "400", error_description = "Image not submitted or not found" });
                }


                // clean the cache
//                _cache.Remove(state);

                // submit image to Face API
                string response = null;
                var client = _httpClientFactory.CreateClient();

                // SUBSCRIPTION KEY
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "YOURAPIKEY");

                // place in a HttpContent
                HttpContent byteContent = new ByteArrayContent(imageByteContent);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Add image to get ID
                HttpResponseMessage res = await client.PostAsync(
                    "https://YOURREGION.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=true",
                    byteContent
                );
                response = await res.Content.ReadAsStringAsync();
                if (res.StatusCode != HttpStatusCode.OK)
                {
                    return BadRequest(new { error = "400", error_description = "Failed to submit to Face API" });
                }

                var faceId = response.Substring(12, 36);
                /* Finish submitting image */
                // call Verify API to compare two FaceIds should happen later down the code, after VC is verified and before final return


                //they payload template is loaded from disk and modified in the code below to make it easier to get started
                //and having all config in a central location appsettings.json. 
                //if you want to manually change the payload in the json file make sure you comment out the code below which will modify it automatically
                //
                string payloadpath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), PRESENTATIONPAYLOAD);
                _log.LogTrace("IssuanceRequest file: {0}", payloadpath);
                if (!System.IO.File.Exists(payloadpath))
                {
                    _log.LogError("File not found: {0}", payloadpath);
                    return BadRequest(new { error = "400", error_description = PRESENTATIONPAYLOAD + " not found" });
                }
                jsonString = System.IO.File.ReadAllText(payloadpath);
                if (string.IsNullOrEmpty(jsonString))
                {
                    _log.LogError("Error reading file: {0}", payloadpath);
                    return BadRequest(new { error = "400", error_description = PRESENTATIONPAYLOAD + " error reading file" });
                }


                // state = Guid.NewGuid().ToString();

                //modify payload with new state, the state is used to be able to update the UI when callbacks are received from the VC Service
                JObject payload = JObject.Parse(jsonString);
                if (payload["callback"]["state"] != null)
                {
                    payload["callback"]["state"] = state;
                }

                //modify the callback method to make it easier to debug with tools like ngrok since the URI changes all the time
                //this way you don't need to modify the callback URL in the payload every time ngrok changes the URI
                if (payload["callback"]["url"] != null)
                {
                    //localhost hostname can't work for callbacks so we won't overwrite it.
                    //this happens for example when testing with sign-in to an IDP and https://localhost is used as redirect URI
                    //in that case the callback should be configured in the payload directly instead of being modified in the code here
                    string host = GetRequestHostName();
                    if (!host.Contains("//localhost"))
                    {
                        payload["callback"]["url"] = String.Format("{0}:/api/verifier/presentationCallback", host);
                    }
                }

                // set our api-key in the request so we can check it in the callbacks we receive
                if (payload["callback"]["headers"]["api-key"] != null)
                {
                    payload["callback"]["headers"]["api-key"] = this._apiKey;
                }

                jsonString = JsonConvert.SerializeObject(payload);

                //CALL REST API WITH PAYLOAD
                HttpStatusCode statusCode = HttpStatusCode.OK;
                response = null;
                try
                {
                    //The VC Request API is an authenticated API. We need to clientid and secret (or certificate) to create an access token which 
                    //needs to be send as bearer to the VC Request API
                    var accessToken = await GetAccessToken();
                    if (accessToken.Item1 == String.Empty)
                    {
                        _log.LogError(String.Format("failed to acquire accesstoken: {0} : {1}"), accessToken.error, accessToken.error_description);
                        return BadRequest(new { error = accessToken.error, error_description = accessToken.error_description });
                    }

                    _log.LogTrace( $"Request API payload: {jsonString}" );
                    client = _httpClientFactory.CreateClient();
                    var defaultRequestHeaders = client.DefaultRequestHeaders;
                    defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.token);

                    res = await client.PostAsync(AppSettings.ApiEndpoint, new StringContent(jsonString, Encoding.UTF8, "application/json"));
                    response = await res.Content.ReadAsStringAsync();
                    statusCode = res.StatusCode;

                    if (statusCode == HttpStatusCode.Created)
                    {
                        _log.LogTrace("succesfully called Request API");
                        JObject requestConfig = JObject.Parse(response);
                        requestConfig.Add(new JProperty("id", state));
                        jsonString = JsonConvert.SerializeObject(requestConfig);

                        //We use in memory cache to keep state about the request. The UI will check the state when calling the presentationResponse method

                        var cacheData = new
                        {
                            status = "notscanned",
                            message = "Request ready, please scan with Authenticator",
                            expiry = requestConfig["expiry"].ToString(),
                            faceId = faceId
                        };
                        _cache.Set(state, JsonConvert.SerializeObject(cacheData));

                        //the response from the VC Request API call is returned to the caller (the UI). It contains the URI to the request which Authenticator can download after
                        //it has scanned the QR code. If the payload requested the VC Request service to create the QR code that is returned as well
                        //the javascript in the UI will use that QR code to display it on the screen to the user.

                        return new ContentResult { ContentType = "application/json", Content = jsonString };
                    }
                    else
                    {
                        _log.LogError("Unsuccesfully called Request API");
                        return BadRequest(new { error = "400", error_description = "Something went wrong calling the API: " + response });
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = "400", error_description = "Something went wrong calling the API: " + ex.Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "400", error_description = ex.Message });
            }
        }

        /// <summary>
        /// This method is called by the VC Request API when the user scans a QR code and presents a Verifiable Credential to the service
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> PresentationCallback()
        {
            try
            {

                // Stop here. The problem is that it takes a state from the callback, rather than from the state we have passed on. Need to investigate why it does that.


                string content = await new System.IO.StreamReader(this.Request.Body).ReadToEndAsync();
                _log.LogTrace("callback!: " + content);
                this.Request.Headers.TryGetValue("api-key", out var apiKey);
                if (this._apiKey != apiKey) 
                {
                    _log.LogTrace("api-key wrong or missing");
                    return new ContentResult() { StatusCode = (int)HttpStatusCode.Unauthorized, Content = "api-key wrong or missing" };
                }               
                JObject presentationResponse = JObject.Parse(content);
                var state = presentationResponse["state"].ToString();

                //there are 2 different callbacks. 1 if the QR code is scanned (or deeplink has been followed)
                //Scanning the QR code makes Authenticator download the specific request from the server
                //the request will be deleted from the server immediately.
                //That's why it is so important to capture this callback and relay this to the UI so the UI can hide
                //the QR code to prevent the user from scanning it twice (resulting in an error since the request is already deleted)
                if (presentationResponse["code"].ToString() == "request_retrieved")
                {
                    // Read what is now in the cache to get faceId ...
                    JObject value = null;
                    if (_cache.TryGetValue(state, out string buf)) {
                        value = JObject.Parse(buf);
                    }

                    // ... so that we can cotninue to include it withn the new cache contents
                    var cacheData = new
                    {
                        status = "request_retrieved",
                        message = "QR Code is scanned. Waiting for validation...",
                        faceId = value["faceId"]
                    };
                    _cache.Set(state, JsonConvert.SerializeObject(cacheData));
                }

                // the 2nd callback is the result with the verified credential being verified.
                // typically here is where the business logic is written to determine what to do with the result
                // the response in this callback contains the claims from the Verifiable Credential(s) being presented by the user
                // In this case the result is put in the in memory cache which is used by the UI when polling for the state so the UI can be updated.
                if (presentationResponse["code"].ToString() == "presentation_verified")
                {

                    /* 
                     * Verify the two images using Verify API 
                    */

                    // Prepare http client to submit to Face API
                    string response = null;
                    var client = _httpClientFactory.CreateClient();
                    // SUBSCRIPTION KEY
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "YOURAPIKEY");

                    // Gather the Face IDs
                    // faceId1 is in variable faceId which is in the cache
                    JObject value = null;
                    if (_cache.TryGetValue(state, out string buf))
                    {
                        value = JObject.Parse(buf);
                    }
                    var faceId1 = value["faceId"];

                    // faceId 2 is in the credential which is included in the callback data that called this function
                    var faceId2 = presentationResponse["issuers"][0]["claims"]["faceId"];

                    // place in a HttpContent
                    var contentData = new {
                        faceId1 = faceId1,
                        faceId2 = faceId2
                    };
                    HttpContent stringContent = new StringContent(JsonConvert.SerializeObject(contentData), Encoding.UTF8, "application/json");

                    // Make the call to Verify API
                    HttpResponseMessage res = await client.PostAsync(
                        "https://YOURREGION.api.cognitive.microsoft.com/face/v1.0/verify",
                        stringContent
                    );
                    response = await res.Content.ReadAsStringAsync();
                    if (res.StatusCode != HttpStatusCode.OK)
                    {
                        return BadRequest(new { error = "400", error_description = "Failed to submit to Verify API" });
                    }

                    var comparisonResult = JObject.Parse(response);
                    /* End image verification */


                    // examine the response to determine if the images are identical
                    if ((bool)comparisonResult["isIdentical"] == true)
                    {
                        try
                        {


                            // The pointless code below is for easier debug, in case an attribute is missing, we get a veri generic exception on "cacheData = new { ... }" below
                            // So here we set the attributes one by one and if there is a problem we will get an exception at the specific attribute
                            var payload = presentationResponse["issuers"].ToString();
                            var subject = presentationResponse["subject"].ToString();
                            var name = presentationResponse["issuers"][0]["claims"]["name"].ToString();
                            var username = presentationResponse["issuers"][0]["claims"]["username"].ToString();
                            var oid = presentationResponse["issuers"][0]["claims"]["oid"].ToString();
                            var tenantId = presentationResponse["issuers"][0]["claims"]["tenantId"].ToString();
                            var faceId = presentationResponse["issuers"][0]["claims"]["faceId"].ToString();


                            var cacheData = new
                            {
                                status = "presentation_verified",
                                message = "Presentation verified",
                                payload = presentationResponse["issuers"].ToString(),
                                subject = presentationResponse["subject"].ToString(),
                                name = presentationResponse["issuers"][0]["claims"]["name"].ToString(),
                                username = presentationResponse["issuers"][0]["claims"]["username"].ToString(),
                                oid = presentationResponse["issuers"][0]["claims"]["oid"].ToString(),
                                tenantId = presentationResponse["issuers"][0]["claims"]["tenantId"].ToString(),
                                faceId = presentationResponse["issuers"][0]["claims"]["faceId"].ToString(),
                                faceId1 = faceId1,
                                faceId2 = faceId2,
                                isIdentical = "true",
                                confidence = comparisonResult["confidence"]
                            };
                            _cache.Remove(state);
                            _cache.Set(state, JsonConvert.SerializeObject(cacheData));
                        }
                        catch (Exception ex)
                        {
                            return BadRequest(new { error = "400", error_description = "The credential did not contain all the required fields." });
                        }
                    }
                    else
                    {
                        var cacheData = new { message = "Biometric check failed. Please try when being the owner of the credential." };
                        return BadRequest(new { error = "400", error_description = "Biometric check failed. Please try when being the owner of the credential." });
                    }
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "400", error_description = ex.Message });
            }
        }
        //
        //this function is called from the UI polling for a response from the AAD VC Service.
        //when a callback is recieved at the presentationCallback service the session will be updated
        //this method will respond with the status so the UI can reflect if the QR code was scanned and with the result of the presentation
        //
        [HttpGet("/api/verifier/presentation-response")]
        public async Task<ActionResult> PresentationResponse()
        {
            try
            {
                //the id is the state value initially created when the issuanc request was requested from the request API
                //the in-memory database uses this as key to get and store the state of the process so the UI can be updated
                string state = this.Request.Query["id"];
                if (string.IsNullOrEmpty(state)) {
                    return BadRequest(new { error = "400", error_description = "Missing argument 'id'" });
                }
                JObject value = null;
                if (_cache.TryGetValue(state, out string buf))
                {
//                    _log.LogTrace( $"id {state}, cache: {buf}");
                    try {
                        value = JObject.Parse(buf);
                    }
                    catch (Exception e) {
                        _log.LogTrace("/presentation-response was called but the verified credential is not ready yet (cache doesn't have parse-able JSON object)");
                        return new OkResult();
                    }

                    if (value.ContainsKey("faceId2")) {
                        _log.LogTrace("Response value:\n{0}", value);

                    }

                    return new ContentResult { ContentType = "application/json", Content = JsonConvert.SerializeObject(value) };
                }
                return new OkResult();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "400", error_description = ex.Message });
            }
        }
        //some helper functions
        protected async Task<(string token, string error, string error_description)> GetAccessToken()
        {
            // You can run this sample using ClientSecret or Certificate. The code will differ only when instantiating the IConfidentialClientApplication
            bool isUsingClientSecret = AppSettings.AppUsesClientSecret(AppSettings);

            // Since we are using application permissions this will be a confidential client application
            IConfidentialClientApplication app;
            if (isUsingClientSecret)
            {
                app = ConfidentialClientApplicationBuilder.Create(AppSettings.ClientId)
                    .WithClientSecret(AppSettings.ClientSecret)
                    .WithAuthority(new Uri(AppSettings.Authority))
                    .Build();
            }
            else
            {
                X509Certificate2 certificate = AppSettings.ReadCertificate(AppSettings.CertificateName);
                app = ConfidentialClientApplicationBuilder.Create(AppSettings.ClientId)
                    .WithCertificate(certificate)
                    .WithAuthority(new Uri(AppSettings.Authority))
                    .Build();
            }

            //configure in memory cache for the access tokens. The tokens are typically valid for 60 seconds,
            //so no need to create new ones for every web request
            app.AddDistributedTokenCache(services =>
            {
                services.AddDistributedMemoryCache();
                services.AddLogging(configure => configure.AddConsole())
                .Configure<LoggerFilterOptions>(options => options.MinLevel = Microsoft.Extensions.Logging.LogLevel.Debug);
            });

            // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the 
            // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
            // a tenant administrator. 
            string[] scopes = new string[] { AppSettings.VCServiceScope };

            AuthenticationResult result = null;
            try
            {
                result = await app.AcquireTokenForClient(scopes)
                    .ExecuteAsync();
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
                // Mitigation: change the scope to be as expected
                return (string.Empty, "500", "Scope provided is not supported");
                //return BadRequest(new { error = "500", error_description = "Scope provided is not supported" });
            }
            catch (MsalServiceException ex)
            {
                // general error getting an access token
                return (String.Empty, "500", "Something went wrong getting an access token for the client API:" + ex.Message);
                //return BadRequest(new { error = "500", error_description = "Something went wrong getting an access token for the client API:" + ex.Message });
            }

            _log.LogTrace(result.AccessToken);
            return (result.AccessToken, String.Empty, String.Empty);
        }
        protected string GetRequestHostName()
        {
            string scheme = "https";// : this.Request.Scheme;
            string originalHost = this.Request.Headers["x-original-host"];
            string hostname = "";
            if (!string.IsNullOrEmpty(originalHost))
                hostname = string.Format("{0}://{1}", scheme, originalHost);
            else hostname = string.Format("{0}://{1}", scheme, this.Request.Host);
            return hostname;
        }
    }
}
