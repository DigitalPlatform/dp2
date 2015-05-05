// dp2catalog USMARC 西文期刊 编目自动创建数据C#脚本程序
// 最后修改时间 2011/8/21

using System;
using System.Windows.Forms;
using System.IO;
using System.Text;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.IO;
using DigitalPlatform.GcatClient;
using DigitalPlatform.Text;
using DigitalPlatform.Script;

using dp2Catalog;

public class MyHost : MarcDetailHost
{

    public void CreateMenu(object sender, GenerateDataEventArgs e)
    {
        ScriptActionCollection actions = new ScriptActionCollection();

        this.ScriptActions = actions;
    }

    // 获得模板定义
    void GetTemplateDef(object sender, GetTemplateDefEventArgs e)
    {
        if (e.FieldName == "008")
        {
            if (this.DetailForm.MarcEditor.MarcDefDom == null)
            {
                e.ErrorInfo = "MarcEditor中的MarcDefDom尚未准备好...";
                return;
            }

            if (this.DetailForm.MarcEditor.Record.Fields.Count == 0)
            {
                e.ErrorInfo = "MarcEditor中没有头标区";
                return;
            }

            // 观察头标区
            Field header = this.DetailForm.MarcEditor.Record.Fields[0];
            if (header.Value.Length < 24)
            {
                e.ErrorInfo = "MarcEditor中头标区不是24字符";
                return;
            }

            string strType = "";
            // http://www.loc.gov/marc/bibliographic/bd008b.html
            // Books definition of field 008/18-34 is used when Leader/06 (Type of record) contains code a (Language material) or t (Manuscript language material) and Leader/07 (Bibliographic level) contains code a (Monographic component part), c (Collection), d (Subunit), or m (Monograph). 
            if ("at".IndexOf(header.Value[6]) != -1
                && "acdm".IndexOf(header.Value[7]) != -1)
                strType = "books";
            // http://www.loc.gov/marc/bibliographic/bd008c.html
            // Computer files definition of field 008/18-34 is used when Leader/06 (Type of record) contains code m.
            else if ("m".IndexOf(header.Value[6]) != -1)
                strType = "computer_files";
            // http://www.loc.gov/marc/bibliographic/bd008p.html
            // Maps definition of field 008/18-34 is used when Leader/06 (Type of record) contains code e (Cartographic material) or f (Manuscript cartographic material).
            else if ("ef".IndexOf(header.Value[6]) != -1)
                strType = "maps";
            // http://www.loc.gov/marc/bibliographic/bd008m.html
            // Music definition of field 008/18-34 is used when Leader/06 (Type of record) contains code c (Notated music), d (Manuscript notated music), i (Nonmusical sound recording), or j (Musical sound recording).
            else if ("cdij".IndexOf(header.Value[6]) != -1)
                strType = "music";
            // http://www.loc.gov/marc/bibliographic/bd008s.html
            // Continuing resources field 008/18-34 contains coded data for all continuing resources, including serials and integrating resources. It is used when Leader/06 (Type of record) contains code a (Language material) and Leader/07 contains code b (Serial component part), i (Integrating resource), or code s (Serial).
            else if ("a".IndexOf(header.Value[6]) != -1
    && "bis".IndexOf(header.Value[7]) != -1)
                strType = "contining_resources";
            // http://www.loc.gov/marc/bibliographic/bd008v.html
            // Visual materials definition of field 008/18-34 is used when Leader/06 (Type of record) contains code g (Projected medium), code k (Two-dimensional nonprojectable graphic, code o (Kit), or code r (Three-dimensional artifact or naturally occurring object).
            else if ("gkor".IndexOf(header.Value[6]) != -1)
                strType = "visual_materials";
            // http://www.loc.gov/marc/bibliographic/bd008x.html
            // Mixed materials definition of field 008/18-34 is used when Leader/06 (Type of record) contains code p (Mixed material). 
            else if ("p".IndexOf(header.Value[6]) != -1)
                strType = "mixed_materials";
            else
            {
                e.ErrorInfo = "无法根据当前头标区 '" + header.Value.Replace(" ", "_") + "' 内容辨别文献类型，所以无法获得模板定义";
                return;
            }


            e.DefNode = this.DetailForm.MarcEditor.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + e.FieldName + "' and @type='" + strType + "']");
            if (e.DefNode == null)
            {
                e.ErrorInfo = "字段名为 '" + e.FieldName + "' 类型为='" + strType + "' 的模板定义无法在MARC定义文件中找到";
                return;
            }

            e.Title = "008 " + strType;
            return;
        }
        if (e.FieldName == "007")
        {
            if (this.DetailForm.MarcEditor.MarcDefDom == null)
            {
                e.ErrorInfo = "MarcEditor中的MarcDefDom尚未准备好...";
                return;
            }

            string strType = "";

            if (e.Value.Length < 1)
            {
                // 权且当作 'a' 处理
                strType = "map";
            }
            else
            {
                // http://www.loc.gov/marc/bibliographic/bd007.html
                // Map (007/00=a)
                if (e.Value[0] == 'a')
                    strType = "map";
                // Electronic resource (007/00=c)
                else if (e.Value[0] == 'c')
                    strType = "electronic_resource";
                // Globe (007/00=d)
                else if (e.Value[0] == 'd')
                    strType = "globe";
                // Tactile material (007/00=f)
                else if (e.Value[0] == 'f')
                    strType = "tactile_material";
                // Projected graphic (007/00=g)
                else if (e.Value[0] == 'g')
                    strType = "projected_graphic";
                // Microform (007/00=h)
                else if (e.Value[0] == 'h')
                    strType = "microform";
                // Nonprojected graphic (007/00=k)
                else if (e.Value[0] == 'k')
                    strType = "nonprojected_graphic";
                // Motion picture (007/00=m)
                else if (e.Value[0] == 'm')
                    strType = "motion_picture";
                // Kit (007/00=o)
                else if (e.Value[0] == 'o')
                    strType = "kit";
                // Notated music (007/00=q)
                else if (e.Value[0] == 'q')
                    strType = "notated_music";
                // Remote-sensing image (007/00=r)
                else if (e.Value[0] == 'r')
                    strType = "remote-sensing_image";
                // Sound recording (007/00=s)
                else if (e.Value[0] == 's')
                    strType = "sound_recording";
                // Text (007/00=t)
                else if (e.Value[0] == 't')
                    strType = "text";
                // Videorecording (007/00=v)
                else if (e.Value[0] == 'v')
                    strType = "videorecording";
                // Unspecified (007/00=z)
                else if (e.Value[0] == 'z')
                    strType = "unspecified";
                else
                {
                    e.ErrorInfo = "无法根据当前007字段第一字符内容 '" + e.Value[0].ToString() + "' 从MARC定义文件中获得模板定义";
                    return;
                }
            }

            e.DefNode = this.DetailForm.MarcEditor.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + e.FieldName + "' and @type='" + strType + "']");
            if (e.DefNode == null)
            {
                e.ErrorInfo = "字段名为 '" + e.FieldName + "' 类型为='" + strType + "' 的模板定义无法在MARC定义文件中找到";
                return;
            }

            e.Title = "007 " + strType;
            return;
        }

        e.Canceled = true;
    }
}