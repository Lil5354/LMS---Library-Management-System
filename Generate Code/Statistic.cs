using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using ClosedXML.Excel;
using LMS.Proccess;

namespace LMS.Generate_Code
{
    public partial class Statistic : UserControl
    {
        public Statistic()
        {
            InitializeComponent();
            InitializeComboBox();
            InitializeDateTimePickers();
        }
        private void InitializeComboBox()
        {
            cbbFilterStatistic.Items.AddRange(new object[] {
                "OVERALL STATISTICS",
                "BOOK CATEGORIES",
                "READER ANALYSIS",
                "BORROWING TRENDS"

            });
        }
        private void InitializeDateTimePickers()
        {
            dtpStart.Value = DateTime.Now.AddMonths(-1);
            dtpEnd.Value = DateTime.Now;
        }
        private void LoadOverallStatistics()
        {
            DateTime start = dtpStart.Value;
            string startDate = start.ToString("yyyy-MM-dd");
            DateTime end = dtpEnd.Value;
            string endDate = end.ToString("yyyy-MM-dd");
            object[] parameters = new object[] { startDate, endDate };

            // Query for most borrowed books
            string queryTopBooks = @"
                SELECT TOP 10
                    B.TITLE AS BookTitle,
                    COUNT(BT.TICKETID) AS BorrowCount
                FROM
                    BORROWINGTICKETS BT
                JOIN
                    BOOKS B ON BT.BOOKID = B.BOOKID
                WHERE
                    BT.BORROWDATE BETWEEN @StartDate AND @EndDate 
                    AND BT.APPROVAL_STATUS = 'Approve'
                GROUP BY
                    B.TITLE
                ORDER BY
                    BorrowCount DESC";

            // Query for borrowing status
            string queryBorrowingStatus = @"
                SELECT 
                    BT.STATUS,
                    COUNT(BT.TICKETID) AS StatusCount
                FROM 
                    BORROWINGTICKETS BT
                WHERE  
                    BT.BORROWDATE BETWEEN @StartDate AND @EndDate 
                    AND BT.APPROVAL_STATUS = 'Approve'
                GROUP BY 
                    BT.STATUS
                ORDER BY 
                    StatusCount DESC";

            var topBooksData = GetDatabase.Instance.GetChartData(queryTopBooks, parameters);
            var borrowingStatusData = GetDatabase.Instance.GetChartData(queryBorrowingStatus, parameters);

            // Top Books Chart Setup
            chartTopBooks.Series.Clear();
            var booksSeries = chartTopBooks.Series.Add("Most Borrowed Books");
            booksSeries.ChartType = SeriesChartType.Pie;
            booksSeries["PieLabelStyle"] = "Outside";
            booksSeries["PieStartAngle"] = "200";
            booksSeries.Label = "#PERCENT{P2}%";
            booksSeries.Font = new Font("Arial", 10, FontStyle.Bold);

            double totalLoans = topBooksData.Sum(x => x.Value);
            foreach (var dataPoint in topBooksData)
            {
                DataPoint point = new DataPoint();
                point.AxisLabel = dataPoint.Key;
                point.YValues = new double[] { dataPoint.Value };
                point.LegendText = $"{dataPoint.Key}: {dataPoint.Value} loans";
                booksSeries.Points.Add(point);
            }

            // Borrowing Status Chart Setup
            chartBorrowingStatus.Series.Clear();
            var statusSeries = chartBorrowingStatus.Series.Add("Borrowing Status");
            statusSeries.ChartType = SeriesChartType.Pie;
            statusSeries["PieLabelStyle"] = "Outside";
            statusSeries["PieStartAngle"] = "270";
            statusSeries.Label = "#PERCENT{P2}%";
            statusSeries.Font = new Font("Arial", 10, FontStyle.Bold);

            double totalStatus = borrowingStatusData.Sum(x => x.Value);
            foreach (var dataPoint in borrowingStatusData)
            {
                DataPoint point = new DataPoint();
                point.AxisLabel = dataPoint.Key;
                point.YValues = new double[] { dataPoint.Value };
                point.LegendText = $"{dataPoint.Key}: {dataPoint.Value} loans";
                statusSeries.Points.Add(point);
            }

            // Legend configuration
            chartTopBooks.Legends[0].Docking = Docking.Right;
            chartTopBooks.Legends[0].Alignment = StringAlignment.Center;
            chartTopBooks.Legends[0].Font = new Font("Arial", 10);

            chartBorrowingStatus.Legends[0].Docking = Docking.Bottom;
            chartBorrowingStatus.Legends[0].Alignment = StringAlignment.Center;
            chartBorrowingStatus.Legends[0].Font = new Font("Arial", 10);

            if (rdoMonth.Checked)
            {
                string queryMonthlyStats = @"
                SELECT 
                    FORMAT(BT.BORROWDATE, 'yyyy-MM') AS Month,
                    COUNT(BT.TICKETID) AS TotalLoans,
                    SUM(CASE WHEN BT.STATUS = 'Restored' THEN 1 ELSE 0 END) AS ReturnedLoans,
                    COUNT(DISTINCT BT.READERID) AS UniqueReaders
                FROM 
                    BORROWINGTICKETS BT
                WHERE 
                    BT.BORROWDATE BETWEEN @StartDate AND @EndDate 
                    AND BT.APPROVAL_STATUS = 'Approve'
                GROUP BY 
                    FORMAT(BT.BORROWDATE, 'yyyy-MM')
                ORDER BY 
                    Month";

                // Get monthly data for each metric
                var monthlyLoansData = GetDatabase.Instance.GetChartData(queryMonthlyStats, parameters);
                var monthlyReturnedData = GetDatabase.Instance.GetChartData(
                    queryMonthlyStats.Replace("COUNT(BT.TICKETID) AS TotalLoans", "SUM(CASE WHEN BT.STATUS = 'Restored' THEN 1 ELSE 0 END) AS ReturnedLoans"),
                    parameters
                );
                var monthlyReadersData = GetDatabase.Instance.GetChartData(
                    queryMonthlyStats.Replace("COUNT(BT.TICKETID) AS TotalLoans", "COUNT(DISTINCT BT.READERID) AS UniqueReaders"),
                    parameters
                );

                // Setup monthly chart series
                chartMainStats.Series.Clear();
                var monthlyLoans = chartMainStats.Series.Add("Total Loans");
                var monthlyReturned = chartMainStats.Series.Add("Returned Loans");
                var monthlyReaders = chartMainStats.Series.Add("Unique Readers");

                // Configure series types
                foreach (var series in chartMainStats.Series)
                {
                    series.ChartType = SeriesChartType.Line;
                    series.MarkerStyle = MarkerStyle.Circle;
                }

                // Add monthly data points
                foreach (var dataPoint in monthlyLoansData)
                    monthlyLoans.Points.AddXY(dataPoint.Key, dataPoint.Value);

                foreach (var dataPoint in monthlyReturnedData)
                    monthlyReturned.Points.AddXY(dataPoint.Key, dataPoint.Value);

                foreach (var dataPoint in monthlyReadersData)
                    monthlyReaders.Points.AddXY(dataPoint.Key, dataPoint.Value);
            }
            else if (rdoYear.Checked)
            {
                string queryYearlyStats = @"
                SELECT 
                    YEAR(BT.BORROWDATE) AS Year,
                    COUNT(BT.TICKETID) AS TotalLoans,
                    SUM(CASE WHEN BT.STATUS = 'Restored' THEN 1 ELSE 0 END) AS ReturnedLoans,
                    COUNT(DISTINCT BT.READERID) AS UniqueReaders
                FROM 
                    BORROWINGTICKETS BT
                WHERE 
                    BT.BORROWDATE BETWEEN @StartDate AND @EndDate 
                    AND BT.APPROVAL_STATUS = 'Approve'
                GROUP BY 
                    YEAR(BT.BORROWDATE)
                ORDER BY 
                    Year";

                var yearlyLoansData = GetDatabase.Instance.GetChartData(queryYearlyStats, parameters);
                var yearlyReturnedData = GetDatabase.Instance.GetChartData(
                    queryYearlyStats.Replace("COUNT(BT.TICKETID) AS TotalLoans", "SUM(CASE WHEN BT.STATUS = 'Restored' THEN 1 ELSE 0 END) AS ReturnedLoans"),
                    parameters
                );
                var yearlyReadersData = GetDatabase.Instance.GetChartData(
                    queryYearlyStats.Replace("COUNT(BT.TICKETID) AS TotalLoans", "COUNT(DISTINCT BT.READERID) AS UniqueReaders"),
                    parameters
                );

                // Setup yearly chart series
                chartMainStats.Series.Clear();
                var yearlyLoans = chartMainStats.Series.Add("Total Loans");
                var yearlyReturned = chartMainStats.Series.Add("Returned Loans");
                var yearlyReaders = chartMainStats.Series.Add("Unique Readers");

                // Configure series types
                foreach (var series in chartMainStats.Series)
                {
                    series.ChartType = SeriesChartType.Line;
                    series.MarkerStyle = MarkerStyle.Circle;
                }

                // Add yearly data points
                foreach (var dataPoint in yearlyLoansData)
                    yearlyLoans.Points.AddXY(dataPoint.Key, dataPoint.Value);

                foreach (var dataPoint in yearlyReturnedData)
                    yearlyReturned.Points.AddXY(dataPoint.Key, dataPoint.Value);

                foreach (var dataPoint in yearlyReadersData)
                    yearlyReaders.Points.AddXY(dataPoint.Key, dataPoint.Value);
            }
        }
        private void LoadCategoryStatistics()
        {
            DateTime start = dtpStart.Value;
            string startDate = start.ToString("yyyy-MM-dd");
            DateTime end = dtpEnd.Value;
            string endDate = end.ToString("yyyy-MM-dd");
            object[] parameters = new object[] { startDate, endDate };

            // Query for category popularity
            string queryCategoryPopularity = @"
                SELECT 
                    C.NAME AS CategoryName,
                    COUNT(BT.TICKETID) AS LoanCount
                FROM 
                    BORROWINGTICKETS BT
                JOIN 
                    BOOKS B ON BT.BOOKID = B.BOOKID
                JOIN 
                    CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
                WHERE 
                    BT.BORROWDATE BETWEEN @StartDate AND @EndDate 
                    AND BT.APPROVAL_STATUS = 'Approve'
                GROUP BY 
                    C.NAME
                ORDER BY 
                    LoanCount DESC";

            // Query for category vs overdue rate
            string queryCategoryOverdue = @"
                SELECT 
                    C.NAME AS CategoryName,
                    COUNT(*) AS TotalReturns,
                    SUM(CASE WHEN BT.RETURNDATE > BT.DUEDATE THEN 1 ELSE 0 END) AS OverdueCount,
                    SUM(CASE WHEN BT.STATUS = 'Restored' THEN 1 ELSE 0 END) AS TotalRestored,
                    CASE 
                        WHEN SUM(CASE WHEN BT.STATUS = 'Restored' THEN 1 ELSE 0 END) > 0
                        THEN SUM(CASE WHEN BT.RETURNDATE > BT.DUEDATE THEN 1 ELSE 0 END) * 100.0 / 
                             SUM(CASE WHEN BT.STATUS = 'Restored' THEN 1 ELSE 0 END)
                        ELSE 0 
                    END AS OverdueRate
                FROM 
                    BORROWINGTICKETS BT
                JOIN 
                    BOOKS B ON BT.BOOKID = B.BOOKID
                JOIN 
                    CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
                WHERE 
                    BT.BORROWDATE BETWEEN @StartDate AND @EndDate 
                    AND BT.APPROVAL_STATUS = 'Approve'
                    AND BT.STATUS = 'Restored'
                GROUP BY 
                    C.NAME
                ORDER BY 
                    OverdueRate DESC";

            var categoryPopularityData = GetDatabase.Instance.GetChartData(queryCategoryPopularity, parameters);
            var categoryOverdueData = GetDatabase.Instance.GetChartData(queryCategoryOverdue, parameters);

            // Category Popularity Chart Setup
            chartTopBooks.Series.Clear();
            var categorySeries = chartTopBooks.Series.Add("Category Popularity");
            categorySeries.ChartType = SeriesChartType.Pie;
            categorySeries["PieLabelStyle"] = "Outside";
            categorySeries["PieStartAngle"] = "200";
            categorySeries.Label = "#PERCENT{P2}%";
            categorySeries.Font = new Font("Arial", 10, FontStyle.Bold);

            double totalCategoryLoans = categoryPopularityData.Sum(x => x.Value);
            foreach (var dataPoint in categoryPopularityData)
            {
                DataPoint point = new DataPoint();
                point.AxisLabel = dataPoint.Key;
                point.YValues = new double[] { dataPoint.Value };
                point.LegendText = $"{dataPoint.Key}: {dataPoint.Value} loans";
                categorySeries.Points.Add(point);
            }

            // Category Overdue Rate Chart Setup
            chartBorrowingStatus.Series.Clear();
            var overdueSeries = chartBorrowingStatus.Series.Add("Category Overdue Rate");
            overdueSeries.ChartType = SeriesChartType.Bar;
            overdueSeries.Font = new Font("Arial", 10, FontStyle.Bold);

            // Configure chart appearance
            chartBorrowingStatus.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            chartBorrowingStatus.ChartAreas[0].AxisX.Interval = 1;
            chartBorrowingStatus.ChartAreas[0].AxisY.Title = "Overdue Rate (%)";
            chartBorrowingStatus.ChartAreas[0].AxisY.LabelStyle.Format = "N1";

            foreach (var dataPoint in categoryOverdueData)
            {
                DataPoint point = new DataPoint();
                point.AxisLabel = dataPoint.Key;
                point.YValues = new double[] { dataPoint.Value };
                point.Label = dataPoint.Value.ToString("N1") + "%";
                overdueSeries.Points.Add(point);
            }

            // Legend configuration
            chartTopBooks.Legends[0].Docking = Docking.Right;
            chartTopBooks.Legends[0].Alignment = StringAlignment.Center;
            chartTopBooks.Legends[0].Font = new Font("Arial", 10);

            chartBorrowingStatus.Legends[0].Docking = Docking.Bottom;
            chartBorrowingStatus.Legends[0].Alignment = StringAlignment.Center;
            chartBorrowingStatus.Legends[0].Font = new Font("Arial", 10);

            // Category Age Distribution (Main Chart)
            string queryCategoryAge = @"
                SELECT 
                    C.NAME AS CategoryName,
                    AVG(YEAR(GETDATE()) - B.PUBLICATIONYEAR) AS AvgAge -- Trừ trực tiếp năm hiện tại với năm xuất bản
                FROM 
                    BOOKS B
                JOIN 
                    CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
                GROUP BY 
                    C.NAME
                ORDER BY 
                    AvgAge DESC;";

            var categoryAgeData = GetDatabase.Instance.GetChartData(queryCategoryAge, parameters);

            // Setup Category Age Chart
            chartMainStats.Series.Clear();
            var ageSeries = chartMainStats.Series.Add("Average Book Age by Category");
            ageSeries.ChartType = SeriesChartType.Column;

            // Configure chart appearance
            chartMainStats.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            chartMainStats.ChartAreas[0].AxisX.Interval = 1;
            chartMainStats.ChartAreas[0].AxisY.Title = "Average Age (Years)";
            chartMainStats.ChartAreas[0].AxisY.LabelStyle.Format = "N1";

            foreach (var dataPoint in categoryAgeData)
            {
                DataPoint point = new DataPoint();
                point.AxisLabel = dataPoint.Key;
                point.YValues = new double[] { dataPoint.Value };
                point.Label = dataPoint.Value.ToString("N1");
                ageSeries.Points.Add(point);
            }

            ageSeries.IsValueShownAsLabel = true;
            ageSeries["PointWidth"] = "0.8";
            ageSeries.LabelFormat = "N1";
            ageSeries.Font = new Font("Arial", 9);
            chartMainStats.Legends[0].Enabled = false;
        }
        private void LoadReaderStatistics()
        {
            DateTime start = dtpStart.Value;
            string startDate = start.ToString("yyyy-MM-dd");
            DateTime end = dtpEnd.Value;
            string endDate = end.ToString("yyyy-MM-dd");
            object[] parameters = new object[] { startDate, endDate };

            // Query for active readers
            string queryActiveReaders = @"
                SELECT 
                    R.FULLNAME AS ReaderName,
                    COUNT(BT.TICKETID) AS LoanCount
                FROM 
                    BORROWINGTICKETS BT
                JOIN 
                    READERS R ON BT.READERID = R.READERID
                WHERE 
                    BT.BORROWDATE BETWEEN @StartDate AND @EndDate 
                    AND BT.APPROVAL_STATUS = 'Approve'
                GROUP BY 
                    R.FULLNAME
                ORDER BY 
                    LoanCount DESC
                OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY";

            // Query for reader age groups
            string queryReaderAgeGroups = @"
                SELECT 
                    CASE 
                        WHEN DATEDIFF(YEAR, R.DATEOFBIRTH, GETDATE()) < 18 THEN 'Under 18'
                        WHEN DATEDIFF(YEAR, R.DATEOFBIRTH, GETDATE()) BETWEEN 18 AND 25 THEN '18-25'
                        WHEN DATEDIFF(YEAR, R.DATEOFBIRTH, GETDATE()) BETWEEN 26 AND 40 THEN '26-40'
                        ELSE 'Over 40'
                    END AS AgeGroup,
                    COUNT(DISTINCT BT.READERID) AS ReaderCount
                FROM 
                    BORROWINGTICKETS BT
                JOIN 
                    READERS R ON BT.READERID = R.READERID
                WHERE 
                    BT.BORROWDATE BETWEEN @StartDate AND @EndDate 
                    AND BT.APPROVAL_STATUS = 'Approve'
                GROUP BY 
                    CASE 
                        WHEN DATEDIFF(YEAR, R.DATEOFBIRTH, GETDATE()) < 18 THEN 'Under 18'
                        WHEN DATEDIFF(YEAR, R.DATEOFBIRTH, GETDATE()) BETWEEN 18 AND 25 THEN '18-25'
                        WHEN DATEDIFF(YEAR, R.DATEOFBIRTH, GETDATE()) BETWEEN 26 AND 40 THEN '26-40'
                        ELSE 'Over 40'
                    END
                ORDER BY 
                    ReaderCount DESC";

            var activeReadersData = GetDatabase.Instance.GetChartData(queryActiveReaders, parameters);
            var readerAgeData = GetDatabase.Instance.GetChartData(queryReaderAgeGroups, parameters);

            // Active Readers Chart Setup
            chartTopBooks.Series.Clear();
            var readersSeries = chartTopBooks.Series.Add("Most Active Readers");
            readersSeries.ChartType = SeriesChartType.Pie;
            readersSeries["PieLabelStyle"] = "Outside";
            readersSeries["PieStartAngle"] = "200";
            readersSeries.Label = "#PERCENT{P2}%";
            readersSeries.Font = new Font("Arial", 10, FontStyle.Bold);

            double totalLoans = activeReadersData.Sum(x => x.Value);
            foreach (var dataPoint in activeReadersData)
            {
                DataPoint point = new DataPoint();
                point.AxisLabel = dataPoint.Key;
                point.YValues = new double[] { dataPoint.Value };
                point.LegendText = $"{dataPoint.Key}: {dataPoint.Value} loans";
                readersSeries.Points.Add(point);
            }

            // Reader Age Groups Chart Setup
            chartBorrowingStatus.Series.Clear();
            var ageSeries = chartBorrowingStatus.Series.Add("Reader Age Groups");
            ageSeries.ChartType = SeriesChartType.Pie;
            ageSeries["PieLabelStyle"] = "Outside";
            ageSeries["PieStartAngle"] = "270";
            ageSeries.Label = "#PERCENT{P2}%";
            ageSeries.Font = new Font("Arial", 10, FontStyle.Bold);

            double totalReaders = readerAgeData.Sum(x => x.Value);
            foreach (var dataPoint in readerAgeData)
            {
                DataPoint point = new DataPoint();
                point.AxisLabel = dataPoint.Key;
                point.YValues = new double[] { dataPoint.Value };
                point.LegendText = $"{dataPoint.Key}: {dataPoint.Value} readers";
                ageSeries.Points.Add(point);
            }

            // Legend configuration
            chartTopBooks.Legends[0].Docking = Docking.Right;
            chartTopBooks.Legends[0].Alignment = StringAlignment.Center;
            chartTopBooks.Legends[0].Font = new Font("Arial", 10);

            chartBorrowingStatus.Legends[0].Docking = Docking.Bottom;
            chartBorrowingStatus.Legends[0].Alignment = StringAlignment.Center;
            chartBorrowingStatus.Legends[0].Font = new Font("Arial", 10);

            // Reader Location Analysis (Main Chart)
            string queryReaderLocation = @"
                SELECT 
                    ISNULL(R.ADDRESS, 'Unknown') AS Location,
                    COUNT(DISTINCT BT.READERID) AS ReaderCount
                FROM 
                    BORROWINGTICKETS BT
                JOIN 
                    READERS R ON BT.READERID = R.READERID
                WHERE  
                    BT.BORROWDATE BETWEEN @StartDate AND @EndDate 
                    AND BT.APPROVAL_STATUS = 'Approve'
                GROUP BY 
                    R.ADDRESS
                ORDER BY 
                    ReaderCount DESC";

            var locationData = GetDatabase.Instance.GetChartData(queryReaderLocation, parameters);

            // Setup Location Chart
            chartMainStats.Series.Clear();
            var locationSeries = chartMainStats.Series.Add("Reader Location Distribution");
            locationSeries.ChartType = SeriesChartType.Column;

            // Configure chart appearance
            chartMainStats.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            chartMainStats.ChartAreas[0].AxisX.Interval = 1;
            chartMainStats.ChartAreas[0].AxisY.Title = "Number of Readers";
            chartMainStats.ChartAreas[0].AxisY.LabelStyle.Format = "N0";

            foreach (var dataPoint in locationData)
            {
                DataPoint point = new DataPoint();
                point.AxisLabel = dataPoint.Key;
                point.YValues = new double[] { dataPoint.Value };
                point.Label = dataPoint.Value.ToString("N0");
                locationSeries.Points.Add(point);
            }

            locationSeries.IsValueShownAsLabel = true;
            locationSeries["PointWidth"] = "0.8";
            locationSeries.LabelFormat = "N0";
            locationSeries.Font = new Font("Arial", 9);
            chartMainStats.Legends[0].Enabled = false;
        }

