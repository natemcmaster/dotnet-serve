using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace McMaster.DotNet.Server
{
    sealed class DirectoryMustExistAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value is string str
                && (str.IndexOfAny(Path.GetInvalidPathChars()) >= 0
                    || !Directory.Exists(str)))
            {
                return new ValidationResult($"The directory {value} does not exist.");
            }

            return ValidationResult.Success;
        }
    }
}
