using ClosedXML.Excel;

namespace DataWorkers
{
    public class ExcelDataExtractor
    {
        private string ExcelPath;
        private string ProjectId;
        private string SheetName;
        private Dictionary<string, Dictionary<string, Dictionary<string, object>>> TestDescription;
        private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>> TestCases;

        public ExcelDataExtractor(string excelPath, string projectId, string projectType)
        {
            ExcelPath = excelPath;
            ProjectId = projectId;
            SheetName = projectType.ToString();

            TestDescription = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
            TestCases = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>>();
        }

        public void GetTestCases()
        {
            using (var workbook = new XLWorkbook(ExcelPath))
            {
                if (workbook.Worksheets.Contains(SheetName))
                {
                    var sheet = workbook.Worksheet(SheetName);
                    Console.WriteLine($"Processing sheet: {SheetName}");

                    var headerRow = sheet.Row(1);
                    var headers = new List<string>();

                    foreach (var cell in headerRow.CellsUsed())
                    {
                        headers.Add(cell.GetString().Trim());
                    }

                    Console.WriteLine($"Headers extracted from {SheetName}: {string.Join(", ", headers)}");

                    foreach (var row in sheet.RowsUsed().Skip(1))
                    {
                        var extractedTests = new Dictionary<string, object>();

                        for (int i = 0; i < headers.Count; i++)
                        {
                            extractedTests[headers[i]] = row.Cell(i + 1).Value;
                        }

                        if (extractedTests.ContainsKey(ProjectId))
                        {
                            var value = extractedTests[ProjectId]?.ToString()?.ToLower();
                            if (string.IsNullOrEmpty(value) || value == "no" || value == "0" || value == "none")
                            {
                                continue;
                            }
                        }
                        else
                        {
                            throw new Exception("Project ID not found in extracted test cases.");
                        }

                        Console.WriteLine($"Extracted test case: {string.Join(", ", extractedTests)}");

                        var category = extractedTests["Test Category"].ToString();
                        var module = extractedTests["Test Module"].ToString();
                        var testCase = extractedTests["Test Case"].ToString();

                        if (!TestCases.ContainsKey(category))
                            TestCases[category] = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();

                        if (!TestCases[category].ContainsKey(module))
                            TestCases[category][module] = new Dictionary<string, Dictionary<string, object>>();

                        TestCases[category][module][testCase] = extractedTests;
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: Sheet '{SheetName}' not found in the workbook.");
                }
            }
        }

        public void GetAdditionalDescriptions()
        {
            using (var workbook = new XLWorkbook(ExcelPath))
            {
                if (workbook.Worksheets.Contains("Main"))
                {
                    var sheet = workbook.Worksheet("Main");
                    Console.WriteLine("Processing sheet: Main");

                    var headerRow = sheet.Row(1);
                    var headers = new List<string>();

                    foreach (var cell in headerRow.CellsUsed())
                    {
                        headers.Add(cell.GetString().Trim());
                    }

                    Console.WriteLine($"Headers extracted from Main: {string.Join(", ", headers)}");

                    foreach (var row in sheet.RowsUsed().Skip(1))
                    {
                        var extractedTests = new Dictionary<string, object>();

                        for (int i = 0; i < headers.Count; i++)
                        {
                            extractedTests[headers[i]] = row.Cell(i + 1).Value;
                        }

                        Console.WriteLine($"Extracted test case: {string.Join(", ", extractedTests)}");

                        var category = extractedTests["Category"].ToString();
                        var module = extractedTests["Module"].ToString();

                        if (!TestDescription.ContainsKey(category))
                            TestDescription[category] = new Dictionary<string, Dictionary<string, object>>();

                        TestDescription[category][module] = extractedTests;
                    }
                }
                else
                {
                    Console.WriteLine("Warning: Sheet 'Main' not found in the workbook.");
                }
            }
        }

    }
}
