using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CFDDNS
{
    public class CloudflareClient
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private const string ApiBaseUrl = "https://api.cloudflare.com/client/v4/";

        private readonly string _email;
        private readonly string _apiKey;

        public CloudflareClient(string email, string apiKey)
        {
            _email = email;
            _apiKey = apiKey;
        }

        public async Task<(bool Success, string Message)> UpdateDnsRecordAsync(DomainConfig domain, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(domain.ZoneId) || string.IsNullOrWhiteSpace(domain.RecordId))
            {
                return (false, "Zone ID 和 Record ID 不能为空。");
            }

            var apiUrl = $"zones/{domain.ZoneId}/dns_records/{domain.RecordId}";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Put, new Uri(new Uri(ApiBaseUrl), apiUrl));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("X-Auth-Email", _email);
                request.Headers.Add("X-Auth-Key", _apiKey);

                var payload = new
                {
                    type = domain.Type,
                    name = domain.Domain,
                    content = ipAddress
                };
                
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await HttpClient.SendAsync(request);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, $"成功更新域名 {domain.Domain} 的 IP 为 {ipAddress}。");
                }
                else
                {
                    // Try to parse error from Cloudflare response
                    string errorMessage = $"更新失败。状态码: {response.StatusCode}.";
                    try
                    {
                        var jsonError = JsonDocument.Parse(responseBody);
                        if (jsonError.RootElement.TryGetProperty("errors", out var errors) && errors.GetArrayLength() > 0)
                        {
                            errorMessage += " 原因: " + errors[0].GetProperty("message").GetString();
                        }
                    }
                    catch { /* Ignore parsing errors */ }
                    
                    return (false, errorMessage);
                }
            }
            catch (Exception ex)
            {
                return (false, $"更新过程中发生异常: {ex.Message}");
            }
        }

        public async Task<(bool Success, string IpAddress, string Message)> GetDnsRecordIpAsync(DomainConfig domain)
        {
            if (string.IsNullOrWhiteSpace(domain.ZoneId) || string.IsNullOrWhiteSpace(domain.RecordId))
            {
                return (false, "", "Zone ID 和 Record ID 不能为空。");
            }

            var apiUrl = $"zones/{domain.ZoneId}/dns_records/{domain.RecordId}";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(ApiBaseUrl), apiUrl));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("X-Auth-Email", _email);
                request.Headers.Add("X-Auth-Key", _apiKey);

                var response = await HttpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = JsonDocument.Parse(responseBody);
                    if (jsonDoc.RootElement.TryGetProperty("result", out var result) &&
                        result.TryGetProperty("content", out var content))
                    {
                        return (true, content.GetString() ?? "", "查询成功。");
                    }
                    return (false, "", "成功响应，但未能解析到记录内容。");
                }
                else
                {
                    string errorMessage = $"查询失败。状态码: {response.StatusCode}.";
                     try
                    {
                        var jsonError = JsonDocument.Parse(responseBody);
                        if (jsonError.RootElement.TryGetProperty("errors", out var errors) && errors.GetArrayLength() > 0)
                        {
                            errorMessage += " 原因: " + errors[0].GetProperty("message").GetString();
                        }
                    }
                    catch { /* Ignore parsing errors */ }
                    return (false, "", errorMessage);
                }
            }
            catch (Exception ex)
            {
                return (false, "", $"查询过程中发生异常: {ex.Message}");
            }
        }
    }
} 