using DigitalPlatform.RFID;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2SSL
{
    /// <summary>
    /// 和盘点有关的数据结构
    /// </summary>
    public static class InventoryData
    {
        // UID --> entity
        static Hashtable _entityTable = new Hashtable();

        public static Entity AddEntity(TagAndData tag, out bool isNewly)
        {
            if (_entityTable.ContainsKey(tag.OneTag.UID))
            {
                // TODO: 更新 tagInfo
                isNewly = false;
                Entity result = _entityTable[tag.OneTag.UID] as Entity;
                InventoryData.NewEntity(tag, result, false);
                return result;
            }

            var entity = InventoryData.NewEntity(tag, null, false);
            _entityTable[entity.UID] = entity;
            isNewly = true;
            return entity;
        }

        // 注：所创建的 Entity 对象其 Error 成员可能有值，表示有出错信息
        // Exception:
        //      可能会抛出异常 ArgumentException
        static Entity NewEntity(TagAndData tag, 
            Entity entity,
            bool throw_exception = true)
        {
            Entity result = entity;
            if (result == null)
            {
                result = new Entity
                {
                    UID = tag.OneTag.UID,
                    ReaderName = tag.OneTag.ReaderName,
                    Antenna = tag.OneTag.AntennaID.ToString(),
                    TagInfo = tag.OneTag.TagInfo,
                };
            }

            LogicChip chip = null;
            if (string.IsNullOrEmpty(tag.Type))
            {
                // Exception:
                //      可能会抛出异常 ArgumentException TagDataException
                try
                {
                    SetTagType(tag, out string pii, out chip);
                    result.PII = pii;
                }
                catch (Exception ex)
                {
                    App.CurrentApp.SpeakSequence("警告: 标签解析出错");
                    if (throw_exception == false)
                    {
                        result.AppendError($"RFID 标签格式错误: {ex.Message}",
                            "red",
                            "parseTagError");
                    }
                    else
                        throw ex;
                }
            }

#if NO
            // Exception:
            //      可能会抛出异常 ArgumentException 
            EntityCollection.SetPII(result, pii);
#endif

            // 2020/4/9
            if (tag.Type == "patron")
            {
                // 避免被当作图书同步到 dp2library
                result.PII = "(读者卡)" + result.PII;
                result.AppendError("读者卡误放入书柜", "red", "patronCard");
            }

            // 2020/7/15
            // 获得图书 RFID 标签的 OI 和 AOI 字段
            if (tag.Type == "book")
            {
                if (chip == null)
                {
                    // Exception:
                    //      可能会抛出异常 ArgumentException TagDataException
                    chip = LogicChip.From(tag.OneTag.TagInfo.Bytes,
            (int)tag.OneTag.TagInfo.BlockSize,
            "" // tag.TagInfo.LockStatus
            );
                }

                string oi = chip.FindElement(ElementOID.OI)?.Text;
                string aoi = chip.FindElement(ElementOID.AOI)?.Text;

                result.OI = oi;
                result.AOI = aoi;

                // 2020/8/27
                // 严格要求必须有 OI(AOI) 字段
                if (string.IsNullOrEmpty(oi) && string.IsNullOrEmpty(aoi))
                    result.AppendError("没有 OI 或 AOI 字段", "red", "missingOI");
            }
            return result;
        }

        // Exception:
        //      可能会抛出异常 ArgumentException TagDataException
        static void SetTagType(TagAndData data,
            out string pii,
            out LogicChip chip)
        {
            pii = null;
            chip = null;

            if (data.OneTag.Protocol == InventoryInfo.ISO14443A)
            {
                data.Type = "patron";
                return;
            }

            if (data.OneTag.TagInfo == null)
            {
                data.Type = ""; // 表示类型不确定
                return;
            }

            if (string.IsNullOrEmpty(data.Type))
            {
                // Exception:
                //      可能会抛出异常 ArgumentException TagDataException
                chip = LogicChip.From(data.OneTag.TagInfo.Bytes,
        (int)data.OneTag.TagInfo.BlockSize,
        "" // tag.TagInfo.LockStatus
        );
                pii = chip.FindElement(ElementOID.PII)?.Text;

                var typeOfUsage = chip.FindElement(ElementOID.TypeOfUsage)?.Text;
                if (typeOfUsage != null && typeOfUsage.StartsWith("8"))
                    data.Type = "patron";
                else
                    data.Type = "book";
            }
        }

    }
}
