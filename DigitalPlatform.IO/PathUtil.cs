using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DigitalPlatform.IO
{

    /// <summary>
    /// Path功能扩展函数
    /// </summary>
    public class PathUtil
    {
        // 获得一个目录下的全部文件名。包括子目录中的
        public static List<string> GetFileNames(string strDataDir,
            FileNameFilterProc filter_proc = null)
        {
            DirectoryInfo di = new DirectoryInfo(strDataDir);

            List<string> result = new List<string>();

            if (filter_proc != null && filter_proc(di) == false)
                return result;

            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis)
            {
                if (filter_proc != null && filter_proc(fi) == false)
                    continue;
                result.Add(fi.FullName);
            }

            // 处理下级目录，递归
            DirectoryInfo[] dis = di.GetDirectories();
            foreach (DirectoryInfo subdir in dis)
            {
                if (filter_proc != null && filter_proc(subdir) == false)
                    continue;

                result.AddRange(GetFileNames(subdir.FullName));
            }

            return result;
        }

        public static bool GetWindowsMimeType(string ext, out string mime)
        {
            mime = "application/octet-stream";
            // 2015/11/23 增加 using 部分
            using (Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext))
            {
                if (regKey != null)
                {
                    object val = regKey.GetValue("Content Type");
                    if (val != null)
                    {
                        string strval = val.ToString();
                        if (!(string.IsNullOrEmpty(strval) || string.IsNullOrWhiteSpace(strval)))
                        {
                            mime = strval;
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        // 根据文件内容和扩展名获得 MIME 类型
        public static string MimeTypeFrom(string strFileName)
        {
            string strMime = API.GetMimeTypeFromFile(strFileName);

            // 如果通过内容无法判断，则进一步用文件扩展名判断
            if (strMime == "application/octet-stream")
            {
                string strFileExtension = Path.GetExtension(strFileName).ToLower();
                return GetMimeTypeByFileExtension(strFileExtension);
            }

            return strMime;
        }

        private static IDictionary<string, string> _mappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {

        #region Big freaking list of mime types
        // combination of values from Windows 7 Registry and 
        // from C:\Windows\System32\inetsrv\config\applicationHost.config
        // some added, including .7z and .dat
        {".323", "text/h323"},
        {".3g2", "video/3gpp2"},
        {".3gp", "video/3gpp"},
        {".3gp2", "video/3gpp2"},
        {".3gpp", "video/3gpp"},
        {".7z", "application/x-7z-compressed"},
        {".aa", "audio/audible"},
        {".AAC", "audio/aac"},
        {".aaf", "application/octet-stream"},
        {".aax", "audio/vnd.audible.aax"},
        {".ac3", "audio/ac3"},
        {".aca", "application/octet-stream"},
        {".accda", "application/msaccess.addin"},
        {".accdb", "application/msaccess"},
        {".accdc", "application/msaccess.cab"},
        {".accde", "application/msaccess"},
        {".accdr", "application/msaccess.runtime"},
        {".accdt", "application/msaccess"},
        {".accdw", "application/msaccess.webapplication"},
        {".accft", "application/msaccess.ftemplate"},
        {".acx", "application/internet-property-stream"},
        {".AddIn", "text/xml"},
        {".ade", "application/msaccess"},
        {".adobebridge", "application/x-bridge-url"},
        {".adp", "application/msaccess"},
        {".ADT", "audio/vnd.dlna.adts"},
        {".ADTS", "audio/aac"},
        {".afm", "application/octet-stream"},
        {".ai", "application/postscript"},
        {".aif", "audio/x-aiff"},
        {".aifc", "audio/aiff"},
        {".aiff", "audio/aiff"},
        {".air", "application/vnd.adobe.air-application-installer-package+zip"},
        {".amc", "application/x-mpeg"},
        {".application", "application/x-ms-application"},
        {".art", "image/x-jg"},
        {".asa", "application/xml"},
        {".asax", "application/xml"},
        {".ascx", "application/xml"},
        {".asd", "application/octet-stream"},
        {".asf", "video/x-ms-asf"},
        {".ashx", "application/xml"},
        {".asi", "application/octet-stream"},
        {".asm", "text/plain"},
        {".asmx", "application/xml"},
        {".aspx", "application/xml"},
        {".asr", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".atom", "application/atom+xml"},
        {".au", "audio/basic"},
        {".avi", "video/x-msvideo"},
        {".axs", "application/olescript"},
        {".bas", "text/plain"},
        {".bcpio", "application/x-bcpio"},
        {".bin", "application/octet-stream"},
        {".bmp", "image/bmp"},
        {".c", "text/plain"},
        {".cab", "application/octet-stream"},
        {".caf", "audio/x-caf"},
        {".calx", "application/vnd.ms-office.calx"},
        {".cat", "application/vnd.ms-pki.seccat"},
        {".cc", "text/plain"},
        {".cd", "text/plain"},
        {".cdda", "audio/aiff"},
        {".cdf", "application/x-cdf"},
        {".cer", "application/x-x509-ca-cert"},
        {".chm", "application/octet-stream"},
        {".class", "application/x-java-applet"},
        {".clp", "application/x-msclip"},
        {".cmx", "image/x-cmx"},
        {".cnf", "text/plain"},
        {".cod", "image/cis-cod"},
        {".config", "application/xml"},
        {".contact", "text/x-ms-contact"},
        {".coverage", "application/xml"},
        {".cpio", "application/x-cpio"},
        {".cpp", "text/plain"},
        {".crd", "application/x-mscardfile"},
        {".crl", "application/pkix-crl"},
        {".crt", "application/x-x509-ca-cert"},
        {".cs", "text/plain"},
        {".csdproj", "text/plain"},
        {".csh", "application/x-csh"},
        {".csproj", "text/plain"},
        {".css", "text/css"},
        {".csv", "text/csv"},
        {".cur", "application/octet-stream"},
        {".cxx", "text/plain"},
        {".dat", "application/octet-stream"},
        {".datasource", "application/xml"},
        {".dbproj", "text/plain"},
        {".dcr", "application/x-director"},
        {".def", "text/plain"},
        {".deploy", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dgml", "application/xml"},
        {".dib", "image/bmp"},
        {".dif", "video/x-dv"},
        {".dir", "application/x-director"},
        {".disco", "text/xml"},
        {".dll", "application/x-msdownload"},
        {".dll.config", "text/xml"},
        {".dlm", "text/dlm"},
        {".doc", "application/msword"},
        {".docm", "application/vnd.ms-word.document.macroEnabled.12"},
        {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
        {".dot", "application/msword"},
        {".dotm", "application/vnd.ms-word.template.macroEnabled.12"},
        {".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
        {".dsp", "application/octet-stream"},
        {".dsw", "text/plain"},
        {".dtd", "text/xml"},
        {".dtsConfig", "text/xml"},
        {".dv", "video/x-dv"},
        {".dvi", "application/x-dvi"},
        {".dwf", "drawing/x-dwf"},
        {".dwp", "application/octet-stream"},
        {".dxr", "application/x-director"},
        {".eml", "message/rfc822"},
        {".emz", "application/octet-stream"},
        {".eot", "application/octet-stream"},
        {".eps", "application/postscript"},
        {".etl", "application/etl"},
        {".etx", "text/x-setext"},
        {".evy", "application/envoy"},
        {".exe", "application/octet-stream"},
        {".exe.config", "text/xml"},
        {".fdf", "application/vnd.fdf"},
        {".fif", "application/fractals"},
        {".filters", "Application/xml"},
        {".fla", "application/octet-stream"},
        {".flr", "x-world/x-vrml"},
        {".flv", "video/x-flv"},
        {".fsscript", "application/fsharp-script"},
        {".fsx", "application/fsharp-script"},
        {".generictest", "application/xml"},
        {".gif", "image/gif"},
        {".group", "text/x-ms-group"},
        {".gsm", "audio/x-gsm"},
        {".gtar", "application/x-gtar"},
        {".gz", "application/x-gzip"},
        {".h", "text/plain"},
        {".hdf", "application/x-hdf"},
        {".hdml", "text/x-hdml"},
        {".hhc", "application/x-oleobject"},
        {".hhk", "application/octet-stream"},
        {".hhp", "application/octet-stream"},
        {".hlp", "application/winhlp"},
        {".hpp", "text/plain"},
        {".hqx", "application/mac-binhex40"},
        {".hta", "application/hta"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".htt", "text/webviewhtml"},
        {".hxa", "application/xml"},
        {".hxc", "application/xml"},
        {".hxd", "application/octet-stream"},
        {".hxe", "application/xml"},
        {".hxf", "application/xml"},
        {".hxh", "application/octet-stream"},
        {".hxi", "application/octet-stream"},
        {".hxk", "application/xml"},
        {".hxq", "application/octet-stream"},
        {".hxr", "application/octet-stream"},
        {".hxs", "application/octet-stream"},
        {".hxt", "text/html"},
        {".hxv", "application/xml"},
        {".hxw", "application/octet-stream"},
        {".hxx", "text/plain"},
        {".i", "text/plain"},
        {".ico", "image/x-icon"},
        {".ics", "application/octet-stream"},
        {".idl", "text/plain"},
        {".ief", "image/ief"},
        {".iii", "application/x-iphone"},
        {".inc", "text/plain"},
        {".inf", "application/octet-stream"},
        {".inl", "text/plain"},
        {".ins", "application/x-internet-signup"},
        {".ipa", "application/x-itunes-ipa"},
        {".ipg", "application/x-itunes-ipg"},
        {".ipproj", "text/plain"},
        {".ipsw", "application/x-itunes-ipsw"},
        {".iqy", "text/x-ms-iqy"},
        {".isp", "application/x-internet-signup"},
        {".ite", "application/x-itunes-ite"},
        {".itlp", "application/x-itunes-itlp"},
        {".itms", "application/x-itunes-itms"},
        {".itpc", "application/x-itunes-itpc"},
        {".IVF", "video/x-ivf"},
        {".jar", "application/java-archive"},
        {".java", "application/octet-stream"},
        {".jck", "application/liquidmotion"},
        {".jcz", "application/liquidmotion"},
        {".jfif", "image/pjpeg"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpb", "application/octet-stream"},
        {".jpe", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".json", "application/json"},
        {".jsx", "text/jscript"},
        {".jsxbin", "text/plain"},
        {".latex", "application/x-latex"},
        {".library-ms", "application/windows-library+xml"},
        {".lit", "application/x-ms-reader"},
        {".loadtest", "application/xml"},
        {".lpk", "application/octet-stream"},
        {".lsf", "video/x-la-asf"},
        {".lst", "text/plain"},
        {".lsx", "video/x-la-asf"},
        {".lzh", "application/octet-stream"},
        {".m13", "application/x-msmediaview"},
        {".m14", "application/x-msmediaview"},
        {".m1v", "video/mpeg"},
        {".m2t", "video/vnd.dlna.mpeg-tts"},
        {".m2ts", "video/vnd.dlna.mpeg-tts"},
        {".m2v", "video/mpeg"},
        {".m3u", "audio/x-mpegurl"},
        {".m3u8", "audio/x-mpegurl"},
        {".m4a", "audio/m4a"},
        {".m4b", "audio/m4b"},
        {".m4p", "audio/m4p"},
        {".m4r", "audio/x-m4r"},
        {".m4v", "video/x-m4v"},
        {".mac", "image/x-macpaint"},
        {".mak", "text/plain"},
        {".man", "application/x-troff-man"},
        {".manifest", "application/x-ms-manifest"},
        {".map", "text/plain"},
        {".master", "application/xml"},
        {".mda", "application/msaccess"},
        {".mdb", "application/x-msaccess"},
        {".mde", "application/msaccess"},
        {".mdp", "application/octet-stream"},
        {".me", "application/x-troff-me"},
        {".mfp", "application/x-shockwave-flash"},
        {".mht", "message/rfc822"},
        {".mhtml", "message/rfc822"},
        {".mid", "audio/mid"},
        {".midi", "audio/mid"},
        {".mix", "application/octet-stream"},
        {".mk", "text/plain"},
        {".mmf", "application/x-smaf"},
        {".mno", "text/xml"},
        {".mny", "application/x-msmoney"},
        {".mod", "video/mpeg"},
        {".mov", "video/quicktime"},
        {".movie", "video/x-sgi-movie"},
        {".mp2", "video/mpeg"},
        {".mp2v", "video/mpeg"},
        {".mp3", "audio/mpeg"},
        {".mp4", "video/mp4"},
        {".mp4v", "video/mp4"},
        {".mpa", "video/mpeg"},
        {".mpe", "video/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpf", "application/vnd.ms-mediapackage"},
        {".mpg", "video/mpeg"},
        {".mpp", "application/vnd.ms-project"},
        {".mpv2", "video/mpeg"},
        {".mqv", "video/quicktime"},
        {".ms", "application/x-troff-ms"},
        {".msi", "application/octet-stream"},
        {".mso", "application/octet-stream"},
        {".mts", "video/vnd.dlna.mpeg-tts"},
        {".mtx", "application/xml"},
        {".mvb", "application/x-msmediaview"},
        {".mvc", "application/x-miva-compiled"},
        {".mxp", "application/x-mmxp"},
        {".nc", "application/x-netcdf"},
        {".nsc", "video/x-ms-asf"},
        {".nws", "message/rfc822"},
        {".ocx", "application/octet-stream"},
        {".oda", "application/oda"},
        {".odc", "text/x-ms-odc"},
        {".odh", "text/plain"},
        {".odl", "text/plain"},
        {".odp", "application/vnd.oasis.opendocument.presentation"},
        {".ods", "application/oleobject"},
        {".odt", "application/vnd.oasis.opendocument.text"},
        {".one", "application/onenote"},
        {".onea", "application/onenote"},
        {".onepkg", "application/onenote"},
        {".onetmp", "application/onenote"},
        {".onetoc", "application/onenote"},
        {".onetoc2", "application/onenote"},
        {".orderedtest", "application/xml"},
        {".osdx", "application/opensearchdescription+xml"},
        {".p10", "application/pkcs10"},
        {".p12", "application/x-pkcs12"},
        {".p7b", "application/x-pkcs7-certificates"},
        {".p7c", "application/pkcs7-mime"},
        {".p7m", "application/pkcs7-mime"},
        {".p7r", "application/x-pkcs7-certreqresp"},
        {".p7s", "application/pkcs7-signature"},
        {".pbm", "image/x-portable-bitmap"},
        {".pcast", "application/x-podcast"},
        {".pct", "image/pict"},
        {".pcx", "application/octet-stream"},
        {".pcz", "application/octet-stream"},
        {".pdf", "application/pdf"},
        {".pfb", "application/octet-stream"},
        {".pfm", "application/octet-stream"},
        {".pfx", "application/x-pkcs12"},
        {".pgm", "image/x-portable-graymap"},
        {".pic", "image/pict"},
        {".pict", "image/pict"},
        {".pkgdef", "text/plain"},
        {".pkgundef", "text/plain"},
        {".pko", "application/vnd.ms-pki.pko"},
        {".pls", "audio/scpls"},
        {".pma", "application/x-perfmon"},
        {".pmc", "application/x-perfmon"},
        {".pml", "application/x-perfmon"},
        {".pmr", "application/x-perfmon"},
        {".pmw", "application/x-perfmon"},
        {".png", "image/png"},
        {".pnm", "image/x-portable-anymap"},
        {".pnt", "image/x-macpaint"},
        {".pntg", "image/x-macpaint"},
        {".pnz", "image/png"},
        {".pot", "application/vnd.ms-powerpoint"},
        {".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12"},
        {".potx", "application/vnd.openxmlformats-officedocument.presentationml.template"},
        {".ppa", "application/vnd.ms-powerpoint"},
        {".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12"},
        {".ppm", "image/x-portable-pixmap"},
        {".pps", "application/vnd.ms-powerpoint"},
        {".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
        {".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
        {".ppt", "application/vnd.ms-powerpoint"},
        {".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
        {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
        {".prf", "application/pics-rules"},
        {".prm", "application/octet-stream"},
        {".prx", "application/octet-stream"},
        {".ps", "application/postscript"},
        {".psc1", "application/PowerShell"},
        {".psd", "application/octet-stream"},
        {".psess", "application/xml"},
        {".psm", "application/octet-stream"},
        {".psp", "application/octet-stream"},
        {".pub", "application/x-mspublisher"},
        {".pwz", "application/vnd.ms-powerpoint"},
        {".qht", "text/x-html-insertion"},
        {".qhtm", "text/x-html-insertion"},
        {".qt", "video/quicktime"},
        {".qti", "image/x-quicktime"},
        {".qtif", "image/x-quicktime"},
        {".qtl", "application/x-quicktimeplayer"},
        {".qxd", "application/octet-stream"},
        {".ra", "audio/x-pn-realaudio"},
        {".ram", "audio/x-pn-realaudio"},
        {".rar", "application/x-rar-compressed"},   // application/octet-stream
        {".ras", "image/x-cmu-raster"},
        {".rat", "application/rat-file"},
        {".rc", "text/plain"},
        {".rc2", "text/plain"},
        {".rct", "text/plain"},
        {".rdlc", "application/xml"},
        {".resx", "application/xml"},
        {".rf", "image/vnd.rn-realflash"},
        {".rgb", "image/x-rgb"},
        {".rgs", "text/plain"},
        {".rm", "application/vnd.rn-realmedia"},
        {".rmi", "audio/mid"},
        {".rmp", "application/vnd.rn-rn_music_package"},
        {".roff", "application/x-troff"},
        {".rpm", "audio/x-pn-realaudio-plugin"},
        {".rqy", "text/x-ms-rqy"},
        {".rtf", "application/rtf"},
        {".rtx", "text/richtext"},
        {".ruleset", "application/xml"},
        {".s", "text/plain"},
        {".safariextz", "application/x-safari-safariextz"},
        {".scd", "application/x-msschedule"},
        {".sct", "text/scriptlet"},
        {".sd2", "audio/x-sd2"},
        {".sdp", "application/sdp"},
        {".sea", "application/octet-stream"},
        {".searchConnector-ms", "application/windows-search-connector+xml"},
        {".setpay", "application/set-payment-initiation"},
        {".setreg", "application/set-registration-initiation"},
        {".settings", "application/xml"},
        {".sgimb", "application/x-sgimb"},
        {".sgml", "text/sgml"},
        {".sh", "application/x-sh"},
        {".shar", "application/x-shar"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".sitemap", "application/xml"},
        {".skin", "application/xml"},
        {".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12"},
        {".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide"},
        {".slk", "application/vnd.ms-excel"},
        {".sln", "text/plain"},
        {".slupkg-ms", "application/x-ms-license"},
        {".smd", "audio/x-smd"},
        {".smi", "application/octet-stream"},
        {".smx", "audio/x-smd"},
        {".smz", "audio/x-smd"},
        {".snd", "audio/basic"},
        {".snippet", "application/xml"},
        {".snp", "application/octet-stream"},
        {".sol", "text/plain"},
        {".sor", "text/plain"},
        {".spc", "application/x-pkcs7-certificates"},
        {".spl", "application/futuresplash"},
        {".src", "application/x-wais-source"},
        {".srf", "text/plain"},
        {".SSISDeploymentManifest", "text/xml"},
        {".ssm", "application/streamingmedia"},
        {".sst", "application/vnd.ms-pki.certstore"},
        {".stl", "application/vnd.ms-pki.stl"},
        {".sv4cpio", "application/x-sv4cpio"},
        {".sv4crc", "application/x-sv4crc"},
        {".svc", "application/xml"},
        {".swf", "application/x-shockwave-flash"},
        {".t", "application/x-troff"},
        {".tar", "application/x-tar"},
        {".tcl", "application/x-tcl"},
        {".testrunconfig", "application/xml"},
        {".testsettings", "application/xml"},
        {".tex", "application/x-tex"},
        {".texi", "application/x-texinfo"},
        {".texinfo", "application/x-texinfo"},
        {".tgz", "application/x-compressed"},
        {".thmx", "application/vnd.ms-officetheme"},
        {".thn", "application/octet-stream"},
        {".tif", "image/tiff"},
        {".tiff", "image/tiff"},
        {".tlh", "text/plain"},
        {".tli", "text/plain"},
        {".toc", "application/octet-stream"},
        {".tr", "application/x-troff"},
        {".trm", "application/x-msterminal"},
        {".trx", "application/xml"},
        {".ts", "video/vnd.dlna.mpeg-tts"},
        {".tsv", "text/tab-separated-values"},
        {".ttf", "application/octet-stream"},
        {".tts", "video/vnd.dlna.mpeg-tts"},
        {".txt", "text/plain"},
        {".u32", "application/octet-stream"},
        {".uls", "text/iuls"},
        {".user", "text/plain"},
        {".ustar", "application/x-ustar"},
        {".vb", "text/plain"},
        {".vbdproj", "text/plain"},
        {".vbk", "video/mpeg"},
        {".vbproj", "text/plain"},
        {".vbs", "text/vbscript"},
        {".vcf", "text/x-vcard"},
        {".vcproj", "Application/xml"},
        {".vcs", "text/plain"},
        {".vcxproj", "Application/xml"},
        {".vddproj", "text/plain"},
        {".vdp", "text/plain"},
        {".vdproj", "text/plain"},
        {".vdx", "application/vnd.ms-visio.viewer"},
        {".vml", "text/xml"},
        {".vscontent", "application/xml"},
        {".vsct", "text/xml"},
        {".vsd", "application/vnd.visio"},
        {".vsi", "application/ms-vsi"},
        {".vsix", "application/vsix"},
        {".vsixlangpack", "text/xml"},
        {".vsixmanifest", "text/xml"},
        {".vsmdi", "application/xml"},
        {".vspscc", "text/plain"},
        {".vss", "application/vnd.visio"},
        {".vsscc", "text/plain"},
        {".vssettings", "text/xml"},
        {".vssscc", "text/plain"},
        {".vst", "application/vnd.visio"},
        {".vstemplate", "text/xml"},
        {".vsto", "application/x-ms-vsto"},
        {".vsw", "application/vnd.visio"},
        {".vsx", "application/vnd.visio"},
        {".vtx", "application/vnd.visio"},
        {".wav", "audio/wav"},
        {".wave", "audio/wav"},
        {".wax", "audio/x-ms-wax"},
        {".wbk", "application/msword"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wcm", "application/vnd.ms-works"},
        {".wdb", "application/vnd.ms-works"},
        {".wdp", "image/vnd.ms-photo"},
        {".webarchive", "application/x-safari-webarchive"},
        {".webtest", "application/xml"},
        {".wiq", "application/xml"},
        {".wiz", "application/msword"},
        {".wks", "application/vnd.ms-works"},
        {".WLMP", "application/wlmoviemaker"},
        {".wlpginstall", "application/x-wlpg-detect"},
        {".wlpginstall3", "application/x-wlpg3-detect"},
        {".wm", "video/x-ms-wm"},
        {".wma", "audio/x-ms-wma"},
        {".wmd", "application/x-ms-wmd"},
        {".wmf", "application/x-msmetafile"},
        {".wml", "text/vnd.wap.wml"},
        {".wmlc", "application/vnd.wap.wmlc"},
        {".wmls", "text/vnd.wap.wmlscript"},
        {".wmlsc", "application/vnd.wap.wmlscriptc"},
        {".wmp", "video/x-ms-wmp"},
        {".wmv", "video/x-ms-wmv"},
        {".wmx", "video/x-ms-wmx"},
        {".wmz", "application/x-ms-wmz"},
        {".wpl", "application/vnd.ms-wpl"},
        {".wps", "application/vnd.ms-works"},
        {".wri", "application/x-mswrite"},
        {".wrl", "x-world/x-vrml"},
        {".wrz", "x-world/x-vrml"},
        {".wsc", "text/scriptlet"},
        {".wsdl", "text/xml"},
        {".wvx", "video/x-ms-wvx"},
        {".x", "application/directx"},
        {".xaf", "x-world/x-vrml"},
        {".xaml", "application/xaml+xml"},
        {".xap", "application/x-silverlight-app"},
        {".xbap", "application/x-ms-xbap"},
        {".xbm", "image/x-xbitmap"},
        {".xdr", "text/plain"},
        {".xht", "application/xhtml+xml"},
        {".xhtml", "application/xhtml+xml"},
        {".xla", "application/vnd.ms-excel"},
        {".xlam", "application/vnd.ms-excel.addin.macroEnabled.12"},
        {".xlc", "application/vnd.ms-excel"},
        {".xld", "application/vnd.ms-excel"},
        {".xlk", "application/vnd.ms-excel"},
        {".xll", "application/vnd.ms-excel"},
        {".xlm", "application/vnd.ms-excel"},
        {".xls", "application/vnd.ms-excel"},
        {".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
        {".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"},
        {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
        {".xlt", "application/vnd.ms-excel"},
        {".xltm", "application/vnd.ms-excel.template.macroEnabled.12"},
        {".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
        {".xlw", "application/vnd.ms-excel"},
        {".xml", "text/xml"},
        {".xmta", "application/xml"},
        {".xof", "x-world/x-vrml"},
        {".XOML", "text/plain"},
        {".xpm", "image/x-xpixmap"},
        {".xps", "application/vnd.ms-xpsdocument"},
        {".xrm-ms", "text/xml"},
        {".xsc", "application/xml"},
        {".xsd", "text/xml"},
        {".xsf", "text/xml"},
        {".xsl", "text/xml"},
        {".xslt", "text/xml"},
        {".xsn", "application/octet-stream"},
        {".xss", "application/xml"},
        {".xtp", "application/octet-stream"},
        {".xwd", "image/x-xwindowdump"},
        {".z", "application/x-compress"},
        {".zip", "application/x-zip-compressed"},
        #endregion

        };

        // 根据文件扩展名获得 MIME 类型
        // http://stackoverflow.com/questions/1029740/get-mime-type-from-filename-extension
        public static string GetMimeTypeByFileExtension(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException("extension");
            }

            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            string mime;

            return _mappings.TryGetValue(extension, out mime) ? mime : "application/octet-stream";
        }

        // 去除路径第一字符的 '/'
        public static string RemoveRootSlash(string strPath)
        {
            if (string.IsNullOrEmpty(strPath) == true)
                return strPath;
            if (strPath[0] == '\\' || strPath[0] == '/')
                return strPath.Substring(1);
            return strPath;
        }

        // 获得一个目录下的全部文件的尺寸总和。包括子目录中的
        public static long GetAllFileSize(string strDataDir, ref long count)
        {
            long size = 0;
            DirectoryInfo di = new DirectoryInfo(strDataDir);
            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
                count++;
            }

            // 处理下级目录，递归
            DirectoryInfo[] dis = di.GetDirectories();
            foreach (DirectoryInfo subdir in dis)
            {
                size += GetAllFileSize(subdir.FullName, ref count);
            }

            return size;
        }

        // get clickonce shortcut filename
        // parameters:
        //      strApplicationName  "DigitalPlatform/dp2 V2/dp2内务 V2"
        public static string GetShortcutFilePath(string strApplicationName)
        {
            // string publisherName = "Publisher Name";
            // string applicationName = "Application Name";
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), strApplicationName) + ".appref-ms";
        }

        public static void DeleteDirectory(string strDirPath)
        {
            try
            {
                Directory.Delete(strDirPath, true);
            }
            catch (DirectoryNotFoundException)
            {
                // 不存在就算了
            }
        }

        // 移除文件目录内所有文件的 ReadOnly 属性
        public static void RemoveReadOnlyAttr(string strSourceDir)
        {
            string strCurrentDir = Directory.GetCurrentDirectory();

            DirectoryInfo di = new DirectoryInfo(strSourceDir);

            FileSystemInfo[] subs = di.GetFileSystemInfos();

            for (int i = 0; i < subs.Length; i++)
            {

                // 递归
                if ((subs[i].Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    RemoveReadOnlyAttr(subs[i].FullName);
                }
                else
                    File.SetAttributes(subs[i].FullName, FileAttributes.Normal);

            }
        }

        public delegate bool FileNameFilterProc(FileSystemInfo fi);

        // 拷贝目录
        // return:
        //      -1  出错
        //      >=0 复制的文件总数
        public static int CopyDirectory(string strSourceDir,
            string strTargetDir,
            FileNameFilterProc filter_proc,
            out string strError)
        {
            strError = "";

            int nCount = 0;
            try
            {
                DirectoryInfo di = new DirectoryInfo(strSourceDir);

                if (di.Exists == false)
                {
                    strError = "源目录 '" + strSourceDir + "' 不存在...";
                    return -1;
                }

#if NO
                if (bDeleteTargetBeforeCopy == true)
                {
                    if (Directory.Exists(strTargetDir) == true)
                        Directory.Delete(strTargetDir, true);
                }
#endif

                CreateDirIfNeed(strTargetDir);

                FileSystemInfo[] subs = di.GetFileSystemInfos();

                foreach (FileSystemInfo sub in subs)
                {
                    if (filter_proc != null && filter_proc(sub) == false)
                        continue;

                    // 复制目录
                    if ((sub.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        int nRet = CopyDirectory(sub.FullName,
                            Path.Combine(strTargetDir, sub.Name),
                            filter_proc,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        nCount += nRet;
                        continue;
                    }
                    // 复制文件
                    string source = sub.FullName;
                    string target = Path.Combine(strTargetDir, sub.Name);
                    // 如果目标文件已经存在，并且修后修改时间相同，则不复制了
                    if (File.Exists(target) == true && File.GetLastWriteTimeUtc(source) == File.GetLastWriteTimeUtc(target))
                        continue;

                    // File.Copy(source, target, true);

                    // 拷贝文件，最多重试 10 次
                    for (int nRedoCount = 0; ; nRedoCount++)
                    {
                        try
                        {
                            File.Copy(source, target, true);
                        }
                        catch (Exception ex)
                        {
                            if (nRedoCount < 10)
                            {
                                Thread.Sleep(100);
                                continue;
                            }
                            else
                            {
                                string strText = "source '" + source + "' lastmodified = '" + File.GetLastWriteTimeUtc(source).ToString() + "'; "
                                    + "target '" + target + "' lastmodified = '" + File.GetLastWriteTimeUtc(target).ToString() + "'";
                                throw new Exception(strText, ex);
                            }
                        }
                        Debug.Assert(File.GetLastWriteTimeUtc(source) == File.GetLastWriteTimeUtc(target), "源文件和目标文件复制完成后，最后修改时间不同");
                        break;
                    }

                    // 把最后修改时间设置为和 source 一样
                    File.SetLastWriteTimeUtc(target, File.GetLastWriteTimeUtc(source));
                    nCount++;
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            return nCount;
        }

        // 拷贝目录
        // 遇到有同名文件会覆盖
        public static int CopyDirectory(string strSourceDir,
            string strTargetDir,
            bool bDeleteTargetBeforeCopy,
            out string strError)
        {
            strError = "";

            try
            {
                DirectoryInfo di = new DirectoryInfo(strSourceDir);

                if (di.Exists == false)
                {
                    strError = "源目录 '" + strSourceDir + "' 不存在...";
                    return -1;
                }

                if (bDeleteTargetBeforeCopy == true)
                {
                    if (Directory.Exists(strTargetDir) == true)
                        Directory.Delete(strTargetDir, true);
                }

                CreateDirIfNeed(strTargetDir);

                FileSystemInfo[] subs = di.GetFileSystemInfos();

                for (int i = 0; i < subs.Length; i++)
                {
                    // 复制目录
                    if ((subs[i].Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        int nRet = CopyDirectory(subs[i].FullName,
                            Path.Combine(strTargetDir, subs[i].Name),
                            bDeleteTargetBeforeCopy,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        continue;
                    }
                    // 复制文件
                    File.Copy(subs[i].FullName, 
                        Path.Combine(strTargetDir, subs[i].Name),
                        true);
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            return 0;
        }

        // 如果目录不存在则创建之
        // return:
        //      false   已经存在
        //      true    刚刚新创建
        public static bool CreateDirIfNeed(string strDir)
        {
            DirectoryInfo di = new DirectoryInfo(strDir);
            if (di.Exists == false)
            {
                di.Create();
                return true;
            }

            return false;
        }

        // 删除一个目录内的所有文件和目录
        // 可能会抛出异常
        public static void ClearDir(string strDir)
        {
            DirectoryInfo di = new DirectoryInfo(strDir);
            if (di.Exists == false)
                return;

            // 删除所有的下级目录
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach (DirectoryInfo childDir in dirs)
            {
                Directory.Delete(childDir.FullName, true);
            }

            // 删除所有文件
            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis)
            {
                File.Delete(fi.FullName);
            }
        }

        // 删除一个目录内的所有文件和目录
        // 不会抛出异常
        public static bool TryClearDir(string strDir)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(strDir);
                if (di.Exists == false)
                    return true;

                // 删除所有的下级目录
                DirectoryInfo[] dirs = di.GetDirectories();
                foreach (DirectoryInfo childDir in dirs)
                {
                    Directory.Delete(childDir.FullName, true);
                }

                // 删除所有文件
                FileInfo[] fis = di.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    File.Delete(fi.FullName);
                }

                return true;
            }
            catch (Exception /*ex*/)
            {
                return false;
            }
        }

        // 是否为纯文件名？
        public static bool IsPureFileName(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return false;
            if (strText.IndexOfAny(new char[] { '/', '\\', ':' }) == -1)
                return true;
            return false;
        }

        // 获得纯文件名部分
        public static string PureName(string strPath)
        {
            // 2012/11/30
            if (string.IsNullOrEmpty(strPath) == true)
                return strPath;

            string sResult = "";
            sResult = strPath;
            sResult = sResult.Replace("/", "\\");
            if (sResult.Length > 0)
            {
                if (sResult[sResult.Length - 1] == '\\')
                    sResult = sResult.Substring(0, sResult.Length - 1);
            }
            int nRet = sResult.LastIndexOf("\\");
            if (nRet != -1)
                sResult = sResult.Substring(nRet + 1);

            return sResult;
        }

        public static string PathPart(string strPath)
        {
            string sResult = "";
            sResult = strPath;
            sResult = sResult.Replace("/", "\\");
            if (sResult.Length > 0)
            {
                if (sResult[sResult.Length - 1] == '\\')
                    sResult = sResult.Substring(0, sResult.Length - 1);
            }
            int nRet = sResult.LastIndexOf("\\");
            if (nRet != -1)
                sResult = sResult.Substring(0, nRet);
            else
                sResult = "";

            return sResult;
        }

        public static string MergePath(string s1, string s2)
        {
            string sResult = "";

            if (s1 != null)
            {
                sResult = s1;
                sResult = sResult.Replace("/", "\\");
                if (sResult != "")
                {
                    if (sResult[sResult.Length - 1] != '\\')
                        sResult += "\\";
                }
                else
                {
                    sResult += "\\";
                }
            }
            if (s2 != null)
            {
                s2 = s2.Replace("/", "\\");
                if (s2 != "")
                {
                    if (s2[0] == '\\')
                        s2 = s2.Remove(0, 1);
                    sResult += s2;
                }

            }

            return sResult;
        }

        // 正规化目录路径名。把所有字符'/'替换为'\'，并且为末尾确保有字符'\'
        public static string CanonicalizeDirectoryPath(string strPath)
        {
            if (string.IsNullOrEmpty(strPath) == true)
                return "";

            strPath = strPath.Replace('/', '\\');

            if (strPath[strPath.Length - 1] != '\\')
                strPath += "\\";

            return strPath;
        }

        // 测试strPath1是否为strPath2的下级目录或文件
        //	strPath1正好等于strPath2的情况也返回true
        public static bool IsChildOrEqual(string strPath1, string strPath2)
        {
            FileSystemInfo fi1 = new DirectoryInfo(strPath1);

            FileSystemInfo fi2 = new DirectoryInfo(strPath2);

            string strNewPath1 = fi1.FullName;
            string strNewPath2 = fi2.FullName;

            if (strNewPath1.Length != 0)
            {
                if (strNewPath1[strNewPath1.Length - 1] != '\\')
                    strNewPath1 += "\\";
            }
            if (strNewPath2.Length != 0)
            {
                if (strNewPath2[strNewPath2.Length - 1] != '\\')
                    strNewPath2 += "\\";
            }

            // 路径1字符串长度比路径2短，说明路径1已不可能是儿子，因为儿子的路径会更长
            if (strNewPath1.Length < strNewPath2.Length)
                return false;


            // 截取路径1中前面一段进行比较
            string strPart = strNewPath1.Substring(0, strNewPath2.Length);
            strPart = strPart.ToUpper();
            strNewPath2 = strNewPath2.ToUpper();

            if (strPart != strNewPath2)
                return false;

            return true;
        }

        // 测试strPath1是否和strPath2为同一文件或目录
        public static bool IsEqual(string strPath1, string strPath2)
        {
            if (String.IsNullOrEmpty(strPath1) == true
                && String.IsNullOrEmpty(strPath2) == true)
                return true;

            if (String.IsNullOrEmpty(strPath1) == true)
                return false;

            if (String.IsNullOrEmpty(strPath2) == true)
                return false;

            if (strPath1 == strPath2)
                return true;

            // TODO: new DirecotryInfo() 对一个文件操作时候会怎样？会抛出异常么? 需要测试一下 2016/11/6
            FileSystemInfo fi1 = new DirectoryInfo(strPath1);
            FileSystemInfo fi2 = new DirectoryInfo(strPath2);

            string strNewPath1 = fi1.FullName.ToUpper();
            string strNewPath2 = fi2.FullName.ToUpper();

            if (strNewPath1.Length != 0)
            {
                if (strNewPath1[strNewPath1.Length - 1] != '\\')
                    strNewPath1 += "\\";
            }
            if (strNewPath2.Length != 0)
            {
                if (strNewPath2[strNewPath2.Length - 1] != '\\')
                    strNewPath2 += "\\";
            }

            if (strNewPath1.Length != strNewPath2.Length)
                return false;

            if (strNewPath1 == strNewPath2)
                return true;

            return false;
        }

        // 测试strPath1是否和strPath2为同一文件或目录
        public static bool IsEqualEx(string strPath1, string strPath2)
        {
            string strNewPath1 = strPath1;
            string strNewPath2 = strPath2;

            if (strNewPath1.Length != 0)
            {
                if (strNewPath1[strNewPath1.Length - 1] != '\\')
                    strNewPath1 += "\\";
            }
            if (strNewPath2.Length != 0)
            {
                if (strNewPath2[strNewPath2.Length - 1] != '\\')
                    strNewPath2 += "\\";
            }

            if (strNewPath1.Length != strNewPath2.Length)
                return false;

            strNewPath1 = strNewPath1.ToUpper();
            strNewPath2 = strNewPath2.ToUpper();

            if (strNewPath1 == strNewPath2)
                return true;

            return false;
        }

        public static string EnsureTailBackslash(string strPath)
        {
            if (strPath == "")
                return "\\";

            string sResult = "";

            sResult = strPath.Replace("/", "\\");

            if (sResult[sResult.Length - 1] != '\\')
                sResult += "\\";

            return sResult;
        }

        // 末尾是否有'\'。如果具备，表示这是一个目录路径。
        public static bool HasTailBackslash(string strPath)
        {
            if (strPath == "")
                return true;	// 理解为'\'

            string sResult = "";

            sResult = strPath.Replace("/", "\\");

            if (sResult[sResult.Length - 1] == '\\')
                return true;

            return false;
        }


        // 测试strPathChild是否为strPathParent的下级目录或文件
        // 如果是下级，则将strPathChild中和strPathParent重合的部分替换为
        // strMacro中的宏字符串返回在strResult中，并且函数返回true。
        // 否则函数返回false，strResult虽返回内容，但不替换。
        //	strPath1正好等于strPath2的情况也返回true
        // 
        // Exception:
        //	System.NotSupportedException
        // Testing:
        //	在testIO.exe中
        public static bool MacroPathPart(string strPathChild,
            string strPathParent,
            string strMacro,
            out string strResult)
        {
            strResult = strPathChild;

            FileSystemInfo fiChild = new DirectoryInfo(strPathChild);

            FileSystemInfo fiParent = new DirectoryInfo(strPathParent);

            string strNewPathChild = fiChild.FullName;
            string strNewPathParent = fiParent.FullName;

            if (strNewPathChild.Length != 0)
            {
                if (strNewPathChild[strNewPathChild.Length - 1] != '\\')
                    strNewPathChild += "\\";
            }
            if (strNewPathParent.Length != 0)
            {
                if (strNewPathParent[strNewPathParent.Length - 1] != '\\')
                    strNewPathParent += "\\";
            }

            // 路径1字符串长度比路径2短，说明路径1已不可能是儿子，因为儿子的路径会更长
            if (strNewPathChild.Length < strNewPathParent.Length)
                return false;


            // 截取路径1中前面一段进行比较
            string strPart = strNewPathChild.Substring(0, strNewPathParent.Length);
            strPart = strPart.ToUpper();
            strNewPathParent = strNewPathParent.ToUpper();

            if (strPart != strNewPathParent)
                return false;

            strResult = strMacro + "\\" + fiChild.FullName.Substring(strNewPathParent.Length);
            // fiChild.FullName是尾部未加'\'以前的形式

            return true;
        }

        // 将路径中的%%宏部分替换为具体内容
        // parameters:
        //		macroTable	宏名和内容的对照表
        //		bThrowMacroNotFoundException	是否抛出MacroNotFoundException异常
        // Exception:
        //	MacroNotFoundException
        //	MacroNameException	函数NextMacro()可能抛出
        // Testing:
        //	在testIO.exe中
        public static string UnMacroPath(Hashtable macroTable,
            string strPath,
            bool bThrowMacroNotFoundException)
        {
            int nCurPos = 0;
            string strPart = "";

            string strResult = "";

            for (; ; )
            {
                strPart = NextMacro(strPath, ref nCurPos);
                if (strPart == "")
                    break;

                if (strPart[0] == '%')
                {
                    string strValue = (string)macroTable[strPart];
                    if (strValue == null)
                    {
                        if (bThrowMacroNotFoundException)
                        {
                            MacroNotFoundException ex = new MacroNotFoundException("macro " + strPart + " not found in macroTable");
                            throw ex;
                        }
                        else
                        {
                            // 将没有找到的宏放回结果中
                            strResult += strPart;
                            continue;
                        }
                    }

                    strResult += strValue;
                }
                else
                {
                    strResult += strPart;
                }
            }

            return strResult;
        }

        // 本函数为UnMacroPath()的服务函数
        // 顺次得到下一个部分
        // nCurPos在第一次调用前其值必须设置为0，然后，调主不要改变其值
        // Exception:
        //	MacroNameException
        static string NextMacro(string strText,
            ref int nCurPos)
        {
            if (nCurPos >= strText.Length)
                return "";

            string strResult = "";
            bool bMacro = false;	// 本次是否在macro上

            if (strText[nCurPos] == '%')
                bMacro = true;

            int nRet = -1;

            if (bMacro == false)
                nRet = strText.IndexOf("%", nCurPos);
            else
                nRet = strText.IndexOf("%", nCurPos + 1);

            if (nRet == -1)
            {
                strResult = strText.Substring(nCurPos);
                nCurPos = strText.Length;
                if (bMacro == true)
                {
                    // 这是异常情况，表明%只有头部一个
                    throw (new MacroNameException("macro " + strResult + " format error"));
                }
                return strResult;
            }

            if (bMacro == true)
            {
                strResult = strText.Substring(nCurPos, nRet - nCurPos + 1);
                nCurPos = nRet + 1;
                return strResult;
            }
            else
            {
                Debug.Assert(strText[nRet] == '%', "当前位置不是%，异常");
                strResult = strText.Substring(nCurPos, nRet - nCurPos);
                nCurPos = nRet;
                return strResult;
            }
        }

        public static string GetShortFileName(string strFileName)
        {
            StringBuilder shortPath = new StringBuilder(300);
            int nRet = API.GetShortPathName(
                strFileName,
                shortPath,
                shortPath.Capacity);
            if (nRet == 0 || nRet >= 300)
            {
                // MessageBox.Show("file '" +strFileName+ "' get short error");
                // return strFileName;
                throw (new Exception("GetShortFileName error"));
            }

            return shortPath.ToString();
        }
    }
}
