using Newtonsoft.Json;
using System.Text;

namespace TelegramBotPdp;

public class QRCodeDataApi
{
    public static async Task<string> GetData(string firstName,string lastName)
    {
        string url = "https://api.pdp.uz/api/turniket/v1/pdp-online-students";

        string jsonData = JsonConvert.SerializeObject(new
        {
            firstName = firstName,
            lastName = lastName
        });
        using (HttpClient client = new HttpClient())
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

            string authInfo = "Basic UERQT25saW5lVHVybmlrZXQ6dFk2WEFEM3I5V0dxcFR4dmtQTktlemhCbjhNYXlGUlF3Mm0=";
            request.Headers.Add("Authorization", authInfo);

            request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.SendAsync(request);

            var r= await response.Content.ReadFromJsonAsync<Responce>();
            return r.Data;
        }
    }
}
class Responce
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Data { get; set; }
}