using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace Host.API.DTOs
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LoginResult
    {
        None             = 0,
        Success          = 1,
        UnExist          = 2,
        ErrorPWD         = 3,
        ErrorCode        = 4,
        ExpiredCode      = 5,
        CreateFail       = 6,
        WeChatLoginFaild = 7
    }

    public class WeChatLoginRequestDTO
    {
        public string code { get; set; }
    }

    public class LoginResponseDTO
    {
        public LoginResult result { get; set; }

        public string data { get; set; }
    }
}