        // BORROWING TRENDS ANALYSIS
        private void LoadBorrowingTrends()
        {
            DateTime start = dtpStart.Value;
            string startDate = start.ToString("yyyy-MM-dd");
            DateTime end = dtpEnd.Value;
            string endDate = end.ToString("yyyy-MM-dd");
            object[] parameters = new object[] { startDate, endDate };

            // Thống kê theo Ngày trong Tuần
            string queryDailyPatterns = @"
                SELECT 
                DATENAME(WEEKDAY, BT.BORROWDATE) AS DayOfWeek,
                COUNT(BT.TICKETID) AS LoanCount,
                COUNT(DISTINCT BT.READERID) AS UniqueReaders
            FROM 
                BORROWINGTICKETS BT
            WHERE 
                BT.BORROWDATE BETWEEN  '2021-01-01' AND '2025-03-25' 
                AND BT.APPROVAL_STATUS = 'Approve'
            GROUP BY 
                DATENAME(WEEKDAY, BT.BORROWDATE),
                DATEPART(WEEKDAY, BT.BORROWDATE)
            ORDER BY 
                DATEPART(WEEKDAY, BT.BORROWDATE)";

            // Query for loan duration
            string queryLoanDuration = @"
                SELECT 
                    CASE 
                        WHEN DATEDIFF(DAY, BT.BORROWDATE, ISNULL(BT.RETURNDATE, GETDATE())) <= 7 THEN '1 Week'
                        WHEN DATEDIFF(DAY, BT.BORROWDATE, ISNULL(BT.RETURNDATE, GETDATE())) <= 14 THEN '2 Weeks'
                        WHEN DATEDIFF(DAY, BT.BORROWDATE, ISNULL(BT.RETURNDATE, GETDATE())) <= 30 THEN '1 Month'
                        ELSE 'Over 1 Month'
                    END AS DurationCategory,
                    COUNT(BT.TICKETID) AS LoanCount
                FROM 
                    BORROWINGTICKETS BT
                WHERE 
                    BT.BORROWDATE BETWEEN @StartDate AND @EndDate 
                    AND BT.APPROVAL_STATUS = 'Approve'
                    AND BT.STATUS = 'Restored'
                GROUP BY 
                    CASE 
                        WHEN DATEDIFF(DAY, BT.BORROWDATE, ISNULL(BT.RETURNDATE, GETDATE())) <= 7 THEN '1 Week'
                        WHEN DATEDIFF(DAY, BT.BORROWDATE, ISNULL(BT.RETURNDATE, GETDATE())) <= 14 THEN '2 Weeks'
                        WHEN DATEDIFF(DAY, BT.BORROWDATE, ISNULL(BT.RETURNDATE, GETDATE())) <= 30 THEN '1 Month'
                        ELSE 'Over 1 Month'
                    END
                ORDER BY 
                    LoanCount DESC";

            var dailyPatternsData = GetDatabase.Instance.GetChartData(queryDailyPatterns, parameters);
            var loanDurationData = GetDatabase.Instance.GetChartData(queryLoanDuration, parameters);

            // Daily Patterns Chart Setup
            chartTopBooks.Series.Clear();
            var dailySeries = chartTopBooks.Series.Add("Weakly Borrowing Patterns");
            dailySeries.ChartType = SeriesChartType.Column;
            dailySeries.Font = new Font("Arial", 10, FontStyle.Bold);

            // Configure chart appearance
            chartTopBooks.ChartAreas[0].AxisX.Title = "Day of Week";
            chartTopBooks.ChartAreas[0].AxisY.Title = "Number of Loans";
            chartTopBooks.ChartAreas[0].AxisY.LabelStyle.Format = "N0";

            foreach (var dataPoint in dailyPatternsData)
            {
                DataPoint point = new DataPoint();
                point.AxisLabel = dataPoint.Key;
                point.YValues = new double[] { dataPoint.Value };
                point.Label = dataPoint.Value.ToString("N0");
                dailySeries.Points.Add(point);
            }

            // Loan Duration Chart Setup
            chartBorrowingStatus.Series.Clear();
            var durationSeries = chartBorrowingStatus.Series.Add("Loan Duration");
            durationSeries.ChartType = SeriesChartType.Pie;
            durationSeries["PieLabelStyle"] = "Outside";
            durationSeries["PieStartAngle"] = "270";
            durationSeries.Label = "#PERCENT{P2}%";
            durationSeries.Font = new Font("Arial", 10, FontStyle.Bold);

            double totalLoans = loanDurationData.Sum(x => x.Value);
            foreach (var dataPoint in loanDurationData)
            {
                DataPoint point = new DataPoint();
                point.AxisLabel = dataPoint.Key;
                point.YValues = new double[] { dataPoint.Value };
                point.LegendText = $"{dataPoint.Key}: {dataPoint.Value} loans";
                durationSeries.Points.Add(point);
            }

            // Legend configuration
            chartTopBooks.Legends[0].Enabled = false;

            chartBorrowingStatus.Legends[0].Docking = Docking.Bottom;
            chartBorrowingStatus.Legends[0].Alignment = StringAlignment.Center;
            chartBorrowingStatus.Legends[0].Font = new Font("Arial", 10);

            // Overdue Analysis (Main Chart)
            string queryOverdueAnalysis = @"
                SELECT 
                    FORMAT(BT.BORROWDATE, 'yyyy-MM-dd') AS BorrowDate,
                    SUM(CASE WHEN BT.RETURNDATE > BT.DUEDATE THEN 1 ELSE 0 END) AS OverdueCount,
                    SUM(CASE WHEN BT.RETURNDATE <= BT.DUEDATE THEN 1 ELSE 0 END) AS OnTimeCount
                FROM 
                    BORROWINGTICKETS BT
                WHERE 
                    BT.BORROWDATE BETWEEN @StartDate AND @EndDate 
                    AND BT.APPROVAL_STATUS = 'Approve'
                    AND BT.STATUS = 'Restored'
                GROUP BY 
                    FORMAT(BT.BORROWDATE, 'yyyy-MM-dd')
                ORDER BY 
                    BorrowDate";

            var overdueData = GetDatabase.Instance.GetDataTable(queryOverdueAnalysis, parameters);

            // Setup Overdue Chart
            chartMainStats.Series.Clear();
            var overdueSeries = chartMainStats.Series.Add("Overdue Loans");
            var onTimeSeries = chartMainStats.Series.Add("On Time Loans");

            // Configure series types
            overdueSeries.ChartType = SeriesChartType.Line;
            overdueSeries.MarkerStyle = MarkerStyle.Circle;
            onTimeSeries.ChartType = SeriesChartType.Line;
            onTimeSeries.MarkerStyle = MarkerStyle.Circle;

            // Add data points
            foreach (DataRow row in overdueData.Rows)
            {
                string date = row["BorrowDate"].ToString();
                int overdueCount = Convert.ToInt32(row["OverdueCount"]);
                int onTimeCount = Convert.ToInt32(row["OnTimeCount"]);

                overdueSeries.Points.AddXY(date, overdueCount);
                onTimeSeries.Points.AddXY(date, onTimeCount);
            }

            // Configure chart appearance
            chartMainStats.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            chartMainStats.ChartAreas[0].AxisX.Interval = 1;
            chartMainStats.ChartAreas[0].AxisY.Title = "Number of Loans";
            chartMainStats.ChartAreas[0].AxisY.LabelStyle.Format = "N0";
        }
        private void UpdateTotalLabel(string seriesName)
        {
            Series targetSeries = chartMainStats.Series.FindByName(seriesName);

            if (targetSeries == null || targetSeries.Points.Count == 0)
            {
                lblTotal.Text = "Total: 0";
                return;
            }

            decimal total = targetSeries.Points.Sum(point => Convert.ToDecimal(point.YValues[0]));

            // Xác định đơn vị dựa trên tên series
            string unit = GetUnitForSeries(seriesName);

            // Định dạng hiển thị
            lblTotal.Text = $"Total: {total:#,##0} {unit}";
        }

