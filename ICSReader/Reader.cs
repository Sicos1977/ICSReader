using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using ICSReader.Exceptions;
using ICSReader.Helpers;
using ICSReader.Localization;
using Calendar = Ical.Net.Calendar;

namespace ICSReader
{
    public class Reader
    {
        #region Fields
        /// <summary>
        ///     Used to keep track if we already did write an empty line
        /// </summary>
        private static bool _emptyLineWritten;
        #endregion

        #region CheckFileNameAndOutputFolder
        /// <summary>
        ///     Checks if the <paramref name="inputFile" /> and <paramref name="outputFolder" /> is valid
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFolder"></param>
        /// <exception cref="ArgumentNullException">Raised when the <paramref name="inputFile" /> or <paramref name="outputFolder" /> is null or empty</exception>
        /// <exception cref="FileNotFoundException">Raised when the <paramref name="inputFile" /> does not exists</exception>
        /// <exception cref="DirectoryNotFoundException">Raised when the <paramref name="outputFolder" /> does not exist</exception>
        /// <exception cref="ICSFileTypeNotSupported">Raised when the extension is not .ics</exception>
        private static void CheckFileNameAndOutputFolder(string inputFile, string outputFolder)
        {
            if (string.IsNullOrEmpty(inputFile))
                throw new ArgumentNullException(inputFile);

            if (string.IsNullOrEmpty(outputFolder))
                throw new ArgumentNullException(outputFolder);

            if (!File.Exists(inputFile))
                throw new FileNotFoundException(inputFile);

            if (!Directory.Exists(outputFolder))
                throw new DirectoryNotFoundException(outputFolder);

            var extension = Path.GetExtension(inputFile);
            if (string.IsNullOrEmpty(extension))
                throw new ICSFileTypeNotSupported("Expected .ics extension on the inputFile");

            extension = extension.ToLowerInvariant();

            switch (extension)
            {
                case ".ics":
                    return;

                default:
                    throw new ICSFileTypeNotSupported("Wrong file extension, expected .ics");
            }
        }
        #endregion

        #region ExtractToFolder
        /// <summary>
        /// This method will read the given <paramref name="inputFile"/> convert it to HTML and write it to the <paramref name="outputFolder"/>
        /// </summary>
        /// <param name="inputFile">The vcf file</param>
        /// <param name="outputFolder">The folder where to save the converted vcf file</param>
        /// <returns>String array containing the full path to the converted VCF file</returns>
        /// <exception cref="ArgumentNullException">Raised when the <paramref name="inputFile" /> or <paramref name="outputFolder" /> is null or empty</exception>
        /// <exception cref="FileNotFoundException">Raised when the <paramref name="inputFile" /> does not exists</exception>
        /// <exception cref="DirectoryNotFoundException">Raised when the <paramref name="outputFolder" /> does not exist</exception>
        /// <exception cref="ICSFileTypeNotSupported">Raised when the extension is not .ics</exception>
        public List<string> ExtractToFolder(string inputFile, string outputFolder)
        {
            outputFolder = FileManager.CheckForBackSlash(outputFolder);
            CheckFileNameAndOutputFolder(inputFile, outputFolder);

            using (TextReader textReader = File.OpenText(inputFile))
            {
                var calender = Calendar.Load(textReader);
                return WriteCalender(calender, outputFolder);
            }
        }
        #endregion

        #region WriteTable methods
        /// <summary>
        ///     Writes the start of the table
        /// </summary>
        /// <param name="table">The <see cref="StringBuilder" /> object that is used to write a table</param>
        private static void WriteTableStart(StringBuilder table)
        {
            table.AppendLine("<table style=\"font-family: Times New Roman; font-size: 12pt;\">");
            _emptyLineWritten = false;
        }

        /// <summary>
        ///     Writes a row to the table
        /// </summary>
        /// <param name="table">The <see cref="StringBuilder" /> object that is used to write a table</param>
        /// <param name="label">The label text that needs to be written</param>
        /// <param name="text">The text that needs to be written after the <paramref name="label" /></param>
        private static void WriteTableRow(StringBuilder table, 
                                          string label,
                                          string text)
        {
            var lines = text.Split('\n');
            var newText = string.Empty;

            foreach (var line in lines)
                newText += HttpUtility.HtmlEncode(line) + "<br/>";

            table.AppendLine(
                $"<tr style=\"height: 18px; vertical-align: top; \"><td style=\"font-weight: bold; white-space:nowrap;\">{HttpUtility.HtmlEncode(label)}:</td><td>{newText}</td></tr>");

            _emptyLineWritten = false;
        }

        /// <summary>
        ///     Writes
        /// </summary>
        /// <param name="table"></param>
        private static void WriteEmptyTableRow(StringBuilder table)
        {
            // Prevent that we write 2 empty lines in a row
            if (_emptyLineWritten)
                return;

            table.AppendLine("<tr style=\"height: 18px; vertical-align: top; \"><td>&nbsp;</td><td>&nbsp;</td></tr>");
            _emptyLineWritten = true;
        }

        /// <summary>
        ///     Writes the end of the table
        /// </summary>
        /// <param name="table">The <see cref="StringBuilder" /> object that is used to write a table</param>
        private static void WriteTableEnd(StringBuilder table)
        {
            table.AppendLine("</table><br/>");
        }
        #endregion

