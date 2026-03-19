using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Cover.Shared.DTOs;
using Cover.Web.Pages;
using Cover.Web.Services;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace Cover.Web.Tests.Pages;

public class AddExpenseTests : TestContext
{
    private readonly IApiClient _api;

    private static readonly List<UserDto> FakeUsers = [new(1, "Alice"), new(2, "Bob")];

    private static readonly ExpenseDto FakeExpense = new(
        1, "Amazon", "Groceries", 5000, SplitType.Equal, 1, "Alice",
        DateOnly.FromDateTime(DateTime.Today), DateTime.UtcNow);

    public AddExpenseTests()
    {
        _api = Substitute.For<IApiClient>();
        _api.GetUsersAsync().Returns(FakeUsers);
        _api.GetMerchantsAsync(Arg.Any<string>()).Returns([]);
        Services.AddSingleton(_api);
        Services.AddScoped<CurrencyState>();
        JSInterop.Setup<string?>("localStorage.getItem", "lastPaidById").SetResult(null);
        JSInterop.SetupVoid("localStorage.setItem", _ => true);
    }

    [Fact]
    public void ErrorBox_NotVisible_OnInitialRender()
    {
        var cut = RenderComponent<AddExpense>();

        Assert.Empty(cut.FindAll(".alert-danger"));
    }

    [Fact]
    public void ErrorBox_ShowsMessage_WhenMerchantEmpty()
    {
        var cut = RenderComponent<AddExpense>();

        cut.Find("button[type='submit']").Click();

        Assert.Equal("Merchant is required.", cut.Find(".alert-danger").TextContent.Trim());
    }

    [Fact]
    public void ErrorBox_ShowsMessage_WhenAmountZero()
    {
        var cut = RenderComponent<AddExpense>();

        // Fill merchant so we pass that validation check
        cut.Find("input[autocomplete='off']").Input("Amazon");
        cut.Find("button[type='submit']").Click();

        Assert.Equal("Amount must be positive.", cut.Find(".alert-danger").TextContent.Trim());
    }

    [Fact]
    public void Submit_NavigatesToHome_WhenSuccessful()
    {
        _api.CreateExpenseAsync(Arg.Any<CreateExpenseRequest>()).Returns(FakeExpense);

        var cut = RenderComponent<AddExpense>();
        cut.Find("input[autocomplete='off']").Input("Amazon");
        cut.Find("input[type='number']").Change("5000");
        cut.Find("button[type='submit']").Click();

        var nav = Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/", nav.Uri);
    }

    [Fact]
    public void ErrorBox_NotVisible_AfterSuccessfulSubmit()
    {
        _api.CreateExpenseAsync(Arg.Any<CreateExpenseRequest>()).Returns(FakeExpense);

        var cut = RenderComponent<AddExpense>();
        cut.Find("input[autocomplete='off']").Input("Amazon");
        cut.Find("input[type='number']").Change("5000");
        cut.Find("button[type='submit']").Click();

        Assert.Empty(cut.FindAll(".alert-danger"));
    }
}
