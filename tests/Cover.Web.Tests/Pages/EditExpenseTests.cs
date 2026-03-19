using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Cover.Shared.DTOs;
using Cover.Web.Pages;
using Cover.Web.Services;
using Xunit;

namespace Cover.Web.Tests.Pages;

public class EditExpenseTests : TestContext
{
    private readonly IApiClient _api;

    private static readonly List<UserDto> FakeUsers = [new(1, "Alice"), new(2, "Bob")];

    private static readonly ExpenseDto ExistingExpense = new(
        1, "Lunch", null, 3000, SplitType.Equal, 1, "Alice",
        new DateOnly(2026, 3, 1), DateTime.UtcNow);

    private static readonly ExpenseDto UpdatedExpense = new(
        1, "Lunch updated", null, 3000, SplitType.Equal, 1, "Alice",
        new DateOnly(2026, 3, 1), DateTime.UtcNow);

    public EditExpenseTests()
    {
        _api = Substitute.For<IApiClient>();
        _api.GetUsersAsync().Returns(FakeUsers);
        _api.GetExpenseAsync(1).Returns(ExistingExpense);
        _api.GetMerchantsAsync(Arg.Any<string>()).Returns([]);
        Services.AddSingleton(_api);
        Services.AddScoped<CurrencyState>();
    }

    [Fact]
    public void ErrorBox_NotVisible_OnInitialRender()
    {
        var cut = RenderComponent<EditExpense>(p => p.Add(e => e.Id, 1));

        Assert.Empty(cut.FindAll(".alert-danger"));
    }

    [Fact]
    public void ErrorBox_ShowsMessage_WhenMerchantEmpty()
    {
        var cut = RenderComponent<EditExpense>(p => p.Add(e => e.Id, 1));

        // Clear the merchant that was populated from the existing expense
        cut.Find("input[autocomplete='off']").Input("");
        cut.Find("button[type='submit']").Click();

        Assert.Equal("Merchant is required.", cut.Find(".alert-danger").TextContent.Trim());
    }

    [Fact]
    public void ErrorBox_ShowsMessage_WhenAmountZero()
    {
        var cut = RenderComponent<EditExpense>(p => p.Add(e => e.Id, 1));

        cut.Find("input[autocomplete='off']").Input("Lunch");
        cut.Find("input[type='number']").Change("0");
        cut.Find("button[type='submit']").Click();

        Assert.Equal("Amount must be positive.", cut.Find(".alert-danger").TextContent.Trim());
    }

    [Fact]
    public void Submit_NavigatesToHome_WhenSuccessful()
    {
        _api.UpdateExpenseAsync(Arg.Any<int>(), Arg.Any<UpdateExpenseRequest>()).Returns(UpdatedExpense);

        var cut = RenderComponent<EditExpense>(p => p.Add(e => e.Id, 1));
        cut.Find("input[autocomplete='off']").Input("Lunch updated");
        cut.Find("button[type='submit']").Click();

        var nav = Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/", nav.Uri);
    }

    [Fact]
    public void ErrorBox_NotVisible_AfterSuccessfulSubmit()
    {
        _api.UpdateExpenseAsync(Arg.Any<int>(), Arg.Any<UpdateExpenseRequest>()).Returns(UpdatedExpense);

        var cut = RenderComponent<EditExpense>(p => p.Add(e => e.Id, 1));
        cut.Find("input[autocomplete='off']").Input("Lunch updated");
        cut.Find("button[type='submit']").Click();

        Assert.Empty(cut.FindAll(".alert-danger"));
    }
}