        private string GetUnitForSeries(string seriesName)
        {
            switch (seriesName)
            {
                case "Total Loans":
                case "Returned Loans":
                case "Unique Readers":
                case "LoanCount":
                case "ReaderCount":
                case "OverdueCount":
                case "OnTimeCount":
                case "LateReturnsCount":
                    return "loans";

                case "Average Book Age by Category":
                    return "years";

                case "Reader Location Distribution":
                    return "readers";

                case "Overdue Loans":
                    return "late turns";

                case "AvgDelayDays":
                    return "days late";

                case "AvgDays":
                    return "day";

                case "Percentage":
                    return "%";

                default:
                    return string.Empty;
            }
        }
        

        private void btnDisplay_Click(object sender, EventArgs e)
        {
            if (cbbFilterStatistic.SelectedIndex != -1)
            {
                string selectedTab = cbbFilterStatistic.Text;
                switch (selectedTab)
                {
                    case "OVERALL STATISTICS":
                        ClearAllCharts();
                        LoadOverallStatistics();
                        UpdateTotalLabel("Total Loans");
                        break;
                    case "BOOK CATEGORIES":
                        ClearAllCharts();
                        LoadCategoryStatistics();
                        UpdateTotalLabel("Average Book Age by Category");
                        break;
                    case "READER ANALYSIS":
                        ClearAllCharts();
                        LoadReaderStatistics();
                        UpdateTotalLabel("Reader Location Distribution");
                        break;
                    case "BORROWING TRENDS":
                        ClearAllCharts();
                        LoadBorrowingTrends();
                        UpdateTotalLabel("Overdue Loans");
                        break;
                    default:
                        break;
                }
            }
        }

