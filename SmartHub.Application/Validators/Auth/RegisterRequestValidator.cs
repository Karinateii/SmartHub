using FluentValidation;
using SmartHub.Application.DTOs.Auth;

namespace SmartHub.Application.Validators.Auth
{
    //Validator for RegisterRequest DTO
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.FirstName)
              .NotEmpty().WithMessage("First name is required.");

            RuleFor(x => x.LastName)
              .NotEmpty().WithMessage("Last name is required.");

            RuleFor(x => x.Email)
              .NotEmpty().WithMessage("Email is required.")
              .EmailAddress().WithMessage("A valid email is required.");

            RuleFor(x => x.Password)
              .NotEmpty().WithMessage("Password is required.")
              .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

            RuleFor(x => x.ConfirmPassword)
              .Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }
}