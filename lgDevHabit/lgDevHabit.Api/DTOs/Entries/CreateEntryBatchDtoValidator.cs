using FluentValidation;

namespace lgDevHabit.Api.DTOs.Entries;

public sealed class CreateEntryBatchDtoValidator : AbstractValidator<CreateEntryBatchDto>
{
    //注入CreateEntryDtoValidator用来检查每个条目的有效性
    public CreateEntryBatchDtoValidator(CreateEntryDtoValidator entryValidator)
    {
        RuleFor(x => x.Entries)
            .NotEmpty()
            .WithMessage("At least one entry is required.")
            .Must(entries => entries.Count <= 20)
            .WithMessage("Maximum of 20 entries per batch.");

        RuleForEach(x => x.Entries)
            .SetValidator(entryValidator);
    }
}
