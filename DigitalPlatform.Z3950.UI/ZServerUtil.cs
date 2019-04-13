using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DigitalPlatform.Z3950.UI
{
    /// <summary>
    /// 关于 Z39.50 服务器信息的一些实用函数
    /// </summary>
    public class ZServerUtil
    {
        public NormalResult GetTarget(
            XmlElement xmlServerNode,
            out TargetInfo targetinfo)
        {
            targetinfo = null;

            targetinfo = new TargetInfo();

            List<string> dbnames = GetDatabaseNames(xmlServerNode);

            targetinfo.HostName = xmlServerNode.GetAttribute("addr");
            string strPort = xmlServerNode.GetAttribute("port");
            if (String.IsNullOrEmpty(strPort) == false)
                targetinfo.Port = Convert.ToInt32(strPort);

            targetinfo.DbNames = dbnames.ToArray();
            targetinfo.UserName = xmlServerNode.GetAttribute("username");

            // password
            string strPassword = xmlServerNode.GetAttribute("password");
            targetinfo.Password = ZServerPropertyForm.GetPassword(strPassword);

            targetinfo.GroupID = xmlServerNode.GetAttribute("groupid");
            string strAuthenticationMethod = xmlServerNode.GetAttribute("authmethod");
            if (String.IsNullOrEmpty(strAuthenticationMethod) == false)
                targetinfo.AuthenticationMethod = Convert.ToInt32(strAuthenticationMethod);

            targetinfo.ConvertEACC = ZServerPropertyForm.GetBool(
                xmlServerNode.GetAttribute("converteacc"));
            targetinfo.FirstFull = ZServerPropertyForm.GetBool(
                xmlServerNode.GetAttribute("firstfull"));
            targetinfo.DetectMarcSyntax = ZServerPropertyForm.GetBool(
                xmlServerNode.GetAttribute("detectmarcsyntax"));

            targetinfo.IgnoreReferenceID = ZServerPropertyForm.GetBool(
                xmlServerNode.GetAttribute("ignorereferenceid"));

            // 对ISBN的预处理
            targetinfo.IsbnForce13 = ZServerPropertyForm.GetBool(
                xmlServerNode.GetAttribute("isbn_force13"));
            targetinfo.IsbnForce10 = ZServerPropertyForm.GetBool(
                xmlServerNode.GetAttribute("isbn_force10"));
            targetinfo.IsbnAddHyphen = ZServerPropertyForm.GetBool(
                xmlServerNode.GetAttribute("isbn_addhyphen"));
            targetinfo.IsbnRemoveHyphen = ZServerPropertyForm.GetBool(
                xmlServerNode.GetAttribute("isbn_removehyphen"));
            targetinfo.IsbnWild = ZServerPropertyForm.GetBool(
                xmlServerNode.GetAttribute("isbn_wild"));

            targetinfo.IssnForce8 = ZServerPropertyForm.GetBool(
                xmlServerNode.GetAttribute("issn_force8"));

            string strPresentPerBatchCount = xmlServerNode.GetAttribute("recsperbatch");

            if (String.IsNullOrEmpty(strPresentPerBatchCount) == false)
                targetinfo.PresentPerBatchCount = Convert.ToInt32(strPresentPerBatchCount);

            // 缺省编码方式
            string strDefaultEncodingName = xmlServerNode.GetAttribute("defaultEncoding");

            if (String.IsNullOrEmpty(strDefaultEncodingName) == false)
            {
                try
                {
                    // 单独处理MARC-8 Encoding
                    if (strDefaultEncodingName.ToLower() == "eacc"
                        || strDefaultEncodingName.ToLower() == "marc-8")
                    {
                        if (this.Marc8Encoding == null)
                        {
                            strError = "尚未初始化this.EaccEncoding成员";
                            return -1;
                        }
                        targetinfo.DefaultRecordsEncoding = this.Marc8Encoding;
                    }
                    else
                        targetinfo.DefaultRecordsEncoding = Encoding.GetEncoding(strDefaultEncodingName);
                }
                catch
                {
                    targetinfo.DefaultRecordsEncoding = Encoding.GetEncoding(936);
                }
            }

            // 检索词编码方式
            string strQueryTermEncodingName = xmlServerNode.GetAttribute("queryTermEncoding");

            if (String.IsNullOrEmpty(strQueryTermEncodingName) == false)
            {
                try
                {
                    targetinfo.DefaultQueryTermEncoding = Encoding.GetEncoding(strQueryTermEncodingName);
                }
                catch
                {
                    targetinfo.DefaultQueryTermEncoding = Encoding.GetEncoding(936);
                }
            }

            string strDefaultMarcSyntax = xmlServerNode.GetAttribute("defaultMarcSyntaxOID");
            // strDefaultMarcSyntax = strDefaultMarcSyntax;    // 可以有--部分

            if (String.IsNullOrEmpty(strDefaultMarcSyntax) == false)
                targetinfo.PreferredRecordSyntax = strDefaultMarcSyntax;

            //
            string strDefaultElementSetName = xmlServerNode.GetAttribute("defaultElementSetName");
            // strDefaultElementSetName = strDefaultElementSetName;    // 可以有--部分

            if (String.IsNullOrEmpty(strDefaultElementSetName) == false)
                targetinfo.DefaultElementSetName = strDefaultElementSetName;

            // 格式和编码之间的绑定信息
            string strBindingDef = xmlServerNode.GetAttribute("recordSyntaxAndEncodingBinding");
            targetinfo.Bindings = new RecordSyntaxAndEncodingBindingCollection();
            if (String.IsNullOrEmpty(strBindingDef) == false)
                targetinfo.Bindings.Load(strBindingDef);

            // charset nego
            targetinfo.CharNegoUTF8 = ZServerPropertyForm.GetBool(
                xmlServerNode.GetAttribute("charNegoUtf8"));
            targetinfo.CharNegoRecordsUTF8 = ZServerPropertyForm.GetBool(
                xmlServerNode.GetAttribute("charNego_recordsInSeletedCharsets"));

            targetinfo.UnionCatalogBindingDp2ServerName =
                xmlServerNode.GetAttribute("unionCatalog_bindingDp2ServerName");

            targetinfo.UnionCatalogBindingUcServerUrl =
                xmlServerNode.GetAttribute("unionCatalog_bindingUcServerUrl");

            return new NormalResult();
        }

        public static List<string> GetDatabaseNames(XmlElement server)
        {
            List<string> names = new List<string>();
            var databases = server.SelectNodes("database");
            foreach (XmlElement database in databases)
            {
                names.Add(database.GetAttribute("name"));
            }
            return names;
        }

        public static string GetDatabaseList(XmlElement server)
        {
            return StringUtil.MakePathList(GetDatabaseNames(server));
        }
    }
}
