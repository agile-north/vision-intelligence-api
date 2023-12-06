using System;
using Contracts;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Contracts
{
    public class Blob
    {
        public byte[]? Data { get; set; }
        public string? DataUrl { get;set; }
        public string? Base64 { get; set; }
        public string? ContentType { get; set; }
        public string AsBase64()
        {
            if (Base64 != null)
                return Base64;

            if (DataUrl != null) {
                var du = Contracts.DataUrl.Parse(DataUrl);
                if (du.IsBase64)
                    return Convert.ToBase64String(du.Data);
            }

            return Convert.ToBase64String(Data!);
        }
        public DataUrl AsDataUrl()
        {
            if (DataUrl != null)
                return DataUrl;

            return new DataUrl(ContentType, Base64);
        }
    }
}
