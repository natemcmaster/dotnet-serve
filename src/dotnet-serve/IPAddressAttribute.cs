using System.ComponentModel.DataAnnotations;
using System.Net;

namespace McMaster.DotNet.Server
{
    sealed class IPAddressAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value is string str && !IPAddress.TryParse(str, out _))
            {
                return new ValidationResult($"'{value}' is not a valid IP address");
            }

            return ValidationResult.Success;
        }
    }
}
