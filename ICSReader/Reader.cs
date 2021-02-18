using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using Ical.Net;

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
                "<tr style=\"height: 18px; vertical-align: top; \"><td style=\"font-weight: bold; white-space:nowrap;\">" +
                HttpUtility.HtmlEncode(label) + ":</td><td>" + newText + "</td></tr>");

            _emptyLineWritten = false;
        }

        /// <summary>
        ///     Writes a row to the table without Html encoding the <paramref name="text" />
        /// </summary>
        /// <param name="header">The <see cref="StringBuilder" /> object that is used to write a table</param>
        /// <param name="label">The label text that needs to be written</param>
        /// <param name="text">The text that needs to be written after the <paramref name="label" /></param>
        private static void WriteTableRowNoEncoding(StringBuilder header,
                                                    string label,
                                                    string text)
        {
            text = text.Replace("\n", "<br/>");

            header.AppendLine(
                "<tr style=\"height: 18px; vertical-align: top; \"><td style=\"font-weight: bold; white-space:nowrap; \">" +
                HttpUtility.HtmlEncode(label) + ":</td><td>" + text + "</td></tr>");

            _emptyLineWritten = false;
        }

        /// <summary>
        ///     Writes a row tot the table and makes <paramref name="text"/> click able (hyperlink) />
        /// </summary>
        /// <param name="header">The <see cref="StringBuilder" /> object that is used to write a table</param>
        /// <param name="label">The label text that needs to be written</param>
        /// <param name="hyperlink">The hyperlink</param>
        /// <param name="text">The text for the hyperlink</param>
        private static void WriteTableRowHyperLink(StringBuilder header,
                                                   string label,
                                                   string hyperlink,
                                                   string text)
        {
            text = text.Replace("\n", "<br/>");

            header.AppendLine(
                "<tr style=\"height: 18px; vertical-align: top; \"><td style=\"font-weight: bold; white-space:nowrap; \">" +
                HttpUtility.HtmlEncode(label) + ":</td><td><a href=\"" + hyperlink + "\">" + text + "<a></td></tr>");

            _emptyLineWritten = false;
        }

        /// <summary>
        ///     Writes a row tot the table and inserts the given <paramref name="imageUrl"/>
        /// </summary>
        /// <param name="header">The <see cref="StringBuilder" /> object that is used to write a table</param>
        /// <param name="label">The label text that needs to be written</param>
        /// <param name="imageUrl">The url to the image</param>
        private static void WriteTableRowImage(StringBuilder header,
                                               string label,
                                               string imageUrl)
        {
            header.AppendLine(
                "<tr style=\"height: 18px; vertical-align: top; \"><td style=\"font-weight: bold; white-space:nowrap; \">" +
                HttpUtility.HtmlEncode(label) + ":</td><td><img src=\"" + imageUrl + "\"></td></tr>");

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
        /// <param name="hyperlinks">When <c>true</c> then hyperlinks are generated for the To, CC, BCC and attachments</param>
        /// <returns></returns>
        private List<string> WriteCalender(Calendar calendar, string outputFolder, bool hyperlinks)
        {
            var fileName = Path.Combine(outputFolder, "calender.html");
            var files = new List<string> { fileName };

            var output = new StringBuilder();

            // Start of table
            WriteTableStart(output);

            //var i = 1;
            //var count = calendar;

            //foreach (var photo in vCard.Photos)
            //{
            //    string photoLabel;
            //    if (count > 1)
            //        photoLabel = LanguageConsts.PhotoLabel + " " + i;
            //    else
            //        photoLabel = LanguageConsts.PhotoLabel;

            //    if (photo.IsLoaded)
            //    {
            //        var tempFileName = Path.Combine(outputFolder, Guid.NewGuid() + ".png");
            //        var bitmap = photo.GetBitmap();
            //        bitmap.Save(tempFileName, ImageFormat.Png);
            //        files.Add(tempFileName);
            //        WriteTableRowImage(output, photoLabel, tempFileName);
            //    }
            //    else
            //    {
            //        if (hyperlinks)
            //            WriteTableRowHyperLink(output, photoLabel, photo.Url.ToString(), photo.Url.ToString());
            //        else
            //            WriteTableRowImage(output, photoLabel, photo.Url.ToString());
            //    }

            //    i += 1;
            //}

            //// Full name
            //if (!string.IsNullOrEmpty(vCard.FormattedName))
            //    WriteTableRow(output, LanguageConsts.DisplayNameLabel, vCard.FormattedName);

            //// Last name
            //if (!string.IsNullOrEmpty(vCard.FamilyName))
            //    WriteTableRow(output, LanguageConsts.SurNameLabel, vCard.FamilyName);

            //// First name
            //if (!string.IsNullOrEmpty(vCard.GivenName))
            //    WriteTableRow(output, LanguageConsts.GivenNameLabel,
            //        vCard.GivenName);

            //// Job title
            //if (!string.IsNullOrEmpty(vCard.Title))
            //    WriteTableRow(output, LanguageConsts.FunctionLabel,
            //        vCard.Title);

            //// Department
            //if (!string.IsNullOrEmpty(vCard.Department))
            //    WriteTableRow(output, LanguageConsts.DepartmentLabel,
            //        vCard.Department);

            //// Company
            //if (!string.IsNullOrEmpty(vCard.Organization))
            //    WriteTableRow(output, LanguageConsts.CompanyLabel, vCard.Organization);

            //// Empty line
            //WriteEmptyTableRow(output);

            //if (vCard.DeliveryLabels.Count == 0)
            //    foreach (var deliveryAddress in vCard.DeliveryAddresses)
            //    {
            //        var address = (!string.IsNullOrWhiteSpace(deliveryAddress.Street) ? deliveryAddress.Street : string.Empty) + Environment.NewLine +
            //            (!string.IsNullOrWhiteSpace(deliveryAddress.PostalCode) ? deliveryAddress.PostalCode : string.Empty) + " " +
            //            (!string.IsNullOrWhiteSpace(deliveryAddress.City) ? deliveryAddress.City : string.Empty) + Environment.NewLine +
            //            (!string.IsNullOrWhiteSpace(deliveryAddress.Region) ? deliveryAddress.Region : string.Empty) + Environment.NewLine +
            //            (!string.IsNullOrWhiteSpace(deliveryAddress.Country) ? deliveryAddress.Country : string.Empty);

            //        // intl, postal, parcel, work
            //        if (deliveryAddress.IsWork)
            //            WriteTableRow(output, LanguageConsts.WorkAddressLabel, address);
            //        else if (deliveryAddress.IsHome)
            //            WriteTableRow(output, LanguageConsts.HomeAddressLabel, address);
            //        else if (deliveryAddress.IsInternational)
            //            WriteTableRow(output, LanguageConsts.InternationalAddressLabel, address);
            //        else if (deliveryAddress.IsPostal)
            //            WriteTableRow(output, LanguageConsts.PostalAddressLabel, address);
            //        else if (deliveryAddress.IsParcel)
            //            WriteTableRow(output, LanguageConsts.ParcelAddressLabel, address);
            //        else if (deliveryAddress.IsDomestic)
            //            WriteTableRow(output, LanguageConsts.DomesticAddressLabel, address);
            //    }

            //// Business address
            //foreach (var deliveryLabel in vCard.DeliveryLabels)
            //{
            //    switch (deliveryLabel.AddressType)
            //    {
            //        case DeliveryAddressTypes.Domestic:
            //            WriteTableRow(output, LanguageConsts.DomesticAddressLabel, deliveryLabel.Text);
            //            break;

            //        case DeliveryAddressTypes.Home:
            //            WriteTableRow(output, LanguageConsts.HomeAddressLabel, deliveryLabel.Text);
            //            break;

            //        case DeliveryAddressTypes.International:
            //            WriteTableRow(output, LanguageConsts.InternationalAddressLabel, deliveryLabel.Text);
            //            break;

            //        case DeliveryAddressTypes.Parcel:
            //            WriteTableRow(output, LanguageConsts.PostalAddressLabel, deliveryLabel.Text);
            //            break;

            //        case DeliveryAddressTypes.Postal:
            //            WriteTableRow(output, LanguageConsts.PostalAddressLabel, deliveryLabel.Text);
            //            break;

            //        case DeliveryAddressTypes.Work:
            //            WriteTableRow(output, LanguageConsts.WorkAddressLabel, deliveryLabel.Text);
            //            break;

            //        case DeliveryAddressTypes.Default:
            //            WriteTableRow(output, LanguageConsts.OtherAddressLabel, deliveryLabel.Text);
            //            break;
            //    }
            //}

            //// Instant messaging
            //if (!string.IsNullOrEmpty(vCard.InstantMessagingAddress))
            //    WriteTableRow(output, LanguageConsts.InstantMessagingAddressLabel, vCard.InstantMessagingAddress);

            //// Empty line
            //WriteEmptyTableRow(output);

            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.Work, PhoneTypes.WorkVoice });
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.Assistant, PhoneTypes.VoiceAssistant });
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.Company, PhoneTypes.VoiceCompany });
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.Home, PhoneTypes.HomeVoice });
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.Cellular, PhoneTypes.CellularVoice });
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.Car, PhoneTypes.CarVoice });
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.Radio, PhoneTypes.VoiceRadio });
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.Pager, PhoneTypes.VoicePager });
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.Callback, PhoneTypes.VoiceCallback });
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.Voice });
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.Preferred });

            //// Telex
            //foreach (var email in vCard.EmailAddresses)
            //{
            //    switch (email.EmailType)
            //    {
            //        case EmailAddressType.Telex:
            //            WriteTableRow(output, LanguageConsts.TelexNumberLabel, email.ToString());
            //            break;
            //    }
            //}
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.Ttytdd });
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.Isdn });
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.Fax });
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.WorkFax });
            //WriteTelephone(vCard, output, new List<PhoneTypes> { PhoneTypes.HomeFax });

            //// Empty line
            //WriteEmptyTableRow(output);

            //i = 1;

            //foreach (var email in vCard.EmailAddresses)
            //{
            //    switch (email.EmailType)
            //    {
            //        case EmailAddressType.AOl:
            //        case EmailAddressType.AppleLink:
            //        case EmailAddressType.AttMail:
            //        case EmailAddressType.CompuServe:
            //        case EmailAddressType.EWorld:
            //        case EmailAddressType.IBMMail:
            //        case EmailAddressType.Internet:
            //        case EmailAddressType.MCIMail:
            //        case EmailAddressType.PowerShare:
            //        case EmailAddressType.Prodigy:
            //            if (i > 1)
            //                WriteEmptyTableRow(output);

            //            if (hyperlinks)
            //                WriteTableRowHyperLink(output, LanguageConsts.EmailEmailAddressLabel + " " + i, "mailto:" + email, email.ToString());
            //            else
            //                WriteTableRowNoEncoding(output, LanguageConsts.EmailEmailAddressLabel + " " + i, email.ToString());

            //            if (!string.IsNullOrEmpty(vCard.FormattedName))
            //                WriteTableRow(output, LanguageConsts.EmailDisplayNameLabel + " " + i,
            //                    vCard.FormattedName + " (" + email + ")");

            //            i += 1;
            //            break;

            //        case EmailAddressType.Telex:
            //            // Ignore
            //            break;

            //    }
            //}

            //// Empty line
            //WriteEmptyTableRow(output);

            //// Birthday
            //if (vCard.BirthDate != null)
            //    WriteTableRow(output, LanguageConsts.BirthdayLabel,
            //        ((DateTime)vCard.BirthDate).ToString(LanguageConsts.DataFormat));

            //// Anniversary
            //if (vCard.Anniversary != null)
            //    WriteTableRow(output, LanguageConsts.WeddingAnniversaryLabel,
            //        ((DateTime)vCard.Anniversary).ToString(LanguageConsts.DataFormat));

            //// Spouse/Partner
            //if (!string.IsNullOrEmpty(vCard.Spouse))
            //    WriteTableRow(output, LanguageConsts.SpouseNameLabel,
            //        vCard.Spouse);

            //// Profession
            //if (!string.IsNullOrEmpty(vCard.Role))
            //    WriteTableRow(output, LanguageConsts.ProfessionLabel,
            //        vCard.Role);

            //// Assistant
            //if (!string.IsNullOrEmpty(vCard.Assistant))
            //    WriteTableRow(output, LanguageConsts.AssistantTelephoneNumberLabel,
            //        vCard.Assistant);


            //// Web page
            //var firstRow = true;
            //foreach (var webpage in vCard.Websites)
            //{
            //    if (!string.IsNullOrEmpty(webpage.Url))
            //    {
            //        if (firstRow)
            //        {
            //            firstRow = false;
            //            if (hyperlinks)
            //                WriteTableRowHyperLink(output, LanguageConsts.HtmlLabel, webpage.Url, webpage.Url);
            //            else
            //                WriteTableRow(output, LanguageConsts.HtmlLabel, webpage.Url);
            //        }
            //        else
            //        {
            //            if (hyperlinks)
            //                WriteTableRowHyperLink(output, string.Empty, webpage.Url, webpage.Url);
            //            else
            //                WriteTableRow(output, string.Empty, webpage.Url);
            //        }
            //    }
            //}

            //// Empty line
            //WriteEmptyTableRow(output);

            //// Categories
            //var categories = vCard.Categories;
            //if (categories != null && categories.Count > 0)
            //    WriteTableRow(output, LanguageConsts.CategoriesLabel,
            //        String.Join("; ", categories));

            //// Empty line
            //WriteEmptyTableRow(output);

            //// Empty line
            //firstRow = true;
            //if (vCard.Notes != null && vCard.Notes.Count > 0)
            //{
            //    foreach (var note in vCard.Notes)
            //    {
            //        if (!string.IsNullOrWhiteSpace(note.Text))
            //        {
            //            if (firstRow)
            //            {
            //                firstRow = false;
            //                WriteTableRow(output, LanguageConsts.NotesLabel, note.Text);
            //            }
            //            else
            //                WriteTableRow(output, string.Empty, note.Text);
            //        }
            //    }
            //}

            //WriteTableEnd(output);

            //// Write the body to a file
            //File.WriteAllText(fileName, output.ToString(), Encoding.UTF8);

            return files;
        }
        #endregion
    }
}
