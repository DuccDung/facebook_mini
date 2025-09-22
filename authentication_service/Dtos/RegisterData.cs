using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace authentication_service.Dtos
{


    public enum SexType
    {
        Custom = -1,
        Female = 1,
        Male = 2    
    }

    public sealed class BirthdayDto
    {
        [Range(1, 31)]
        [JsonPropertyName("day")]
        public int? Day { get; set; }

        [Range(1, 12)]
        [JsonPropertyName("month")]
        public int? Month { get; set; }

        [Range(1900, 2100)]
        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [Range(0, 200)]
        [JsonPropertyName("age")]
        public int? Age { get; set; }
    }

    public sealed class RegisterRequest
    {
        [Required, StringLength(100)]
        [JsonPropertyName("firstname")]
        public string Firstname { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [JsonPropertyName("lastname")]
        public string Lastname { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(255)]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(255)]
        [JsonPropertyName("email_confirmation")]
        public string EmailConfirmation { get; set; } = string.Empty;

        [Required, StringLength(255, MinimumLength = 6)]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("birthday")]
        public BirthdayDto? Birthday { get; set; }

        [JsonPropertyName("sex")]
        public SexType? Sex { get; set; } 

        [StringLength(100)]
        [JsonPropertyName("preferred_pronoun")]
        public string PreferredPronoun { get; set; } = string.Empty;

        [StringLength(100)]
        [JsonPropertyName("custom_gender")]
        public string CustomGender { get; set; } = string.Empty;

        [StringLength(255)]
        [JsonPropertyName("referrer")]
        public string Referrer { get; set; } = string.Empty;
    }

}
