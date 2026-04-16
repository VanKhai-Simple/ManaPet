using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Petshop_frontend.Helpers
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>();
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>();

        public void AddRequestData(string key, string value) => _requestData.Add(key, value);
        public void AddResponseData(string key, string value) => _responseData.Add(key, value);

        public string GetResponseData(string key) => _responseData.TryGetValue(key, out var val) ? val : string.Empty;

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    // VNPAY yêu cầu Key không encode, chỉ encode Value
                    data.Append(kv.Key + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            string queryString = data.ToString();
            // Loại bỏ dấu & cuối cùng
            if (queryString.Length > 0)
            {
                queryString = queryString.Remove(queryString.Length - 1);
            }

            // Tạo mã băm từ chuỗi queryString (chưa có dấu ?)
            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, queryString);

            // Nối URL cuối cùng: baseUrl + ? + queryString + hash
            string finalUrl = baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnp_SecureHash;

            return finalUrl;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Key) && kv.Key.StartsWith("vnp_"))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            if (data.Length > 0) data.Remove(data.Length - 1, 1);
            string checkSum = HmacSHA512(secretKey, data.ToString());
            return checkSum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue) hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
    }
}