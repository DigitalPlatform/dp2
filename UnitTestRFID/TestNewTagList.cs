using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform;
using DigitalPlatform.RFID;
using static DigitalPlatform.RFID.LogicChip;

namespace UnitTestRFID
{
    [TestClass]
    public class TestNewTagList
    {
        // 测试添加一个标签到空列表中
        // 获得标签详细内容时，返回的是空
        [TestMethod]
        public void Test_add_1()
        {
            NewTagList.Clear();

            List<OneTag> tag_list = new List<OneTag>();

            // 准备内容
            tag_list.Add(BuildOne15693Tag("M201", 1, "111111"));

            List<TagAndData> new_results = new List<TagAndData>();
            List<TagAndData> changed_results = new List<TagAndData>();
            List<TagAndData> removed_results = new List<TagAndData>();

            List<TypeAndError> errors = new List<TypeAndError>();

            NewTagList.Refresh(
                "*",
                tag_list,
                (readerName, uid, antennaID) =>
                {
                    // 获得标签内容
                    return new GetTagInfoResult { };
                    // return null;
                },
                (new_tags, changed_tags, removed_tags) =>
                {
                    if (new_tags != null)
                        new_results.AddRange(new_tags);
                    if (changed_tags != null)
                        changed_results.AddRange(changed_tags);
                    if (removed_tags != null)
                        removed_results.AddRange(removed_tags);
                },
                (string type, string error) =>
                {
                    errors.Add(new TypeAndError
                    {
                        Type = type,
                        Error = error
                    });
                });

            Assert.AreEqual(1, new_results.Count);

            // 检查得到的 new_results
            var result = new_results[0];
            Assert.AreEqual(null, result.OneTag.TagInfo);
            Assert.IsTrue(IsEqual(result.OneTag, tag_list[0]));

            Assert.AreEqual(0, changed_results.Count);
            Assert.AreEqual(0, removed_results.Count);

            Assert.AreEqual(1, NewTagList.Tags.Count);
        }

        // 测试添加一个标签到空列表中
        // 获得标签详细内容时，返回正常的结果
        [TestMethod]
        public void Test_add_2()
        {
            NewTagList.Clear();

            List<OneTag> tag_list = new List<OneTag>();

            // 准备内容
            tag_list.Add(BuildOne15693Tag("M201", 1, "111111"));

            List<TagAndData> new_results = new List<TagAndData>();
            List<TagAndData> changed_results = new List<TagAndData>();
            List<TagAndData> removed_results = new List<TagAndData>();

            List<TypeAndError> errors = new List<TypeAndError>();

            NewTagList.Refresh(
                "*",
                tag_list,
                (readerName, uid, antennaID) =>
                {
                    // 获得标签内容
                    return new GetTagInfoResult
                    {
                        TagInfo = BuildOne15693TagInfo(tag_list[0])
                    };
                    // return null;
                },
                (new_tags, changed_tags, removed_tags) =>
                {
                    if (new_tags != null && new_tags.Count > 0)
                    {
                        // 检查得到的 new_tags
                        Assert.AreEqual(1, new_tags.Count);
                        var result = new_tags[0];
                        // 这个时候，TagInfo 还是 null
                        Assert.AreEqual(null, result.OneTag.TagInfo);
                        Assert.IsTrue(IsEqual(result.OneTag, tag_list[0]));

                        new_results.AddRange(new_tags);
                    }
                    if (changed_tags != null && changed_tags.Count > 0)
                    {
                        Assert.AreEqual(1, changed_tags.Count);

                        var result = changed_tags[0];
                        // 这个时候，TagInfo 应该不是 null 了
                        Assert.AreNotEqual(null, result.OneTag.TagInfo);
                        // 比较除了 TagInfo 以外的成员
                        Assert.IsTrue(IsEqual(result.OneTag, tag_list[0], false));
                        // 专门比较 TagInfo
                        Assert.IsTrue(IsEqual(BuildOne15693TagInfo(tag_list[0]), result.OneTag.TagInfo));

                        changed_results.AddRange(changed_tags);
                    }
                    if (removed_tags != null)
                        removed_results.AddRange(removed_tags);
                },
                (string type, string error) =>
                {
                    errors.Add(new TypeAndError
                    {
                        Type = type,
                        Error = error
                    });
                });

            Assert.AreEqual(1, new_results.Count);
            Assert.AreEqual(1, changed_results.Count);
            Assert.AreEqual(0, removed_results.Count);

            Assert.AreEqual(1, NewTagList.Tags.Count);
        }

