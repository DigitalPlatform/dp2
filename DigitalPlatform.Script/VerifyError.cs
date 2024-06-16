using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Script
{
    // 2024/6/16
    /// <summary>
    /// 描述校验结果行
    /// </summary>
    public class VerifyError
    {
        // 错误级别。为 info warning error 三种
        public string Level { get; set; }
        public string Text { get; set; }

        public static VerifyError FromLine(string line,
            bool convert_level = true)
        {
            if (line.StartsWith("[") == false)
                return new VerifyError { Text = line };
            var parts = StringUtil.ParseTwoPart(line, "]");
            var level = parts[0]?.TrimStart('[', ' ');
            var text = parts[1]?.TrimStart(' ');
            var error =  new VerifyError
            {
                Level = level,
                Text = text
            };
            if (convert_level)
            {
                if (error.Level == "错误")
                    error.Level = "error";
                else if (error.Level == "警告")
                    error.Level = "warning";
                else if (error.Level == "信息")
                    error.Level = "info";
                else if (error.Level == "成功")
                    error.Level = "succeed";
            }
            return error;
        }

        public static void AddError(List<VerifyError> errors,
            string text)
        {
            errors.Add(new VerifyError
            {
                Level = "error",
                Text = text
            });
        }

        public static void AddWarning(List<VerifyError> errors,
    string text)
        {
            errors.Add(new VerifyError
            {
                Level = "warning",
                Text = text
            });
        }

        public static void AddInfo(List<VerifyError> errors,
    string text)
        {
            errors.Add(new VerifyError
            {
                Level = "info",
                Text = text
            });
        }

        public static void AddSucceed(List<VerifyError> errors,
string text)
        {
            errors.Add(new VerifyError
            {
                Level = "succeed",
                Text = text
            });
        }

        public static string BuildTextLines(List<VerifyError> errors)
        {
            if (errors == null)
                return "";
            StringBuilder result = new StringBuilder();
            foreach (var error in errors)
            {
                result.AppendLine($"[{error.Level}] {error.Text}");
            }

            return result.ToString();
        }
    }

}
