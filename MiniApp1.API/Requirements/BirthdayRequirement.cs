using Microsoft.AspNetCore.Authorization;

namespace MiniApp1.API.Requirements
{
    public class BirthdayRequirement : IAuthorizationRequirement
    {
        public int Age { get; set; }

        public BirthdayRequirement(int birthday)
        {
            Age = birthday;
        }
    }

    public class BirthdayRequirementHandler : AuthorizationHandler<BirthdayRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, BirthdayRequirement requirement)
        {
            var birthDate = context.User.FindFirst("birth-date");

            if (birthDate == null)
            {
                context.Fail();

                return Task.CompletedTask;
            }

            var today = DateTime.Now;
            var age = today.Year - Convert.ToDateTime(birthDate.Value).Year;


            if (requirement.Age <= age )
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
