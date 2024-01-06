namespace BudgetSystem;

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
        foreach (var budget in budgets)
        {
            if (start.Year == end.Year && start.Month == end.Month)
            {
                if (budget.YearMonth == start.ToString("yyyyMM"))
                {
                    var overlappingDays = end.Day - start.Day + 1;
                    return overlappingDays * budget.DailyAmount();
                }
            }
            else
            {
                var nextMonthOfEnd = new DateTime(end.Year, end.Month, 1).AddMonths(1);
                for (var currentMonth = start; currentMonth < nextMonthOfEnd; currentMonth.AddMonths(1))
                {
                    int overlappingDays;
                    if (budget.YearMonth == start.ToString("yyyyMM"))
                    {
                        // var startDays = DateTime.DaysInMonth(start.Year, start.Month);
                        // overlappingDays = startDays - start.Day + 1;
                        overlappingDays = (budget.LastDay() - start).Days + 1;
                    }
                    else if (budget.YearMonth == end.ToString("yyyyMM"))
                    {
                        overlappingDays = (end - budget.FirstDay()).Days + 1;
                    }
                    else if (budget.YearMonth == currentMonth.ToString("yyyyMM"))
                    {
                        overlappingDays = (budget.LastDay() - budget.FirstDay()).Days + 1;
                    }
                    else
                    {
                        overlappingDays = 0;
                    }

                    totalAmount += overlappingDays * budget.DailyAmount();
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