        // TODO: 如果两个读卡器都发现了一个同样 UID 的标签，要做一下取舍

        // 测试对列表中的标签进行更新。从一个读卡器变动到另一个读卡器
        [TestMethod]
        public void Test_update_1()
        {
            NewTagList.Clear();

            List<OneTag> tag_list = new List<OneTag>();

            // 准备内容
            tag_list.Add(BuildOne15693Tag("M201", 1, "111111"));
            NewTagList.Refresh(
                "*",
                tag_list,
                (readerName, uid, antennaID) =>
                {
                    // 获得标签内容
                    return new GetTagInfoResult
                    {
                        TagInfo = BuildOne15693TagInfo(tag_list[0])
                    };
                    // return null;
                },
                null,
                null);

            // 检查一下
            Assert.AreEqual(1, NewTagList.Tags.Count);

            List<TagAndData> new_results = new List<TagAndData>();
            List<TagAndData> changed_results = new List<TagAndData>();
            List<TagAndData> removed_results = new List<TagAndData>();

            List<TypeAndError> errors = new List<TypeAndError>();


            List<OneTag> update_list = new List<OneTag>();
            update_list.Add(BuildOne15693Tag("RL8600", 2, "111111"));

            // 用 update_list 集合来刷新
            NewTagList.Refresh(
                "*",
                update_list,
                (readerName, uid, antennaID) =>
                {
                    // 获得标签内容
                    return new GetTagInfoResult
                    {
                        TagInfo = BuildOne15693TagInfo(update_list[0])
                    };
                },
                (new_tags, changed_tags, removed_tags) =>
                {
                    if (new_tags != null && new_tags.Count > 0)
                    {
                        throw new Exception("new_tags 不应该发生添加");
                    }
                    if (changed_tags != null && changed_tags.Count > 0)
                    {
                        // 检查
                        Assert.AreEqual(1, changed_tags.Count);

                        var result = changed_tags[0];
                        // 比较除了 TagInfo 以外的成员
                        Assert.IsTrue(IsEqual(result.OneTag, update_list[0], false));

                        if (result.OneTag.TagInfo == null)
                        {

                        }
                        else
                        {
                            // 这个时候，TagInfo 应该不是 null 了
                            Assert.AreNotEqual(null, result.OneTag.TagInfo);
                            // 专门比较 TagInfo
                            Assert.IsTrue(IsEqual(BuildOne15693TagInfo(update_list[0]), result.OneTag.TagInfo));
                        }

                        changed_results.AddRange(changed_tags);
                    }
                    if (removed_tags != null && removed_tags.Count > 0)
                    {
                        throw new Exception("removed_tags 不应该发生添加");
                    }
                },
                (string type, string error) =>
                {
                    errors.Add(new TypeAndError
                    {
                        Type = type,
                        Error = error
                    });
                });

            Assert.AreEqual(0, new_results.Count);
            Assert.AreEqual(1, changed_results.Count);  // 两阶段也是可能的
            Assert.AreEqual(0, removed_results.Count);

            Assert.AreEqual(1, NewTagList.Tags.Count);
        }


