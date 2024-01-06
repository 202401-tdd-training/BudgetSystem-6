namespace BudgetSystem;

public class Period
{
    public Period(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    private DateTime End { get; set; }
    private DateTime Start { get; set; }

    public int OverlappingDays(Budget budget)
    {
        var firstDay = budget.FirstDay();
        var lastDay = budget.LastDay();
        var overlappingEnd = End < lastDay
            ? End
            : lastDay;

        var overlappingStart = Start > firstDay
            ? Start
            : firstDay;

        return (overlappingEnd - overlappingStart).Days + 1;
    }
}

public class BudgetService
{
    private readonly IBudgetRepo _budgetRepo;

    public BudgetService(IBudgetRepo budgetRepo)
    {
        _budgetRepo = budgetRepo;
    }

    public decimal Query(DateTime start, DateTime end)
    {
        if (start > end)
        {
            return 0.0m;
        }

        var budgets = _budgetRepo.GetAll();

        var totalAmount = 0m;
        if (start.Year == end.Year && start.Month == end.Month)
        {
            var currentBudget = budgets.SingleOrDefault(b => b.YearMonth == start.ToString("yyyyMM"));
            if (currentBudget != null)
            {
                var overlappingDays = end.Day - start.Day + 1;
                return overlappingDays * currentBudget.DailyAmount();
            }
        }
        else
        {
            var nextMonthOfEnd = new DateTime(end.Year, end.Month, 1).AddMonths(1);
            for (var currentMonth = start; currentMonth < nextMonthOfEnd; currentMonth.AddMonths(1))
            {
                var currentBudget = budgets.SingleOrDefault(b => b.YearMonth == currentMonth.ToString("yyyyMM"));
                if (currentBudget != null)
                {
                    var overlappingDays = new Period(start, end).OverlappingDays(currentBudget);

                    totalAmount += overlappingDays * currentBudget.DailyAmount();
                }
            }
        }

        return totalAmount;
    }
}

public interface IBudgetRepo
{
    List<Budget> GetAll();
}

public class Budget
{
    public int Amount { get; set; }
    public string YearMonth { get; set; }

    public int DailyAmount()
    {
        return Amount / Days();
    }

    public int Days()
    {
        var year = int.Parse(YearMonth.Substring(0, 4));
        var month = int.Parse(YearMonth.Substring(4, 2));
        return DateTime.DaysInMonth(year, month);
    }

    public DateTime FirstDay()
    {
        return DateTime.ParseExact(YearMonth, "yyyyMM", null);
    }

    public DateTime LastDay()
    {
        return DateTime.ParseExact(YearMonth + Days(), "yyyyMMdd", null);
    }
}