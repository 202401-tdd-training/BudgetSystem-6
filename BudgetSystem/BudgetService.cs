namespace BudgetSystem;

public class Period
{
    public Period(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    public DateTime End { get; private set; }
    public DateTime Start { get; private set; }
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
                    var overlappingDays = OverlappingDays(new Period(start, end), currentBudget);

                    totalAmount += overlappingDays * currentBudget.DailyAmount();
                }
            }
        }

        return totalAmount;
    }

    private static int OverlappingDays(Period period, Budget budget)
    {
        DateTime overlappingEnd;
        DateTime overlappingStart;
        if (budget.YearMonth == period.Start.ToString("yyyyMM"))
        {
            overlappingEnd = budget.LastDay();
            overlappingStart = period.Start;
        }
        else if (budget.YearMonth == period.End.ToString("yyyyMM"))
        {
            overlappingEnd = period.End;
            overlappingStart = budget.FirstDay();
        }
        else
        {
            overlappingEnd = budget.LastDay();
            overlappingStart = budget.FirstDay();
        }

        var overlappingDays = (overlappingEnd - overlappingStart).Days + 1;
        return overlappingDays;
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