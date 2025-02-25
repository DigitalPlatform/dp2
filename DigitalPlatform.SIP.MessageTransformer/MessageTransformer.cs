using Jint;
using Jint.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DigitalPlatform.SIP
{
    public class MessageTransformRuler
    {
        public int FirstIndex = -1;
        public List<string> IngoreFields = new List<string>();
        public Dictionary<string, string> TransFields = new Dictionary<string, string>();
        public Dictionary<string, string> AddFields = new Dictionary<string, string>();
        public Dictionary<string, Engine> AddScriptFields = new Dictionary<string, Engine>();
        public Dictionary<string, Dictionary<string, string>> FieldValueMapper = new Dictionary<string, Dictionary<string, string>>();

        public MessageTransformRuler() { }
    }
    public class MessageTransformer
    {
        private static readonly List<string> IgnoreMessage = new List<string>();
        private static readonly Dictionary<string, MessageTransformRuler> FilteMessage = new Dictionary<string, MessageTransformRuler>();
        private Encoding Encoding = Encoding.UTF8;

        private static MessageTransformer instance;
        public static MessageTransformer Instance()
        {
            if (instance == null)
            {
                instance = new MessageTransformer();
            }
            return instance;
        }
        private MessageTransformer() { }
        public void Initial(string rule)
        {
            IgnoreMessage.Clear();
            FilteMessage.Clear();
            StringReader stringReader = new StringReader(rule);

            string currMapperFld = "";
            MessageTransformRuler ruler = null;

            while (stringReader.Peek() != -1) // 是否读到末尾
            {
                string line = stringReader.ReadLine();
                // 处理读到的行，#后的文本属于注释信息，只取#之前的文本，然后使用分隔符 : 分割字符串，空格与制表符过滤掉，并忽略分隔后的空值
                string[] ret = line.Split('#')[0].Split(new char[] { ' ', '\t', ':' }, StringSplitOptions.RemoveEmptyEntries);

                if (ret.Length > 0)
                {
                    if (ret[0] == "msg") // 消息处理规则
                    {
                        if (ret.Length == 3 && ret[2] == "ign")
                        {
                            // 消息忽略，如果检测到消息处理方式是忽略，则服务器不应该传给设备端，或者设备端不应该发送给服务器。应该拦截后不处理
                            IgnoreMessage.Add(ret[1]);
                        }
                        else
                        {
                            ruler = new MessageTransformRuler();
                            if (ret.Length == 4 && ret[2] == "beg") // 分割后有4部分，说明接下来的一行或多行，有一行是属于头部后的第一个字段，需要beg参数确定第一个字段的位置
                            {
                                ruler.FirstIndex = Int32.Parse(ret[3]);
                            }

                            if (!FilteMessage.ContainsKey(ret[1]))
                            {
                                FilteMessage.Add(ret[1], ruler);
                            }
                            // 分割后有4部分，说明接下来的一行或多行，有一行是属于头部后的第一个字段，需要beg参数确定第一个字段的位置
                            // 分割后只有2部分，说明接下来的一行或多行，属于当前消息号的字段处理部分
                        }
                    }
                    else if (ret[0] == "fld") // 字段处理规则
                    {
                        if (ret.Length == 3 && ret[2] == "ign")
                        {
                            ruler.IngoreFields.Add(ret[1]);// 字段跳过处理
                        }
                        else if (ret.Length == 3 && ret[2] == "map")
                        {
                            ruler.FieldValueMapper.Add(ret[1], new Dictionary<string, string>());
                            currMapperFld = ret[1];
                        }
                        else if (ret.Length == 4 && ret[2] == "tsf")
                        {
                            ruler.TransFields.Add(ret[1], ret[3]);
                            currMapperFld = ret[1];
                        }
                        else if (ret.Length == 4 && ret[2] == "add")
                        {
                            if (ret[3] != "```")
                            {
                                ruler.AddFields.Add(ret[1], ret[3]);
                            }
                            else
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                // 进行行级读取，不做任何检测，直到再次遇到 ``` 会结束脚本读入
                                while (stringReader.Peek() != -1)
                                {
                                    string scriptLine = stringReader.ReadLine();
                                    if (scriptLine == "```")
                                    { // 脚本结束符
                                        break;
                                    }
                                    else
                                    {
                                        // 将脚本代码追加到字符串对象
                                        stringBuilder.Append(scriptLine);
                                    }
                                }

                                Engine en = new Engine();
                                // 这里需要运行脚本，包括定义一些函数，或者一些变量值，相当于将脚本预编译。到处理数据那一步，才真正的传入消息数据，对数据进行加工后返回处理结果
                                // 预先定义好函数名，将代码作用域限定，在处理数据时使用Invoke方法调用函数名，并将消息当成参数传入。
                                stringBuilder.Insert(0, "function callTransformerResult(msg){");
                                stringBuilder.Append("}");
                                en.Execute(stringBuilder.ToString());
                                ruler.AddScriptFields.Add(ret[1], en);
                            }
                        }
                        else if (ret.Length == 5 && ret[2] == "tsf" && ret[4] == "map")
                        {
                            ruler.TransFields.Add(ret[1], ret[3]);
                            ruler.FieldValueMapper.Add(ret[1], new Dictionary<string, string>());
                            currMapperFld = ret[1];
                        }
                    }
                    else if (ret[0] == "map")
                    {
                        ruler.FieldValueMapper[currMapperFld].Add(ret[1], ret[2]);
                    }
                    else if (ret[0] == "quirk")
                    {
                        Encoding = Encoding.GetEncoding(ret[1]);
                    }
                }
            }
        }

        private string ComputeChecksum(byte[] message)
        {
            int checksum = 0;
            foreach (byte b in message)
            {
                checksum += b;
            }
            checksum = (checksum ^ 0xFFFF) + 1;
            return string.Format("{0:X4}", checksum);
        }

        public void Process(string message, out string msg)
        {
            string msgCode = message.Substring(0, 2);
            if (IgnoreMessage.Contains(msgCode)) // 检查消息是否要忽略的
            {
                msg = null;
                return;
            }
            if (FilteMessage.ContainsKey(msgCode))
            {
                MessageTransformRuler ruler = FilteMessage[msgCode];
                string[] msgParts = new string[2];
                if (ruler.FirstIndex > -1) // 指定了第一个位置，说明紧跟着头的第一个字段正文需要处理
                {
                    // 将消息分割成头与正文两部分
                    msgParts[0] = message.Substring(0, ruler.FirstIndex);
                    msgParts[1] = message.Substring(ruler.FirstIndex);
                }
                else
                {
                    // 因为紧挨着头的字段不需要处理，所以用第一个 | 来做分割，取后面部分
                    int firstIndex = message.IndexOf('|');
                    msgParts[0] = message.Substring(0, firstIndex);
                    msgParts[1] = message.Substring(firstIndex);
                }

                string[] fieldParts = msgParts[1].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                string[] fieldTempParts = new string[fieldParts.Length];

                for (int i = 0; i < fieldParts.Length; i++)
                {
                    string fieldName = fieldParts[i].Substring(0, 2);
                    fieldTempParts[i] = fieldParts[i];
                    if (ruler.IngoreFields.Contains(fieldName)) // 如果过滤字段里包含当前
                    {
                        fieldTempParts[i] = ""; // 赋值为空字符串，join时过滤掉
                    }
                    if (ruler.TransFields.ContainsKey(fieldName)) // 如果变换字段里包含当前
                    {
                        fieldTempParts[i] = fieldParts[i].Remove(0, 2).Insert(0, ruler.TransFields[fieldName]); // 移除掉赋值为空字符串，join时过滤掉
                    }
                    if (ruler.FieldValueMapper.ContainsKey(fieldName)) // 如果值映射字段里包含当前
                    {
                        // 这里需要做一个检测，可能某个字段既要改字段名又要进行值映射，进行值映射的时候，需要修改包含新的字段名的字段。
                        if (ruler.FieldValueMapper.ContainsKey(fieldName))
                        {
                            string value = fieldTempParts[i].Remove(0, 2);
                            if (ruler.FieldValueMapper[fieldName].ContainsKey(value))
                            {
                                fieldTempParts[i] = fieldTempParts[i].Remove(2).Insert(2, ruler.FieldValueMapper[fieldName][value]); // 替换值
                            }
                        }
                        else
                        {
                            string value = fieldParts[i].Remove(0, 2);
                            if (ruler.FieldValueMapper[fieldName].ContainsKey(value))
                            {
                                fieldTempParts[i] = fieldParts[i].Remove(2).Insert(2, ruler.FieldValueMapper[fieldName][value]); // 替换值
                            }
                        }
                    }
                }
                // 完成基础变换后的字段重新拼接
                msgParts[1] = string.Join("|", fieldTempParts.Where(item => !string.IsNullOrEmpty(item)));

                // 循环完消息已有字段后需要检查是否有add 跟脚本
                string[] append = new string[2] { "", "" };

                if (ruler.AddFields.Count > 0)
                {
                    List<string> addFlds = new List<string>();
                    foreach (string key in ruler.AddFields.Keys)
                    {
                        addFlds.Add(key + ruler.AddFields[key]);
                    }

                    // 追加 add 关键字的字段到消息末尾
                    append[0] = string.Join("|", addFlds.ToArray().Where(item => !string.IsNullOrEmpty(item)));
                }
                if (ruler.AddScriptFields.Count > 0)
                {
                    List<string> addFlds = new List<string>();
                    foreach (string key in ruler.AddScriptFields.Keys)
                    {
                        // 从脚本里获取到执行结果，callTransformerResult 是初始化Engine时设置的函数名，定义的所有代码都是基于这个作用域里。
                        JsValue val = null;
                        try
                        {
                            val = ruler.AddScriptFields[key].Invoke("callTransformerResult", message);
                            addFlds.Add(key + val.AsString());
                        }
                        catch (Jint.Runtime.JavaScriptException e)
                        {
                            // 脚本运行异常报错的话，将空值赋值给字段，
                            addFlds.Add(key + "");
                        }
                    }
                    // 追加 add 关键字的字段到消息末尾
                    append[1] = string.Join("|", addFlds.ToArray().Where(item => !string.IsNullOrEmpty(item)));
                }
                string appended = string.Join("|", append.Where(item => !string.IsNullOrEmpty(item)));


                // 检查消息是否有 AYxAZxxxx，有的话需要重新计算 checksum 
                int pos = msgParts[1].IndexOf("|AY");
                if (pos > 0)
                {
                    // msgParts[1] 去掉 ay az 后与 appended 合并
                    msg = string.Join("|", new string[] { msgParts[1].Substring(0, pos), appended }.Where(item => !string.IsNullOrEmpty(item)));
                    // 定长头 与刚赋值的 msg 合并
                    msg = string.Join(ruler.FirstIndex > -1 ? "" : "|", new string[] { msgParts[0], msg });
                    // 重新计算checksum，与 ay az 合并成完整的消息
                    msg = msg + msgParts[1].Substring(pos, 6) + ComputeChecksum(Encoding.GetBytes(msg));
                }
                else
                {
                    // 没有AY AZ字段，将新字段追加到消息末尾
                    msg = string.Join(ruler.FirstIndex > -1 ? "" : "|", new string[] { msgParts[0], string.Join("|", new string[] { msgParts[1], appended }.Where(item => !string.IsNullOrEmpty(item))) });
                }
                return;
            }
            // 不进行任何处理，返回
            msg = message;
        }
    }
}
