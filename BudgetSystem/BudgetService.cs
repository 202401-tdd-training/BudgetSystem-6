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

        var data = _budgetRepo.GetAll()
                              .Select(s =>
                              {
                                  var year = int.Parse(s.YearMonth.Substring(0, 4));
                                  var month = int.Parse(s.YearMonth.Substring(4, 2));
                                  var days = DateTime.DaysInMonth(year, month);
                                  return new
                                         {
                                             Year = year,
                                             Month = month,
                                             DailyAmount = s.Amount / days
                                         };
                              });

        var dayRange = GetMonthsWithDaysInRange(start, end);

        return dayRange.Join(data,
                             day => new { day.Year, day.Month },
                             dd => new { dd.Year, dd.Month },
                             (day, dd) => dd.DailyAmount * day.Days
                       )
                       .Sum();
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