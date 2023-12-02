using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace Contracts
{
    public class DataUrl
    {
        public string MimeType { get; private set; }
        public string Charset { get; private set; }
        public string Name { get; private set; }
        public bool IsBase64 { get; private set; }
        public byte[] Data { get; private set; }

        private static readonly Regex DataUrlPattern = new Regex(
            @"^data:(?<mime>[a-zA-Z0-9\+\-\.]+/[a-zA-Z0-9\+\-\.]+)?(;charset=[a-zA-Z0-9\-_]+)?(;name=(?:""[^""]*""|[^;]+))?(;\s*base64)?,(?<data>.+)$",
            RegexOptions.Compiled
        );

        public static bool IsDataUrl(string str)
        {
            return DataUrlPattern.IsMatch(str);
        }

        public static bool TryParse(string url, out DataUrl dataUrl)
        {
            dataUrl = null;

            if (!IsDataUrl(url))
                return false;

            try
            {
                dataUrl = Parse(url);
                return true;
            }
            catch
            {
                dataUrl = null;
                return false;
            }
        }

        public static DataUrl Parse(string dataUrl)
        {
            if (!dataUrl.StartsWith("data:"))
                throw new FormatException("Invalid Data URL format.");

            var parts = dataUrl.Substring(5).Split(new[] { ',' }, 2);
            if (parts.Length != 2)
                throw new FormatException("Invalid Data URL format.");

            var metaData = parts[0];
            var data = parts[1];

            string mimeType = "text/plain";
            string charset = null;
            bool isBase64 = false;
            string name = null;

            var attributes = metaData.Split(';');
            if (attributes.Any())
            {
                mimeType = attributes[0];
                foreach (var attr in attributes.Skip(1))
                {
                    if (attr.Equals("base64", StringComparison.OrdinalIgnoreCase))
                    {
                        isBase64 = true;
                    }
                    else if (attr.StartsWith("charset=", StringComparison.OrdinalIgnoreCase))
                    {
                        charset = attr.Substring(8);
                    }
                    else if (attr.StartsWith("name=", StringComparison.OrdinalIgnoreCase))
                    {
                        name = attr.Substring(5);
                    }
                }
            }

            byte[] bytes;

            if (isBase64)
            {
                bytes = Convert.FromBase64String(data);
            }
            else
            {
                data = Uri.UnescapeDataString(data);
                bytes = charset.ToLower() == "utf-8"
                    ? Encoding.UTF8.GetBytes(data)
                    : Encoding.ASCII.GetBytes(data);
            }

            return new DataUrl
            {
                MimeType = mimeType,
                Charset = charset,
                IsBase64 = isBase64,
                Data = bytes,
                Name = name
            };
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("data:");

            sb.Append(MimeType);
            if (!string.IsNullOrEmpty(Charset))
            {
                sb.Append(";charset=" + Charset);
            }
            if (!string.IsNullOrEmpty(Name))
            {
                sb.Append(";name=" + Name);
            }
            if (IsBase64)
            {
                sb.Append(";base64");
                sb.Append("," + Convert.ToBase64String(Data));
            }
            else
            {
                sb.Append("," + Uri.EscapeDataString(Encoding.UTF8.GetString(Data)));
            }

            return sb.ToString();
        }

        public static implicit operator string(DataUrl url)
        {
            return url.ToString();
        }

        public static implicit operator DataUrl(string value)
        {
            return Parse(value);
        }

        public DataUrl()
        {
            MimeType = "text/plain";
            IsBase64 = false;
            Data = new byte[0];
        }

        public DataUrl(string mimeType, string data, string name = null) : this(mimeType, Convert.FromBase64String(data), true, name){ }

        public DataUrl(string mimeType, byte[] data, bool base64, string name = null)
        {
            MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
            Data = data;
            IsBase64 = base64;
            Name = name;
        }
    }
}
