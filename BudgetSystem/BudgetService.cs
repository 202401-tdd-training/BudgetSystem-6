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
            return 0.0m;

        var budgets = _budgetRepo.GetAll();
        // var data = budgets
        //     .Select(s =>
        //     {
        //         var year = int.Parse(s.YearMonth.Substring(0, 4));
        //         var month = int.Parse(s.YearMonth.Substring(4, 2));
        //         var days = DateTime.DaysInMonth(year, month);
        //         return new
        //                {
        //                    Year = year,
        //                    Month = month,
        //                    DailyAmount = s.Amount / days
        //                };
        //     });
        var dailyAmountObjects = new List<DailyAmountObject>();

        var dateRange = new List<DateResult>();
        var totalAmount = 0m;
        foreach (var budget in budgets)
        {
            var year = int.Parse(budget.YearMonth.Substring(0, 4));
            var month = int.Parse(budget.YearMonth.Substring(4, 2));
            var days = DateTime.DaysInMonth(year, month);
            var dailyAmountObject = new DailyAmountObject
                                    {
                                        Year = year,
                                        Month = month,
                                        DailyAmount = budget.Amount / days
                                    };
            dailyAmountObjects.Add(dailyAmountObject);

            if (start.Year == end.Year && start.Month == end.Month)
            {
                if (budget.YearMonth == start.ToString("yyyyMM"))
                {
                    var overlappingDays = end.Day - start.Day + 1;
                    return overlappingDays * (budget.Amount / days);
                }
            }
            else
            {
                var nextMonthOfEnd = new DateTime(end.Year, end.Month, 1).AddMonths(1);
                for (var currentMonth = start; currentMonth < nextMonthOfEnd; currentMonth.AddMonths(1))
                {
                    if (budget.YearMonth == start.ToString("yyyyMM"))
                    {
                        var startDays = DateTime.DaysInMonth(start.Year, start.Month);
                        var overlappingDays = startDays - start.Day + 1;
                        totalAmount += overlappingDays * (budget.Amount / days);
                        // dateRange.Add(new DateResult()
                        //               {
                        //                   Year = start.Year,
                        //                   Month = start.Month,
                        //                   Days = overlappingDays
                        //               });
                    }
                    else if (budget.YearMonth == end.ToString("yyyyMM"))
                    {
                        var overlappingDays = end.Day;
                        totalAmount += overlappingDays * (budget.Amount / days);
                        // dateRange.Add(new DateResult()
                        //               {
                        //                   Year = end.Year,
                        //                   Month = end.Month,
                        //                   Days = overlappingDays
                        //               });
                    }
                    else if (budget.YearMonth == currentMonth.ToString("yyyyMM"))
                    {
                        var overlappingDays = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
                        totalAmount += overlappingDays * (budget.Amount / days);

                        // dateRange.Add(new DateResult()
                        //               {
                        //                   Year = currentMonth.Year,
                        //                   Month = currentMonth.Month,
                        //                   Days = overlappingDays
                        //               });
                    }
                }
            }
        }

        return totalAmount;

        // return dateRange.Join(dailyAmountObjects,
        //                       day => new { day.Year, day.Month },
        //                       dd => new { dd.Year, dd.Month },
        //                       (day, dd) => dd.DailyAmount * day.Days
        //                 )
        //                 .Sum();
    }

    private List<DateResult> GetMonthsWithDaysInRange(DateTime startDate, DateTime endDate)
    {
        List<DateResult> result = new List<DateResult>();

        if (startDate.Year == endDate.Year && startDate.Month == endDate.Month)
            return new List<DateResult>()
                   {
                       new DateResult()
                       {
                           Year = startDate.Year,
                           Month = startDate.Month,
                           Days = endDate.Day - startDate.Day + 1
                       }
                   };

        var startDays = DateTime.DaysInMonth(startDate.Year, startDate.Month);

        result.Add(new DateResult()
                   {
                       Year = startDate.Year,
                       Month = startDate.Month,
                       Days = startDays - startDate.Day + 1
                   });

        result.Add(new DateResult()
                   {
                       Year = endDate.Year,
                       Month = endDate.Month,
                       Days = endDate.Day
                   });

        var secondMonth = startDate.AddMonths(1);

        for (var date = secondMonth; date < endDate; date.AddMonths(1))
        {
            if (date.Year == endDate.Year && date.Month == endDate.Month)
                break;

            result.Add(new DateResult()
                       {
                           Year = date.Year,
                           Month = date.Month,
                           Days = DateTime.DaysInMonth(date.Year, date.Month)
                       });
        }

        return result;
    }
}

public class DailyAmountObject
{
    public int DailyAmount { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}

public interface IBudgetRepo
{
    List<Budget> GetAll();
}

public class Budget
{
    public int Amount { get; set; }
    public string YearMonth { get; set; }
}

public class DateResult
{
    public int Days { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}