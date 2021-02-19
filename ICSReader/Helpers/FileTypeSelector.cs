//
// FileTypeSelector.cs
//
// Author: Kees van Spelde <sicos2002@hotmail.com>
//
// Copyright (c) 2021 Magic-Sessions. (www.magic-sessions.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace ICSReader.Helpers
{
    /// <summary>
    /// Deze classe wordt gebruikt om de informatie van een bestand in op te slaan. 
    /// Zoals de extensie en een beschrijving van het bestand.
    /// </summary>
    public class FileTypeFileInfo
    {
        #region Fields
        private byte[] _magicBytes;
        #endregion

        #region Properties
        /// <summary>
        /// De bytes waaraan we het bestand kunnen herkennen
        /// </summary>
        public byte[] MagicBytes
        {
            get => _magicBytes;
            set
            {
                _magicBytes = value;
                if (_magicBytes != null)
                    MagicBytesAsString = Encoding.ASCII.GetString(_magicBytes);
            }
        }

        /// <summary>
        /// The offset we goto before checking for the magic bytes
        /// </summary>
        public int OffSet { get; }

        /// <summary>
        /// We zetten de Magic Bytes om naar een string. Deze hebben we nodig om snel
        /// op een patern te kunnen zoeken
        /// </summary>
        public string MagicBytesAsString { get; private set; }

        /// <summary>
        /// Extensie van het bestand, bijvoorbeeld "tif", "jpg", etc...
        /// </summary>
        public string Extension { get; }

        /// <summary>
        /// Omschrijving van het bestand
        /// </summary>
        public string Description { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="offset">De offset</param>
        /// <param name="magicBytes">De bytes waaraan we het bestand kunnen herkennen</param>
        /// <param name="extension">De extensie van het bestand zonder punt (bv. doc, xls, msg)</param>
        /// <param name="description">De omschrijving van het bestnad (bv. Word document, Excel werkblad, Outlook bericht)</param>
        internal FileTypeFileInfo(int offset, byte[] magicBytes, string extension, string description)
        {
            OffSet = offset;
            MagicBytes = magicBytes;
            Extension = extension;
            Description = description;
        }
        #endregion
    }

    /// <summary>
    /// Met deze classe kan bepaald worden wat voor type een bestand is.
    /// Hiervoor wordt niet naar de extensie van het document gekeken maar naar de zogenaamde
    /// Magic Bytes in een bestand.
    /// </summary>
    public class FileTypeSelector
    {
        #region Consts
        private const string Executable = "EXECUTABLE";
        private const string MicrosoftOffice = "MICROSOFTOFFICE";
        private const string ZipOrOpenOfficeFormat = "ZIPOROPENOFFICEFORMAT";
        private const string ByteOrderMarkerFile = "BYTEORDERMARKERFILE";
        private const string RiffContainer = "RIFFCONTAINER";
        #endregion

        #region Fields
        /// <summary>
        /// Deze dictionary bevat alle bekende magic bytes van de te herkennen bestand + de informatie die bij het bestand hoort.
        /// </summary>
        private static readonly List<FileTypeFileInfo> FileTypes = GetFileTypes();
        #endregion

        #region STB
        /// <summary>
        /// Zet een string om naar een byte array
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static byte[] Stb(string line)
        {
            return Encoding.ASCII.GetBytes(line);
        }
        #endregion

        #region GetFileTypes
        /// <summary>
        /// Geeft alle bekende FileTypen
        /// </summary>
        /// <returns></returns>
        private static List<FileTypeFileInfo> GetFileTypes()
        {
            // ReSharper disable UseObjectOrCollectionInitializer
            var fileTypes = new List<FileTypeFileInfo>();
            // ReSharper restore UseObjectOrCollectionInitializer

            // Microsoft binary formats
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, MicrosoftOffice, "Microsoft Office applications (Word, Powerpoint, Excel, Works)"));

            // Microsoft open document file format of zip
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x50, 0x4B }, ZipOrOpenOfficeFormat, "Zip or Microsoft Office 2007 document"));

            // PDF
            fileTypes.Add(new FileTypeFileInfo(0, Stb("%PDF-1.7"), ".pdf", "Adobe Portable Document file (version 1.7)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("%PDF-1.6"), ".pdf", "Adobe Portable Document file (version 1.6)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("%PDF-1.5"), ".pdf", "Adobe Portable Document file (version 1.5)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("%PDF-1.4"), ".pdf", "Adobe Portable Document file (version 1.4)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("%PDF-1.3"), ".pdf", "Adobe Portable Document file (version 1.3)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("%PDF-1.2"), ".pdf", "Adobe Portable Document file (version 1.2)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("%PDF-1.1"), ".pdf", "Adobe Portable Document file (version 1.1)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("%PDF-1.0"), ".pdf", "Adobe Portable Document file (version 1.0)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("%PDF"), ".pdf", "Adobe Portable Document file"));

            // Webarchive
            fileTypes.Add(new FileTypeFileInfo(0, Stb("bplist"), ".webarchive", "Safari webarchive"));

            // Winmail.dat
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x78, 0x9F, 0x3E, 0x22 }, ".dat", "Microsoft Outlook winmail.dat file"));

            // RTF
            fileTypes.Add(new FileTypeFileInfo(0, Stb("{\\rtf1"), ".rtf", "Rich Text Format"));
            
            // FileNet COLD document
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0xC5, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0D }, ".cold", "FileNet COLD document"));

            // Developing
            fileTypes.Add(new FileTypeFileInfo(0, Stb("# Microsoft Developer Studio"), ".dsp", "Microsoft Developer Studio project file"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("dswfile"), ".dsp", "Microsoft Visual Studio workspace file"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("#!/usr/bin/perl"), ".pl", "Perl script file"));

            // Corel Paint Shop Pro
            fileTypes.Add(new FileTypeFileInfo(0, Stb("Paint Shop Pro Image File"), ".pspimage", "Corel Paint Shop Pro Image file"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("JASC BROWS FILE"), ".jbf", "Corel Paint Shop Pro browse file"));

            // Microsoft Access
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x00, 0x01, 0x00, 0x00, 0x53, 0x74, 0x61, 0x6E, 0x64, 0x61, 0x72, 0x64, 0x20, 0x4A, 0x65, 0x74, 0x20, 0x44, 0x42 }, ".mdb", "Microsoft Access file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x00, 0x01, 0x00, 0x00, 0x53, 0x74, 0x61, 0x6E, 0x64, 0x61, 0x72, 0x64, 0x20, 0x41, 0x43, 0x45, 0x20, 0x44, 0x42 }, ".accdb", "Microsoft Access 2007 file"));

            // Microsoft Outlook
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x9C, 0xCB, 0xCB, 0x8D, 0x13, 0x75, 0xD2, 0x11, 0x91, 0x58, 0x00, 0xC0, 0x4F, 0x79, 0x56, 0xA4 }, ".wab", "Outlook address file"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("!BD"), ".pst", "Microsoft  Outlook Personal Folder File"));

            // ZIP
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x01, 0x00 }, ".zip", "ZLock Pro encrypted ZIP"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("WinZip"), ".zip", "WinZip compressed archive"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("PKLITE"), ".zip", "PKLITE compressed ZIP archive (see also PKZIP)"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C }, ".7z", "7-Zip compressed file")); // 7Z zip formaat	
            fileTypes.Add(new FileTypeFileInfo(0, Stb("PKSFX"), ".zip", "PKSFX self-extracting executable compressed file (see also PKZIP)"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x1F, 0x8B, 0x08 }, ".gz", "GZIP archive file"));

            // XML
            fileTypes.Add(new FileTypeFileInfo(0, Stb("<?xml version=\"1.0\"?>"), ".xml", "XML File (UTF16 encoding)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("<?xml version=\"1.0\" encoding=\"utf-16\""), ".xml", "XML File (UTF16 encoding)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("<?xml version=\"1.0\" encoding=\"utf-8\""), ".xml", "XML File (UTF8 encoding)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("<?xml version=\"1.0\" encoding=\"utf-7\""), ".xml", "XML File (UTF7 encoding)"));

            // EML
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x52, 0x65, 0x74, 0x75, 0x72, 0x6E, 0x2D, 0x50, 0x61, 0x74, 0x68, 0x3A, 0x20 }, ".eml", "A commmon file extension for e-mail files"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x46, 0x72, 0x6F, 0x6D, 0x20, 0x3F, 0x3F, 0x3F }, ".eml", "E-mail markup language file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x46, 0x72, 0x6F, 0x6D, 0x20, 0x20, 0x20 }, ".eml", "E-mail markup language file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x46, 0x72, 0x6F, 0x6D, 0x3A, 0x20 }, ".eml", "E-mail markup language file"));

            // TIF
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x4D, 0x4D, 0x00, 0x2B }, ".tif", "BigTIFF files; Tagged Image File Format files > 4 GB"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x4D, 0x4D, 0x00, 0x2A }, ".tif", "Tagged Image File Format file (big endian, i.e., LSB last in the byte; Motorola)"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x49, 0x49, 0x2A, 0x00 }, ".tif", "Tagged Image File Format file (little endian, i.e., LSB first in the byte; Intel)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("I I"), ".tif", "Tagged Image File Format file"));

            // AutoCAD
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x41, 0x43, 0x31, 0x30, 0x30, 0x32 }, ".dwg", "Generic AutoCAD drawing - AutoCAD R2.5"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x41, 0x43, 0x31, 0x30, 0x30, 0x33 }, ".dwg", "Generic AutoCAD drawing - AutoCAD R2.6"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x41, 0x43, 0x31, 0x30, 0x30, 0x34 }, ".dwg", "Generic AutoCAD drawing - AutoCAD R9"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x41, 0x43, 0x31, 0x30, 0x30, 0x36 }, ".dwg", "Generic AutoCAD drawing - AutoCAD R10"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x41, 0x43, 0x31, 0x30, 0x30, 0x39 }, ".dwg", "Generic AutoCAD drawing - AutoCAD R11/R12"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x41, 0x43, 0x31, 0x30, 0x31, 0x30 }, ".dwg", "Generic AutoCAD drawing - AutoCAD R13 (subtype 10)"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x41, 0x43, 0x31, 0x30, 0x31, 0x31 }, ".dwg", "Generic AutoCAD drawing - AutoCAD R13 (subtype 11)"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x41, 0x43, 0x31, 0x30, 0x31, 0x32 }, ".dwg", "Generic AutoCAD drawing - AutoCAD R13 (subtype 12)"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x41, 0x43, 0x31, 0x30, 0x31, 0x33 }, ".dwg", "Generic AutoCAD drawing - AutoCAD R13 (subtype 13)"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x41, 0x43, 0x31, 0x30, 0x31, 0x34 }, ".dwg", "Generic AutoCAD drawing - AutoCAD R13 (subtype 14)"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x41, 0x43, 0x31, 0x30, 0x31, 0x35 }, ".dwg", "Generic AutoCAD drawing - AutoCAD R2000"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x41, 0x43, 0x31, 0x30, 0x31, 0x38 }, ".dwg", "Generic AutoCAD drawing - AutoCAD R2004"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x41, 0x43, 0x31, 0x30, 0x32, 0x31 }, ".dwg", "Generic AutoCAD drawing - AutoCAD R2007"));

            // HEIF
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, 0x68, 0x65, 0x69, 0x63 }, ".heif", "HEIF/HEVC"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x68, 0x65, 0x69, 0x63 }, ".heif", "HEIF/HEVC"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x00, 0x00, 0x00, 0x24, 0x66, 0x74, 0x79, 0x70, 0x68, 0x65, 0x69, 0x63 }, ".heif", "HEIF/HEVC"));
            
            // WMF
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0xD7, 0xCD, 0xC6, 0x9A }, ".wmf", "Windows metafile"));

            // JPG
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }, ".jpg", "Samsung D807 JPEG file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, ".jpg", "JPEG/JIFF file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }, ".jpg", "JPEG/Exif file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 }, ".jpg", "Canon EOS-1D JPEG file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }, ".jpg", "Samsung D500 JPEG file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }, ".jpg", "Still Picture Interchange File Format (SPIFF)"));

            fileTypes.Add(new FileTypeFileInfo(0, Stb("RIFF"), RiffContainer, "WAV or AVI"));
            
            // PNG
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x89, 0x50, 0x4E, 0x47 }, ".png", "Portable Network Graphics"));

            // RealAudio
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x2E, 0x52, 0x4D, 0x46, 0x00, 0x00, 0x00, 0x12, 0x00 }, ".ra", "RealAudio file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x2E, 0x72, 0x61, 0xFD, 0x00 }, ".ra", "RealAudio streaming media file"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb(".REC"), ".ivr", "RealPlayer video file (V11 and later)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb(".RMF"), ".rm", "RealMedia streaming media file"));

            // MP3
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x49, 0x44, 0x33 }, ".mp3", "MPEG-1 Layer 3 file without an ID3 tag or with an ID3v1 tag (which's appended at the end of the file)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("ID3"), ".mp3", "MPEG-1 Audio Layer 3 (MP3) audio file"));

            // IMG
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x00, 0x01, 0x00, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01 }, ".img", "Image Format Bitmap file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x50, 0x49, 0x43, 0x54, 0x00, 0x08 }, ".img", "ADEX Corp. ChromaGraph Graphics Card Bitmap Graphic file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x53, 0x43, 0x4D, 0x49 }, ".img", "Img Software Set Bitmap"));

            // GIF
            fileTypes.Add(new FileTypeFileInfo(0, Stb("GIF87a"), ".gif", "Graphics interchange format file (GIF87a)"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("GIF89a"), ".gif", "Graphics interchange format file (GIF89a)"));

            // BMP
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x42, 0x4D }, ".bmp", "Windows (or device-independent) bitmap image"));
            
            // MDI
            fileTypes.Add(new FileTypeFileInfo(0, Stb("MThd"), ".mdi", "Musical Instrument Digital Interface (MIDI) sound file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x45, 0x50 }, ".mdi", "Microsoft Document Imaging file"));

            // WRI
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x32, 0xBE }, ".wri", "Microsoft Write file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x31, 0xBE }, ".wri", "Microsoft Write file"));

            // ARC
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x1A, 0x02 }, ".arc", "LH archive file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x1A, 0x03 }, ".arc", "LH archive file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x1A, 0x04 }, ".arc", "LH archive file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x1A, 0x08 }, ".arc", "LH archive file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x1A, 0x09 }, ".arc", "LH archive file"));

            // Windows Event viewer 
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x45, 0x6C, 0x66, 0x46, 0x69, 0x6C, 0x65, 0x00 }, ".evtx", "Windows Vista event log file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x30, 0x00, 0x00, 0x00, 0x4C, 0x66, 0x4C, 0x65 }, ".evt", "Windows Event Viewer file"));

            // Microsoft Help File
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF }, ".hlp", "Windows help file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x4C, 0x4E, 0x02, 0x00 }, ".hlp", "Windows Help file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x3F, 0x5F, 0x03, 0x00 }, ".hlp", "Windows help file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x49, 0x54, 0x53, 0x46 }, ".chm", "Microsoft Compiled HTM   L Help File"));

            // SWF
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x46, 0x57, 0x53 }, ".swf", "Macromedia Shockwave Flash player file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x43, 0x57, 0x53 }, ".swf", "Shockwave Flash file (v5+)"));

            // CAB
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x4D, 0x53, 0x43, 0x46 }, ".cab", "Microsoft cabinet file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x49, 0x53, 0x63, 0x28 }, ".cab", "Install Shield v5.x or 6.x compressed file"));

            // vCard
            fileTypes.Add(new FileTypeFileInfo(0, Stb("BEGIN:VCARD"), ".vcf", "vCard file"));

            // RAR
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 }, ".rar", "WinRAR compressed archive file"));

            // LHA
            fileTypes.Add(new FileTypeFileInfo(0, Stb("-lh"), ".lha", "Compressed archive file"));

            // Adobe PhotoShop
            fileTypes.Add(new FileTypeFileInfo(0, Stb("8BPS"), ".psd", "Photoshop image file"));

            // Apple Quicktime
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x0B, 0x77 }, ".ac3", "Apple QuickTime movie"));  // Kan ook DLL en dat soort dingen zijn

            // MKV
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x1A, 0x45, 0xD5, 0xA3, 0x93, 0x42, 0x82, 0x88, 0x6D, 0x61, 0x74, 0x72, 0x6F, 0x73, 0x6B }, ".mkv", "Matroska open movie format"));

            // Ogg
            fileTypes.Add(new FileTypeFileInfo(0, Stb("OggS"), ".opus", "Opus Interactive Audio Codec"));

            // Windows Media
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C }, ".wmv", "Microsoft Windows Media Audio/Video File (Advanced Streaming Format"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C }, ".wma", "	Microsoft Windows Media Audio/Video File (Advanced Streaming Format)"));

            // Apple quicktime
            fileTypes.Add(new FileTypeFileInfo(4, Stb("ftyp3gp"), ".3gp", "3rd Generation Partnership Project 3GPP multimedia files"));
            fileTypes.Add(new FileTypeFileInfo(4, Stb("ftypM4A"), ".m4a", "Apple Lossless Audio Codec file"));
            fileTypes.Add(new FileTypeFileInfo(4, Stb("ftypM4V"), ".m4v", "ISO Media, MPEG v4 system, or iTunes AVC-LC file"));
            fileTypes.Add(new FileTypeFileInfo(4, Stb("ftypMSNV"), ".mp4", "MPEG-4 video file"));
            fileTypes.Add(new FileTypeFileInfo(4, Stb("ftypisom"), ".mp4", "ISO Base Media file (MPEG-4) v1"));
            fileTypes.Add(new FileTypeFileInfo(4, Stb("ftypmp42"), ".m4v", "MPEG-4 video|QuickTime file"));
            fileTypes.Add(new FileTypeFileInfo(4, Stb("ftypqt"), ".mov", "QuickTime movie file"));
            fileTypes.Add(new FileTypeFileInfo(4, Stb("moov"), ".mov", "QuickTime movie file"));
            
            // NT Backup
            fileTypes.Add(new FileTypeFileInfo(0, Stb("TAPE"), ".bkf", "Windows NT Backup file (NTBackup)"));

            // Windows registry file
            fileTypes.Add(new FileTypeFileInfo(0, Stb("Windows Registry Editor Version 5.00"), ".reg", "Windows Registry Editor Version 5.00 file"));

            // Others
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x0, 0x05, 0x16, 0x07, 0x00, 0x02, 0x00, 0x00, 0x4D, 0x61, 0x63, 0x20, 0x4F, 0x53, 0x20, 0x58 }, ".maxosxattr", "MAC OS X - Attribute file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x0, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, 0x4D, 0x34, 0x41 }, ".m4a", "Apple audio and video files"));

            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11 }, ".wmv", "Advanced Systems Format"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C }, ".wmv", "Advanced Systems Format"));
            
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 }, ".mp4", "MPEG-4 video files"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x33, 0x67, 0x70, 0x35 }, ".mp4", "MPEG-4 video files"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x49, 0x49, 0x1A, 0x00, 0x00, 0x00, 0x48, 0x45, 0x41, 0x50, 0x43, 0x43, 0x44, 0x52, 0x02, 0x00 }, ".crw", "Canon digital camera RAW file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, 0x4D, 0x34, 0x41, 0x20, 0x00, 0x00, 0x00, 0x00 }, ".mov", "Apple QuickTime movie file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x4C, 0x00, 0x00, 0x00, 0x01, 0x14, 0x02, 0x00 }, ".lnk", "Windows shortcut file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x52, 0x45, 0x47, 0x45, 0x44, 0x49, 0x54 }, ".reg", "Windows NT Registry and Registry Undo files"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x43, 0x50, 0x54, 0x46, 0x49, 0x4C, 0x45 }, ".cpt", "Corel Photopaint file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x4A, 0x41, 0x52, 0x43, 0x53, 0x00 }, ".jar", "JARCS compressed archive"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x46, 0x4F, 0x52, 0x4D, 0x00 }, ".aiff", "Audio Interchange File"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x4B, 0x49, 0x00, 0x00 }, ".shd", "Windows 9x printer spool file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x46, 0x4C, 0x56, 0x01 }, ".flv", "Flash video file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x01, 0x0F, 0x00, 0x00 }, ".mdf", "Microsoft SQL Server 2000 database"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x00, 0x00, 0x02, 0x00 }, ".cur", "Windows cursor file"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x00, 0x00, 0x01, 0xBA }, ".vob", "DVD Video Movie File (video/dvd, video/mpeg)"));
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x00, 0x00, 0x01, 0x00 }, ".ico", "Windows icon file"));
            fileTypes.Add(new FileTypeFileInfo(0, Stb("MZ"), Executable, "EXE or DLL file"));  // Kan ook DLL en dat soort dingen zijn

            // EMF
            fileTypes.Add(new FileTypeFileInfo(0, new byte[] { 0x01, 0x00, 0x00, 0x00 }, ".emf", "Extended (Enhanced) Windows Metafile Format"));

            return fileTypes;
        }
        #endregion

        #region IndexOf
        /// <summary>
        /// Omdat er geen IndexOf achtige functies bestaan voor bytes arrays is deze custom functie gemaakt.
        /// Hiermee kan naar een patern bytes in een andere bytes array gezocht worden
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private static int IndexOf(byte[] input, IReadOnlyList<byte> pattern, int startIndex)
        {
            var firstByte = pattern[0];
            int index;

            if ((index = Array.IndexOf(input, firstByte, startIndex)) < 0) return index;
            for (var i = 0; i < pattern.Count; i++)
            {
                if (index + i >= input.Length)
                    return -1;

                if (pattern[i] == input[index + i]) continue;
                index += i;
                IndexOf(input, pattern, index);
            }

            return index;
        }
        #endregion

        #region GetFileTypeFileInfo
        /// <summary>
        /// Geeft de extensie van een bestand door naar de magic bytes van het bestand te kijken.
        /// Wanneer het type niet bepaald kan worden wordt er een lege string geretourneerd
        /// </summary>
        /// <param name="fileName">Het input bestand</param>
        /// <returns>De extensie behorende bij het input bestand inclusief de punt (bijvoorbeeld .doc)</returns>
        public static FileTypeFileInfo GetFileTypeFileInfo(string fileName)
        {
            return GetFileTypeFileInfo(fileName, null);
        }

        /// <summary>
        /// Geeft de extensie van een bestand door naar de magic bytes van het bestand te kijken.
        /// Wanneer het type niet bepaald kan worden wordt er een lege string geretourneerd
        /// </summary>
        /// <param name="data">Byte array van het te detecteren bestand</param>
        /// <returns>De extensie behorende bij het input bestand inclusief de punt (bijvoorbeeld .doc)</returns>
        public static FileTypeFileInfo GetFileTypeFileInfo(byte[] data)
        {
            return GetFileTypeFileInfo(string.Empty, data);
        }

        /// <summary>
        /// Geeft de extensie van een bestand door naar de magic bytes van het bestand te kijken.
        /// Wanneer het type niet bepaald kan worden wordt er een lege string geretourneerd
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static FileTypeFileInfo GetFileTypeFileInfo(string fileName, byte[] data)
        {
            //data = new byte[32];

            if (data == null)
            {
                if (string.IsNullOrEmpty(fileName))
                // ReSharper disable LocalizableElement
                    throw new ArgumentException("Er is geen bestandsnaam opgegeven", nameof(fileName));
                // ReSharper restore LocalizableElement

                using (var fs = File.OpenRead(fileName))
                {
                    // Aan de eerste 128 bytes hebben we genoeg om de bestandstypes te herkennen
                    data = new byte[128];
                    fs.Read(data, 0, 128);
                }
            }
            
            var result = FileTypes.FirstOrDefault(m => data.StartsWith(m.OffSet, m.MagicBytes));

            // Omdat er bepaalde bestanden zijn die we nog nader moeten onderzoeken gooien we deze door de swtich statement
            if (result != null)
            {
                switch (result.Extension)
                {
                    case Executable:
                        return GetBinaryTypeInfo(fileName);

                    case ZipOrOpenOfficeFormat:
                        return CheckZipOrOpenOfficeFormat(fileName);

                    case ByteOrderMarkerFile:
                        // Omdat we een UNICODE bestand hebben converteren we deze terug naar ASCII
                        var temp = StripByteOrderMarker(data);
                        temp = Encoding.ASCII.GetBytes(Encoding.Unicode.GetString(temp));
                        return GetFileTypeFileInfo(fileName, temp);

                    case RiffContainer:
                        var line = Encoding.ASCII.GetString(data).ToUpperInvariant();
                        if (line.Contains("WAVE"))
                            return new FileTypeFileInfo(0, new byte[] {0x52, 0x49, 0x46, 0x46}, ".wav",
                                "Waveform Audio File");

                        if (line.Contains("AVI"))
                            return new FileTypeFileInfo(0, new byte[] {0x52, 0x49, 0x46, 0x46}, ".avi",
                                "Audio Video Interleave video");

                        if (line.Contains("WEBP"))
                            return new FileTypeFileInfo(0, new byte[] {0x52, 0x49, 0x46, 0x46}, ".webp",
                                "WebP is an image format employing both lossy[6] and lossless compression. It is currently developed by Google");

                        if (line.Contains("ACON"))
                            return new FileTypeFileInfo(0, new byte[] {0x52, 0x49, 0x46, 0x46}, ".acon",
                                "Windows animated cursor");

                        if (line.Contains("AMV"))
                            return new FileTypeFileInfo(0, new byte[] {0x52, 0x49, 0x46, 0x46}, ".amv", "MTV Video");

                        if (line.Contains("BND"))
                            return new FileTypeFileInfo(0, new byte[] {0x52, 0x49, 0x46, 0x46}, ".bnd",
                                "RIFF Bundle File");

                        if (line.Contains("CDR"))
                            return new FileTypeFileInfo(0, new byte[] {0x52, 0x49, 0x46, 0x46}, ".cdr", "Coreldraw");

                        if (line.Contains("PAL"))
                            return new FileTypeFileInfo(0, new byte[] {0x52, 0x49, 0x46, 0x46}, ".pal",
                                "RIFF Palette File");

                        if (line.Contains("RDIB"))
                            return new FileTypeFileInfo(0, new byte[] {0x52, 0x49, 0x46, 0x46}, ".rdib",
                                "RDIB (or RIFF DIB) is a raster image file format based on RIFF and BMP. There are two varieties: \"simple RDIB\", which is a standard BMP file in a RIFF container, and \"extended RDIB\", which is more complex");

                        if (line.Contains("RMID"))
                            return new FileTypeFileInfo(0, new byte[] {0x52, 0x49, 0x46, 0x46}, ".rmid",
                                "RIFF MIDI (RMID) is a simple RIFF wrapper for a MIDI music file");

                        if (line.Contains("RMMP"))
                            return new FileTypeFileInfo(0, new byte[] {0x52, 0x49, 0x46, 0x46}, ".rmmp",
                                "RIFF Multimedia Movie (also called RIFF RMMP, MMM movie, etc.) is a RIFF-based video/animation file format. It is perhaps a predecessor of AVI.");

                        if (line.Contains("SHW4"))
                            return new FileTypeFileInfo(0, new byte[] {0x52, 0x49, 0x46, 0x46}, ".shw4",
                                "By SHW, we refer to a family of file formats associated with the presentation software CorelSHOW and Corel Presentations. There are apparently several .SHW formats, which may be quite different from each other.");

                        return new FileTypeFileInfo(0, null, string.Empty, "Unknown RIFF container type");
                }
            }

            if (result == null)
                result = InspectTextBasedFileFormats(fileName);

            return result ?? new FileTypeFileInfo(0, null, string.Empty, "Unknown file type");
        }
        #endregion

        #region GetFileExtension
        /// <summary>
        /// Bepaald de extensie van het bestand door naar de interne structuur van het bestand te kijken
        /// </summary>
        /// <param name="fileName">Het input bestand</param>
        /// <returns>De extense, leeg wanneer deze niet bepaald kon worden</returns>
        public string GetFileExtension(string fileName)
        {
            var fileTypeFileInfo = GetFileTypeFileInfo(fileName);
            return fileTypeFileInfo == null ? string.Empty : fileTypeFileInfo.Extension;
        }
        #endregion

        #region GetBinaryTypeInfo
        [DllImport("kernel32")]
        private static extern int GetBinaryType(string lpApplicationName, ref int lpBinaryType);
        /// <summary>
        /// Geeft meer informatie over een EXE file
        /// </summary>
        /// <param name="fileName">Het bestand</param>
        /// <returns>Type bestand of null als er geen informatie over het bestand is</returns>
        private static FileTypeFileInfo GetBinaryTypeInfo(string fileName)
        {
            const int scs32BitBinary = 0;
            const int scsDosBinary = 1;
            const int scsWowBinary = 2;
            const int scsPifBinary = 3;
            const int scsPosixBinary = 4;
            const int scsOs216Binary = 5;
            const int scs64BitBinary = 6;

            var returnCode = -1;

            GetBinaryType(fileName, ref returnCode);

            switch (returnCode)
            {
                case scs32BitBinary:
                    return new FileTypeFileInfo(0, null, ".exe", "32 bits Windows application");

                case scsDosBinary:
                    return new FileTypeFileInfo(0, null, ".exe", "MS-DOS application");

                case scsWowBinary:
                    return new FileTypeFileInfo(0, null, ".exe", "16-bit Windows application");

                case scsPifBinary:
                    return new FileTypeFileInfo(0, null, ".exe", "PIF file that executes an MS-DOS application");

                case scsPosixBinary:
                    return new FileTypeFileInfo(0, null, ".exe", "POSIX application");

                case scsOs216Binary:
                    return new FileTypeFileInfo(0, null, ".exe", "16-bit OS/2 application");

                case scs64BitBinary:
                    return new FileTypeFileInfo(0, null, ".exe", "64 bits Windows application");

                default:
                    return new FileTypeFileInfo(0, null, Path.GetExtension(fileName), string.Empty);
            }
        }
        #endregion
        
        #region InspectTextBasedFileFormats
        private static FileTypeFileInfo InspectTextBasedFileFormats(string fileName)
        {
            var lines = new List<string>();

            using (var streamReader = new StreamReader(fileName))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    line = line.ToLower();
                    lines.Add(line);

                    if (line.Contains("<?xml"))
                        return new FileTypeFileInfo(0, null, ".xml", "Extensible Markup Language");

                    if (line.Contains("mime-version: 1.0"))
                        return new FileTypeFileInfo(0, null, ".eml", "Extended Markup Language");

                    if (line.Contains("<html"))
                        return new FileTypeFileInfo(0, null, ".htm", "Hypertext Markup Language");
                }
            }

            var text = string.Join("\\n", lines);

            if (text.StartsWith("{") && text.EndsWith("}") || text.StartsWith("[") && text.EndsWith("]"))
                return  new FileTypeFileInfo(0, null, ".json", "JavaScript Object Notation");

            return null;
        }
        #endregion

        #region CheckZipOrOpenOfficeFormat
        /// <summary>
        /// Bepaald of het opgegeven bestand een ZIP of een Office 2007 Open document formaat is
        /// </summary>
        /// <param name="fileName">Pad en bestandsnaam</param>
        /// <returns></returns>
        private static FileTypeFileInfo CheckZipOrOpenOfficeFormat(string fileName)
        {
            var fileAscii = Encoding.ASCII.GetString(File.ReadAllBytes(fileName));

            if (fileAscii.Contains("word/_rels/"))
                return new FileTypeFileInfo(0, null, ".docx", "Microsoft Word open XML document format");

            if (fileAscii.Contains("xl/_rels/workbook"))
                return new FileTypeFileInfo(0, null, ".xlsx", "Microsoft Excel open XML document format");

            if (fileAscii.Contains("ppt/slides/_rels"))
                return new FileTypeFileInfo(0, null, ".pptx", "Microsoft PowerPoint open XML document format");

            if (fileAscii.Contains("CHNKWKS"))
                return new FileTypeFileInfo(0, null, ".wks", "Microsoft Works");

            if (fileAscii.Contains("Document.iwa"))
                return new FileTypeFileInfo(0, null, ".iwa", "iWork Archive");

            // Anders is het waarschijnlijk een ZIP bestand
            return new FileTypeFileInfo(0, null, ".zip", "Zip compressed archive");
        }
        #endregion

        #region StripByteOrderMarker
        /// <summary>
        /// Haalt de BOM weg uit de omgegeven magic bytes
        /// </summary>
        /// <param name="magicBytes"></param>
        /// <returns>Byte array zonder BOM</returns>
        private static byte[] StripByteOrderMarker(byte[] magicBytes)
        {
            if (IndexOf(magicBytes, new byte[] { 0xDD, 0x73, 0x66, 0x73 }, 0) == 0 ||
                IndexOf(magicBytes, new byte[] { 0xFF, 0xFE, 0x00, 0x00 }, 0) == 0 ||
                IndexOf(magicBytes, new byte[] { 0x00, 0x00, 0xFE, 0xFF }, 0) == 0 ||
                IndexOf(magicBytes, new byte[] { 0x84, 0x31, 0x95, 0x33 }, 0) == 0)
                return magicBytes.Select(m => m).Skip(4).ToArray();

            if (IndexOf(magicBytes, new byte[] { 0xEF, 0xBB, 0xBF }, 0) == 0 ||
                IndexOf(magicBytes, new byte[] { 0x2B, 0x2F, 0x76 }, 0) == 0 ||
                IndexOf(magicBytes, new byte[] { 0x0E, 0xFE, 0xFF }, 0) == 0 ||
                IndexOf(magicBytes, new byte[] { 0xFB, 0xEE, 0x28 }, 0) == 0 ||
                IndexOf(magicBytes, new byte[] { 0xF7, 0x64, 0x4C }, 0) == 0)
                return magicBytes.Select(m => m).Skip(3).ToArray();

            if (IndexOf(magicBytes, new byte[] { 0xFE, 0xFF }, 0) == 0 ||
                IndexOf(magicBytes, new byte[] { 0xFF, 0xFE }, 0) == 0)
                return magicBytes.Select(m => m).Skip(2).ToArray();

            return null;
        }
        #endregion
    }
}