        // 测试从列表中移走标签
        // 获得标签详细内容时，返回正常的结果
        [TestMethod]
        public void Test_remove_1()
        {
            NewTagList.Clear();

            List<OneTag> tag_list = new List<OneTag>();

            // 准备内容
            tag_list.Add(BuildOne15693Tag("M201", 1, "111111"));
            NewTagList.Refresh(
                "*",
                tag_list,
                (readerName, uid, antennaID) =>
                {
                    // 获得标签内容
                    return new GetTagInfoResult
                    {
                        TagInfo = BuildOne15693TagInfo(tag_list[0])
                    };
                    // return null;
                },
                null,
                null);

            List<TagAndData> new_results = new List<TagAndData>();
            List<TagAndData> changed_results = new List<TagAndData>();
            List<TagAndData> removed_results = new List<TagAndData>();

            List<TypeAndError> errors = new List<TypeAndError>();

            // 用空集合来刷新
            NewTagList.Refresh(
                "*",
                new List<OneTag>(),
                (readerName, uid, antennaID) =>
                {
                    throw new Exception("不应该被调用");
                },
                (new_tags, changed_tags, removed_tags) =>
                {
                    if (new_tags != null && new_tags.Count > 0)
                    {
                        throw new Exception("new_tags 不应该发生添加");
                    }
                    if (changed_tags != null && changed_tags.Count > 0)
                    {
                        throw new Exception("changed_tags 不应该发生添加");
                    }
                    if (removed_tags != null && removed_tags.Count > 0)
                    {
                        // 检查
                        Assert.AreEqual(1, removed_tags.Count);

                        var result = removed_tags[0];
                        // 这个时候，TagInfo 应该不是 null 了
                        Assert.AreNotEqual(null, result.OneTag.TagInfo);
                        // 比较除了 TagInfo 以外的成员
                        Assert.IsTrue(IsEqual(result.OneTag, tag_list[0], false));
                        // 专门比较 TagInfo
                        Assert.IsTrue(IsEqual(BuildOne15693TagInfo(tag_list[0]), result.OneTag.TagInfo));

                        removed_results.AddRange(removed_tags);
                    }
                },
                (string type, string error) =>
                {
                    errors.Add(new TypeAndError
                    {
                        Type = type,
                        Error = error
                    });
                });

            Assert.AreEqual(0, new_results.Count);
            Assert.AreEqual(0, changed_results.Count);
            Assert.AreEqual(1, removed_results.Count);

            Assert.AreEqual(0, NewTagList.Tags.Count);
        }

        public static OneTag BuildOne15693Tag(string readerName,
            uint antennaID,
            string uid)
        {
            OneTag tag = new OneTag();
            tag.Protocol = InventoryInfo.ISO15693;
            tag.ReaderName = readerName;
            tag.AntennaID = antennaID;
            tag.UID = uid;

            return tag;
        }

        public static TagInfo BuildOne15693TagInfo(OneTag tag)
        {
            TagInfo info = new TagInfo();
            info.ReaderName = tag.ReaderName;
            info.AntennaID = tag.AntennaID;
            info.UID = tag.UID;

            // 每个块内包含的字节数
            info.BlockSize = 4;
            // 块最大总数
            info.MaxBlockCount = 28;
            info.Bytes = BuildBytes(4, 28, "0000001");
            return info;
        }

        static byte[] BuildBytes(
            int block_size,
            int max_block_count,
            string pii)
        {
            LogicChip chip = new LogicChip();
            chip.NewElement(ElementOID.PII, pii);
            chip.NewElement(ElementOID.OMF, "BA");

            return chip.GetBytes(block_size * max_block_count,
                 block_size,
                 GetBytesStyle.None,
                 out string block_map);
        }

        #region 一些用于比较的函数

        static bool IsEqual(OneTag tag1, OneTag tag2,
            bool compare_tagInfo = true)
        {
            if (tag1.Protocol != tag2.Protocol)
                return false;
            if (tag1.UID != tag2.UID)
                return false;
            if (tag1.ReaderName != tag2.ReaderName)
                return false;
            if (tag1.AntennaID != tag2.AntennaID)
                return false;

            if (tag1.DSFID != tag2.DSFID)
                return false;
            if (compare_tagInfo)
            {
                if (tag1.TagInfo == null && tag2.TagInfo == null)
                {

                }
                else
                {
                    if (IsEqual(tag1.TagInfo, tag2.TagInfo) == false)
                        return false;
                }
            }

            return true;
        }

        static bool IsEqual(TagInfo info1, TagInfo info2)
        {
            if (info1.UID != info2.UID)
                return false;
            if (info1.ReaderName != info2.ReaderName)
                return false;
            if (info1.AntennaID != info2.AntennaID)
                return false;

            if (info1.DSFID != info2.DSFID)
                return false;
            if (info1.AFI != info2.AFI)
                return false;
            if (info1.EAS != info2.EAS)
                return false;

            if (ByteArray.Compare(info1.Bytes, info2.Bytes) != 0)
                return false;

            return true;
        }

        #endregion
    }

    public class TypeAndError
    {
        public string Type { get; set; }
        public string Error { get; set; }
    }
}