        private void ClearAllCharts()
        {
            if (chartTopBooks != null && chartTopBooks.Series.Count > 0)
            {
                chartTopBooks.Series.Clear();
                chartTopBooks.Titles.Clear();
                chartTopBooks.ChartAreas[0].RecalculateAxesScale();
            }

            if (chartBorrowingStatus != null && chartBorrowingStatus.Series.Count > 0)
            {
                chartBorrowingStatus.Series.Clear();
                chartBorrowingStatus.Titles.Clear();
                chartBorrowingStatus.ChartAreas[0].RecalculateAxesScale();
            }

            if (chartMainStats != null && chartMainStats.Series.Count > 0)
            {
                chartMainStats.Series.Clear();
                chartMainStats.Titles.Clear();
                chartMainStats.ChartAreas[0].RecalculateAxesScale();
            }
        }

        private void cbbFilterStatistic_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbFilterStatistic.SelectedIndex != -1)
            {
                string selectedTab = cbbFilterStatistic.Text;
                switch (selectedTab)
                {
                    case "OVERALL STATISTICS":
                        rdoMonth.Visible = true;
                        rdoYear.Visible = true;
                        rdoMonth.Checked = false;
                        rdoYear.Checked = false;
                        ClearAllCharts();
                        grb1.Text = "Most Borrowed Books";
                        grb2.Text = "Borrowing Status";
                        grb3.Text = "Monthly/Yearly Statistics";
                        break;
                    case "BOOK CATEGORIES":
                        rdoMonth.Visible = false;
                        rdoYear.Visible = false;
                        ClearAllCharts();
                        grb1.Text = "Category Popularity";
                        grb2.Text = "Overdue Rate by Category";
                        grb3.Text = "Average Book Age by Category";
                        break;
                    case "READER ANALYSIS":
                        rdoMonth.Visible = false;
                        rdoYear.Visible = false;
                        ClearAllCharts();
                        grb1.Text = "Most Active Readers";
                        grb2.Text = "Reader Age Groups";
                        grb3.Text = "Reader Location Distribution";
                        break;
                    case "BORROWING TRENDS":
                        rdoMonth.Visible = false;
                        rdoYear.Visible = false;
                        ClearAllCharts();
                        grb1.Text = "Hourly Borrowing Patterns";
                        grb2.Text = "Loan Duration";
                        grb3.Text = "Overdue vs On Time Loans";
                        break;
                    default:
                        break;
                }
            }
        }
        private void ExportOverallStatistics(string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Overall Statistics");

                // Thêm dữ liệu từ chartTopBooks (Most Borrowed Books)
                worksheet.Cell(1, 1).Value = "Most Borrowed Books";
                worksheet.Cell(2, 1).Value = "Book Title";
                worksheet.Cell(2, 2).Value = "Borrow Count";

                int row = 3;
                foreach (var point in chartTopBooks.Series[0].Points)
                {
                    worksheet.Cell(row, 1).Value = point.AxisLabel;
                    worksheet.Cell(row, 2).Value = point.YValues[0];
                    row++;
                }

                // Thêm dữ liệu từ chartBorrowingStatus (Borrowing Status)
                worksheet.Cell(1, 4).Value = "Borrowing Status";
                worksheet.Cell(2, 4).Value = "Status";
                worksheet.Cell(2, 5).Value = "Count";

                row = 3;
                foreach (var point in chartBorrowingStatus.Series[0].Points)
                {
                    worksheet.Cell(row, 4).Value = point.AxisLabel;
                    worksheet.Cell(row, 5).Value = point.YValues[0];
                    row++;
                }

                // Thêm dữ liệu từ chartMainStats (Monthly/Yearly Statistics)
                worksheet.Cell(1, 7).Value = "Time Statistics";
                worksheet.Cell(2, 7).Value = "Period";
                worksheet.Cell(2, 8).Value = "Total Loans";
                worksheet.Cell(2, 9).Value = "Returned Loans";
                worksheet.Cell(2, 10).Value = "Unique Readers";

                row = 3;
                for (int i = 0; i < chartMainStats.Series[0].Points.Count; i++)
                {
                    worksheet.Cell(row, 7).Value = chartMainStats.Series[0].Points[i].AxisLabel;
                    worksheet.Cell(row, 8).Value = chartMainStats.Series[0].Points[i].YValues[0];
                    worksheet.Cell(row, 9).Value = chartMainStats.Series[1].Points[i].YValues[0];
                    worksheet.Cell(row, 10).Value = chartMainStats.Series[2].Points[i].YValues[0];
                    row++;
                }

                workbook.SaveAs(filePath);
            }
        }

        private void ExportCategoryStatistics(string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Category Statistics");

                // Thêm dữ liệu từ chartTopBooks (Category Popularity)
                worksheet.Cell(1, 1).Value = "Category Popularity";
                worksheet.Cell(2, 1).Value = "Category";
                worksheet.Cell(2, 2).Value = "Loan Count";

                int row = 3;
                foreach (var point in chartTopBooks.Series[0].Points)
                {
                    worksheet.Cell(row, 1).Value = point.AxisLabel;
                    worksheet.Cell(row, 2).Value = point.YValues[0];
                    row++;
                }

                // Thêm dữ liệu từ chartBorrowingStatus (Category Overdue Rate)
                worksheet.Cell(1, 4).Value = "Category Overdue Rate";
                worksheet.Cell(2, 4).Value = "Category";
                worksheet.Cell(2, 5).Value = "Overdue Rate (%)";

                row = 3;
                foreach (var point in chartBorrowingStatus.Series[0].Points)
                {
                    worksheet.Cell(row, 4).Value = point.AxisLabel;
                    worksheet.Cell(row, 5).Value = point.YValues[0];
                    row++;
                }

                // Thêm dữ liệu từ chartMainStats (Average Book Age)
                worksheet.Cell(1, 7).Value = "Average Book Age by Category";
                worksheet.Cell(2, 7).Value = "Category";
                worksheet.Cell(2, 8).Value = "Average Age (Years)";

                row = 3;
                foreach (var point in chartMainStats.Series[0].Points)
                {
                    worksheet.Cell(row, 7).Value = point.AxisLabel;
                    worksheet.Cell(row, 8).Value = point.YValues[0];
                    row++;
                }

                workbook.SaveAs(filePath);
            }
        }

        private void ExportReaderStatistics(string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reader Statistics");

                // Thêm dữ liệu từ chartTopBooks (Most Active Readers)
                worksheet.Cell(1, 1).Value = "Most Active Readers";
                worksheet.Cell(2, 1).Value = "Reader Name";
                worksheet.Cell(2, 2).Value = "Loan Count";

                int row = 3;
                foreach (var point in chartTopBooks.Series[0].Points)
                {
                    worksheet.Cell(row, 1).Value = point.AxisLabel;
                    worksheet.Cell(row, 2).Value = point.YValues[0];
                    row++;
                }

                // Thêm dữ liệu từ chartBorrowingStatus (Reader Age Groups)
                worksheet.Cell(1, 4).Value = "Reader Age Groups";
                worksheet.Cell(2, 4).Value = "Age Group";
                worksheet.Cell(2, 5).Value = "Reader Count";

                row = 3;
                foreach (var point in chartBorrowingStatus.Series[0].Points)
                {
                    worksheet.Cell(row, 4).Value = point.AxisLabel;
                    worksheet.Cell(row, 5).Value = point.YValues[0];
                    row++;
                }

                // Thêm dữ liệu từ chartMainStats (Reader Location)
                worksheet.Cell(1, 7).Value = "Reader Location Distribution";
                worksheet.Cell(2, 7).Value = "Location";
                worksheet.Cell(2, 8).Value = "Reader Count";

                row = 3;
                foreach (var point in chartMainStats.Series[0].Points)
                {
                    worksheet.Cell(row, 7).Value = point.AxisLabel;
                    worksheet.Cell(row, 8).Value = point.YValues[0];
                    row++;
                }

                workbook.SaveAs(filePath);
            }
        }

        private void ExportBorrowingTrends(string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Borrowing Trends");

                // Thêm dữ liệu từ chartTopBooks (Hourly Patterns)
                worksheet.Cell(1, 1).Value = "Borrowing by Hour";
                worksheet.Cell(2, 1).Value = "Hour";
                worksheet.Cell(2, 2).Value = "Loan Count";

                int row = 3;
                foreach (var point in chartTopBooks.Series[0].Points)
                {
                    worksheet.Cell(row, 1).Value = point.AxisLabel;
                    worksheet.Cell(row, 2).Value = point.YValues[0];
                    row++;
                }

                // Thêm dữ liệu từ chartBorrowingStatus (Loan Duration)
                worksheet.Cell(1, 4).Value = "Loan Duration";
                worksheet.Cell(2, 4).Value = "Duration";
                worksheet.Cell(2, 5).Value = "Count";

                row = 3;
                foreach (var point in chartBorrowingStatus.Series[0].Points)
                {
                    worksheet.Cell(row, 4).Value = point.AxisLabel;
                    worksheet.Cell(row, 5).Value = point.YValues[0];
                    row++;
                }

                // Thêm dữ liệu từ chartMainStats (Overdue vs On Time)
                worksheet.Cell(1, 7).Value = "Return Status Over Time";
                worksheet.Cell(2, 7).Value = "Date";
                worksheet.Cell(2, 8).Value = "On Time";
                worksheet.Cell(2, 9).Value = "Overdue";

                row = 3;
                for (int i = 0; i < chartMainStats.Series[0].Points.Count; i++)
                {
                    worksheet.Cell(row, 7).Value = chartMainStats.Series[0].Points[i].AxisLabel;
                    worksheet.Cell(row, 8).Value = chartMainStats.Series[1].Points[i].YValues[0];
                    worksheet.Cell(row, 9).Value = chartMainStats.Series[0].Points[i].YValues[0];
                    row++;
                }

                workbook.SaveAs(filePath);
            }
        }
        private void btnClearFilter_Click(object sender, EventArgs e)
        {
            ClearAllCharts();
        }

        private void btnExportFile_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Excel Files|*.xlsx";
                    saveFileDialog.Title = "Save Chart Data as Excel";
                    saveFileDialog.FileName = $"Library_Statistics_{DateTime.Now:yyyyMMdd}.xlsx";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog.FileName;

                        // Xác định loại thống kê đang hiển thị
                        string selectedTab = cbbFilterStatistic.Text;
                        switch (selectedTab)
                        {
                            case "OVERALL STATISTICS":
                                ExportOverallStatistics(filePath);
                                break;
                            case "BOOK CATEGORIES":
                                ExportCategoryStatistics(filePath);
                                break;
                            case "READER ANALYSIS":
                                ExportReaderStatistics(filePath);
                                break;
                            case "BORROWING TRENDS":
                                ExportBorrowingTrends(filePath);
                                break;
                        }

                        MessageBox.Show("Export to Excel successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