        #region WriteCalender
        /// <summary>
        ///     Writes the body of the MSG Contact to html or text and extracts all the attachments. The
        ///     result is return as a List of strings
        /// </summary>
        /// <param name="calendar"><see cref="Calendar" /></param>
        /// <param name="outputFolder">The folder where we need to write the output</param>
        /// <returns></returns>
        private List<string> WriteCalender(Calendar calendar, string outputFolder)
        {
            var fileName = Path.Combine(outputFolder, "calender.html");
            var files = new List<string> { fileName };

            var output = new StringBuilder();

            foreach (var calenderEvent in calendar.Events)
            {
                // Start of table
                WriteTableStart(output);

                if (calenderEvent.Created != null)
                    WriteTableRow(output, LanguageConsts.DtStart, calenderEvent.Created.ToString(LanguageConsts.DateTimeFormat, CultureInfo.InvariantCulture));

                if (calenderEvent.LastModified != null)
                    WriteTableRow(output, LanguageConsts.DtStart, calenderEvent.LastModified.ToString(LanguageConsts.DateTimeFormat, CultureInfo.InvariantCulture));

                WriteEmptyTableRow(output);

                if (!string.IsNullOrEmpty(calenderEvent.Organizer?.CommonName))
                    WriteTableRow(output, LanguageConsts.OrganizerName, calenderEvent.Organizer.CommonName);
                
                // Attendees
                if (calenderEvent.Attendees?.Count > 0)
                {
                    var attendees = new List<string>();

                    foreach (var attendee in calenderEvent.Attendees)
                        attendees.Add($"{attendee.CommonName}{(!string.IsNullOrWhiteSpace(attendee.Role) ? $" ({attendee.Role})" : string.Empty)} ");

                    WriteTableRow(output, LanguageConsts.Attendee, string.Join("; ", attendees));
                }
                    
                WriteEmptyTableRow(output);

                if (calenderEvent.DtStart != null)
                    WriteTableRow(output, LanguageConsts.DtStart, calenderEvent.DtStart.AsSystemLocal.ToString(LanguageConsts.DateTimeFormat, CultureInfo.InvariantCulture));

                if (calenderEvent.DtEnd != null)
                    WriteTableRow(output, LanguageConsts.DtEnd, calenderEvent.DtEnd.AsSystemLocal.ToString(LanguageConsts.DateTimeFormat, CultureInfo.InvariantCulture));

                WriteTableRow(output, LanguageConsts.Duration, calenderEvent.Duration.ToString());

                WriteTableRow(output, LanguageConsts.IsAllDay, calenderEvent.IsAllDay ? LanguageConsts.Yes : LanguageConsts.No);

                WriteEmptyTableRow(output);

                if (!string.IsNullOrWhiteSpace(calenderEvent.Location))
                    WriteTableRow(output, LanguageConsts.Location, calenderEvent.Location);

                WriteEmptyTableRow(output);
                
                if (!string.IsNullOrWhiteSpace(calenderEvent.Status))
                    WriteTableRow(output, LanguageConsts.Status, calenderEvent.Status);

                WriteEmptyTableRow(output);

                if (calenderEvent.Resources?.Count > 0)
                    WriteTableRow(output, LanguageConsts.Resources, string.Join("; ", calenderEvent.Resources));

                WriteEmptyTableRow(output);
                
                if (calenderEvent.Categories?.Count > 0)
                    WriteTableRow(output, LanguageConsts.Categories, string.Join("; ", calenderEvent.Categories));

                WriteEmptyTableRow(output);

                if (!string.IsNullOrWhiteSpace(calenderEvent.Description))
                    WriteTableRow(output, LanguageConsts.Description, calenderEvent.Description);

                WriteEmptyTableRow(output);
                
                if (calenderEvent.Comments?.Count > 0)
                    WriteTableRow(output, LanguageConsts.Comments, string.Join("; ", calenderEvent.Comments));

                WriteEmptyTableRow(output);

                if (!string.IsNullOrWhiteSpace(calenderEvent.Summary))
                    WriteTableRow(output, LanguageConsts.Summary, calenderEvent.Summary);

                WriteEmptyTableRow(output);
                
                if (calenderEvent.Contacts?.Count > 0)
                    WriteTableRow(output, LanguageConsts.Contacts, string.Join("; ", calenderEvent.Contacts));

                WriteTableEnd(output);

                // Write the body to a file
                File.WriteAllText(FileManager.FileExistsMakeNew(fileName), output.ToString(), Encoding.UTF8);

                var i = 1;

                foreach (var attachment in calenderEvent.Attachments)
                {
                    var attachmentFileName = $"attachment{i}";
                    var fileTypeFileInfo = FileTypeSelector.GetFileTypeFileInfo(attachment.Data);

                    if (fileTypeFileInfo != null)
                    {
                        attachmentFileName = Path.Combine(outputFolder, attachmentFileName + fileTypeFileInfo.Extension);
                        attachmentFileName = FileManager.FileExistsMakeNew(attachmentFileName);
                        File.WriteAllBytes(attachmentFileName, attachment.Data);
                    }

                    i += 1;
                }
            }

            return files;
        }
        #endregion
    }
}
