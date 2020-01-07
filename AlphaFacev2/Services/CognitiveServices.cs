using AlphaFacev2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AlphaFacev2.Services
{
    public class CognitiveServices
    {
        const string subscriptionKey = "9d1d213b82b440e881893324ed33cab9";
        const string uriBase = "https://northeurope.api.cognitive.microsoft.com/face/v1.0/";

        public async Task<VerifiedFace> VerifyAsync(Stream firstFile, Stream secondFile)
        {
            using (var client = new HttpClient())
            {
                // Request Headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                var uri = uriBase + "detect?returnFaceId=true";

                // PREPARE FIRST CALL TO COGNITIVE SERVICES (GET FIRST FaceId)
                var content1 = new StreamContent(firstFile);
                content1.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response1 = await client.PostAsync(uri, content1);
                var result1 = await response1.Content.ReadAsStringAsync();
                var face1 = JsonConvert.DeserializeObject<IList<DetectedFace>>(result1);
                string faceId1 = null;
                if (face1.Count != 0) //(face1.Count != 0 && face1.Count <= 1)
                {
                    faceId1 = face1.FirstOrDefault().FaceId.ToString();
                }
                else
                {
                    return new VerifiedFace();
                }

                // PREPARE SECOND CALL TO COGNITIVE SERVICES (GET SECOND FaceId)
                var content2 = new StreamContent(secondFile);
                content2.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response2 = await client.PostAsync(uri, content2);
                var result2 = await response2.Content.ReadAsStringAsync();
                var face2 = JsonConvert.DeserializeObject<IList<DetectedFace>>(result2);
                string faceId2 = null;
                if (face2.Count != 0)
                {
                    faceId2 = face2.FirstOrDefault().FaceId.ToString();
                }
                else
                {
                    return new VerifiedFace();
                }

                // PREPARE THIRD CALL TO COGNITIVE SERVICES (VERIFY BY BOTH faceIDs)
                uri = uriBase + "verify";
                var json = new { faceId1 = $"{faceId1}", faceId2 = $"{faceId2}" };
                string jsonInString = JsonConvert.SerializeObject(json);
                var content3 = new StringContent(jsonInString, Encoding.UTF8, "application/json");
                var response3 = await client.PostAsync(uri, content3);
                var result3 = await response3.Content.ReadAsStringAsync();
                var verify = JsonConvert.DeserializeObject<VerifiedFace>(result3);

                return verify;
            }
        }
    }
}
