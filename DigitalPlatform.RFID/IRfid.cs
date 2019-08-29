﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{
    /// <summary>
    /// RFID 功能接口
    /// </summary>
    public interface IRfid
    {
        // 列出当前可用的 reader
        ListReadersResult ListReaders();

        NormalResult GetState(string style);

        NormalResult ActivateWindow();

        ListTagsResult ListTags(string reader_name, string style);

        GetTagInfoResult GetTagInfo(string reader_name, string uid);

        NormalResult WriteTagInfo(
    string reader_name,
    TagInfo old_tag_info,
    TagInfo new_tag_info);

        // parameters:
        //      tag_name    标签名字。为 pii:xxxx 或者 uid:xxxx 形态
        NormalResult SetEAS(
string reader_name,
string tag_name,
bool enable);

        NormalResult ChangePassword(string reader_name,
string uid,
string type,
uint old_password,
uint new_password);

        // 开始或者结束捕获标签
        NormalResult BeginCapture(bool begin);

        NormalResult EnableSendKey(bool enable);
    }

    [Serializable()]
    public class ListReadersResult : NormalResult
    {
        public string[] Readers { get; set; }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            text.Append(base.ToString() + "\r\n");
            if (Readers != null)
            {
                int i = 1;
                foreach (string reader in Readers)
                {
                    text.Append($"{i}) {reader.ToString()}\r\n");
                    i++;
                }
            }
            return text.ToString();
        }
    }

    [Serializable()]
    public class OneTag
    {
        public string Protocol { get; set; }

        public string ReaderName { get; set; }
        public string UID { get; set; }
        public DateTime LastActive { get; set; }

        public byte DSFID { get; set; }
        // public InventoryInfo InventoryInfo { get; set; }

        public TagInfo TagInfo { get; set; }

        public OneTag()
        {
            this.LastActive = DateTime.Now;
        }

        // 2019/8/29
        public OneTag Clone()
        {
            OneTag result = new OneTag();
            result.Protocol = this.Protocol;
            result.ReaderName = this.ReaderName;
            result.UID = this.UID;
            result.LastActive = this.LastActive;
            result.DSFID = this.DSFID;
            result.TagInfo = this.TagInfo;
            return result;
        }

        public override string ToString()
        {
            return $"ReaderName={ReaderName},UID={UID},DSFID={Element.GetHexString(DSFID)},Protocol={Protocol}";
        }

        public string GetDescription()
        {
            StringBuilder text = new StringBuilder();
            text.Append(this.ToString() + "\r\n");
            if (this.TagInfo != null)
                text.Append($"*** TagInfo:\r\n{TagInfo.ToString()}");

            /*
            if (this.OriginBytes != null)
            {
                text.Append($"\r\n初始字节内容:\r\n{GetBytesString(this.OriginBytes, this.BlockSize, this.OriginLockStatus)}\r\n");
            }

            {
                LogicChip chip = LogicChip.From(this.OriginBytes, this.BlockSize, this.OriginLockStatus);
                text.Append($"初始元素:(共 {chip.Elements.Count} 个)\r\n");
                int i = 0;
                foreach (Element element in chip.Elements)
                {
                    text.Append($"{++i}) {element.ToString()}\r\n");
                }
            }

            try
            {
                // 注意 GetBytes() 调用后，元素排列顺序会发生变化
                byte[] bytes = this.GetBytes(
                    this.MaxBlockCount * this.BlockSize,
                    this.BlockSize,
                    GetBytesStyle.None,
                    out string block_map);
                text.Append($"\r\n当前字节内容:\r\n{GetBytesString(bytes, this.BlockSize, block_map)}\r\n");
            }
            catch (Exception ex)
            {
                text.Append($"\r\n当前字节内容:\r\n构造 Bytes 过程出现异常: {ex.Message}\r\n");
            }

            {
                text.Append($"当前元素:(共 {this.Elements.Count} 个)\r\n");
                int i = 0;
                foreach (Element element in this.Elements)
                {
                    text.Append($"{++i}) {element.ToString()}\r\n");
                }
            }
            */
            return text.ToString();
        }

    }

    [Serializable()]
    public class ListTagsResult : NormalResult
    {
        public List<OneTag> Results { get; set; }

        public ListTagsResult()
        {

        }

        public ListTagsResult(NormalResult result) : base(result)
        {

        }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            text.Append(base.ToString() + "\r\n");
            if (Results != null)
            {
                int i = 1;
                foreach (OneTag info in Results)
                {
                    text.Append($"{i++}) {info.ToString()}\r\n");
                }
            }
            return text.ToString();
        }
    }


}
