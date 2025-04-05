using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWorkers
{
    internal class DocWriter
    {
        private string TemplatePath;
        private string OutputPath;
        private string DesignCode;
        private WordprocessingDocument Doc;

        public DocWriter(string templatePath, string designCode, string outputPath)
        {
            TemplatePath = templatePath;
            OutputPath = outputPath;
            DesignCode = designCode;

            File.Copy(TemplatePath, OutputPath, true);
            Doc = WordprocessingDocument.Open(OutputPath, true);
        }

        public Paragraph GetInsertionPoint(string insertionPoint)
        {
            var paragraphs = Doc.MainDocumentPart.Document.Body.Elements<Paragraph>();
            foreach (var paragraph in paragraphs)
            {
                if (paragraph.InnerText.Contains(insertionPoint))
                {
                    return paragraph;
                }
            }
            throw new Exception($"Insertion point '{insertionPoint}' not found in document.");
        }

        public void AddTextWithStyle(Paragraph paragraph, string text, string styleName = null)
        {
            Run run = new Run(new Text(text));
            paragraph.Append(run);
            if (!string.IsNullOrEmpty(styleName))
            {
                paragraph.ParagraphProperties = new ParagraphProperties(new ParagraphStyleId { Val = styleName });
            }
        }

        public void AddTable(List<List<string>> tableData, string tableStyle = "TableGrid")
        {
            Table table = new Table();
            TableProperties tblProps = new TableProperties(new TableStyle { Val = tableStyle });
            table.AppendChild(tblProps);

            foreach (var row in tableData)
            {
                TableRow tableRow = new TableRow();
                foreach (var cellText in row)
                {
                    TableCell tableCell = new TableCell(new Paragraph(new Run(new Text(cellText))));
                    tableRow.Append(tableCell);
                }
                table.Append(tableRow);
            }

            Doc.MainDocumentPart.Document.Body.Append(table);
        }

        public void UpdatePlaceholder(string value, string placeholder)
        {
            var paragraphs = Doc.MainDocumentPart.Document.Body.Elements<Paragraph>();
            foreach (var paragraph in paragraphs)
            {
                if (paragraph.InnerText.Contains(placeholder))
                {
                    paragraph.RemoveAllChildren<Run>();
                    paragraph.Append(new Run(new Text(paragraph.InnerText.Replace(placeholder, value))));
                }
            }
        }

        public void SaveDoc()
        {
            Doc.MainDocumentPart.Document.Save();
            Doc.Dispose();
        }
    }
}
