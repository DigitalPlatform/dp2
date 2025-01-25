using DigitalPlatform;
using DigitalPlatform.rms.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dp2KernelApiTester.TestCase
{
    public static class TestQueryXml
    {
        static string _strDatabaseName1 = "中文图书";
        static string _strDatabaseName2 = "中文期刊";
        static string _strDatabaseName3 = "西文图书";
        static string _strDatabaseName4 = "西文期刊";

        public static NormalResult TestAll(
            CancellationToken token,
            string style = null)
        {
            {
                var result = PrepareEnvironment();
                if (result.Value == -1)
                    return result;
            }

            // 多个途径并列
            {
                // 不限制命中数
                {
                    DataModel.SetMessage($"=== 多个途径并列，不限制命中数 ===");

                    var result = TestMultiFromSearch(
                        new QueryStyle { LimitCount = -1 },
                        token);
                    if (result.Value == -1)
                        return result;

                    DataModel.SetMessage($"=== 多个途径并列，不限制命中数，SQL 排序 ===");

                    result = TestMultiFromSearch(
        new QueryStyle
        {
            LimitCount = -1,
            OrderInXml = "DESC"
        },
        token);
                    if (result.Value == -1)
                        return result;
                }

                // 限制命中数
                {
                    DataModel.SetMessage($"=== 多个途径并列，限制命中数 1000 ===");

                    var result = TestMultiFromSearch(
                        new QueryStyle { LimitCount = 1000 },
                        token);
                    if (result.Value == -1)
                        return result;

                    DataModel.SetMessage($"=== 多个途径并列，限制命中数，SQL 排序 ===");

                    result = TestMultiFromSearch(
        new QueryStyle
        {
            LimitCount = 1000,
            OrderInXml = "DESC"
        },
        token);
                    if (result.Value == -1)
                        return result;
                }
            }

            // 空值检索
            {
                // 不限制命中数
                {
                    DataModel.SetMessage($"=== 空值检索，不限制命中数 ===");

                    var result = TestNullSearch(
                        new QueryStyle { LimitCount = -1 },
                        token);
                    if (result.Value == -1)
                        return result;

                    DataModel.SetMessage($"=== 空值检索，不限制命中数，SQL 排序 ===");

                    result = TestNullSearch(
        new QueryStyle
        {
            LimitCount = -1,
            OrderInXml = "DESC"
        },
        token);
                    if (result.Value == -1)
                        return result;
                }

                // 限制命中数
                {
                    DataModel.SetMessage($"=== 空值检索，限制命中数 1000 ===");

                    var result = TestNullSearch(
                        new QueryStyle { LimitCount = 1000 },
                        token);
                    if (result.Value == -1)
                        return result;

                    DataModel.SetMessage($"=== 空值检索，限制命中数，SQL 排序 ===");

                    result = TestNullSearch(
        new QueryStyle
        {
            LimitCount = 1000,
            OrderInXml = "DESC"
        },
        token);
                    if (result.Value == -1)
                        return result;
                }
            }

            // 普通的逻辑检索
            {
                // 不限制命中数
                {
                    DataModel.SetMessage($"=== 测试不限制命中数 ===");

                    var result = TestSingleDbLogicSearch(
                        new QueryStyle { LimitCount = -1 },
                        token);
                    if (result.Value == -1)
                        return result;

                    DataModel.SetMessage($"=== 测试不限制命中数，SQL 排序 ===");

                    result = TestSingleDbLogicSearch(
        new QueryStyle
        {
            LimitCount = -1,
            OrderInXml = "DESC"
        },
        token);
                    if (result.Value == -1)
                        return result;
                }

                // 限制命中数
                {
                    DataModel.SetMessage($"=== 测试限制命中数 1000 ===");

                    var result = TestSingleDbLogicSearch(
                        new QueryStyle { LimitCount = 1000 },
                        token);
                    if (result.Value == -1)
                        return result;

                    DataModel.SetMessage($"=== 测试限制命中数，SQL 排序 ===");

                    result = TestSingleDbLogicSearch(
        new QueryStyle
        {
            LimitCount = 1000,
            OrderInXml = "DESC"
        },
        token);
                    if (result.Value == -1)
                        return result;
                }

            }

            {
                var result = Finish();
                if (result.Value == -1)
                    return result;
            }

            return new NormalResult();
        }

        static string[] database_names = new string[] {
                _strDatabaseName1,
                _strDatabaseName2,
                _strDatabaseName3,
                _strDatabaseName4,
        };

        public static NormalResult PrepareEnvironment()
        {
            string strError = "";

            var channel = DataModel.GetChannel();

            DataModel.SetMessage("准备环境成功", "green");
            return new NormalResult();
        ERROR1:
            DataModel.SetMessage($"PrepareEnvironment() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        public static NormalResult Finish()
        {
            string strError = "";

            var channel = DataModel.GetChannel();

            DataModel.SetMessage("清理环境成功", "green");
            return new NormalResult();
        ERROR1:
            DataModel.SetMessage($"Finish() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        // TODO: target list 中多个检索途径并列的测试
        public static NormalResult TestMultiFromSearch(
QueryStyle queryStyle,
CancellationToken token)
        {
            var channel = DataModel.GetChannel();
            string resultset_name = "default";

            string BuildQuery1(string dbname)
            {
                string orderElement = "";
                if (string.IsNullOrEmpty(queryStyle.OrderInXml) == false)
                    orderElement = $"<order>{queryStyle.OrderInXml}</order>";
                return $"<target list='{dbname}:题名,责任者,__id'><item><word>中国人</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>{queryStyle.LimitCount}</maxCount>{orderElement}</item><lang>chi</lang></target>";
            }

            string BuildOutputStyle()
            {
                return $"explain,{queryStyle.SortInOutputStyle}";
            }

            var ctr = token.Register(() =>
            {
                try
                {
                    channel.BeginStop();
                }
                catch
                {

                }
            });

            try
            {
                var database_name = "中文期刊";
                {
                    token.ThrowIfCancellationRequested();

                    {
                        string query1 = BuildQuery1(database_name);
                        var ret = channel.DoSearch(query1,
                            resultset_name,
                            BuildOutputStyle(),
                            out string explain_info,
                            out string strError);
                        if (ret == -1)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"DoSearch() 出错: {strError}"
                            };
                        DataModel.SetMessage($"多个途径并列检索验证成功");
                        DataModel.SetMessage($"---\r\nexplain:\r\n {explain_info}\r\n---", "recpath");
                    }

                    token.ThrowIfCancellationRequested();
                }

                return new NormalResult();
            }
            finally
            {
                ctr.Dispose();
            }
        }


        // TODO: target list 中多个数据库，每个数据库多个检索途径并列的测试

        // 测试空值检索
        public static NormalResult TestNullSearch(
    QueryStyle queryStyle,
    CancellationToken token)
        {
            var channel = DataModel.GetChannel();
            string resultset_name = "default";

            string BuildQuery1(string dbname)
            {
                string orderElement = "";
                if (string.IsNullOrEmpty(queryStyle.OrderInXml) == false)
                    orderElement = $"<order>{queryStyle.OrderInXml}</order>";
                return $"<target list='{dbname}:题名'><option warning='0'/><item>{orderElement}<word></word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>{queryStyle.LimitCount}</maxCount></item><lang>zh</lang></target>";
            }

            string BuildOutputStyle()
            {
                return $"explain,{queryStyle.SortInOutputStyle}";
            }

            var ctr = token.Register(() =>
            {
                try
                {
                    channel.BeginStop();
                }
                catch
                {

                }
            });

            try
            {
                var database_name = "中文期刊";
                {
                    token.ThrowIfCancellationRequested();

                    // AND
                    {
                        string query1 = BuildQuery1(database_name);
                        var ret = channel.DoSearch(query1,
                            resultset_name,
                            BuildOutputStyle(),
                            out string explain_info,
                            out string strError);
                        if (ret == -1)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"DoSearch() 出错: {strError}"
                            };
                        DataModel.SetMessage($"空值检索验证成功");
                        DataModel.SetMessage($"---\r\nexplain:\r\n {explain_info}\r\n---", "recpath");
                    }

                    token.ThrowIfCancellationRequested();
                }

                return new NormalResult();
            }
            finally
            {
                ctr.Dispose();
            }
        }


        public class QueryStyle
        {
            // 是否加入 maxCount 元素。-1 表示不加入
            public int LimitCount = -1;
            public string OrderInXml = "";
            public string SortInOutputStyle = "";
        }

        // 针对单一数据库的逻辑检索
        public static NormalResult TestSingleDbLogicSearch(
            QueryStyle queryStyle,
            CancellationToken token)
        {
            var channel = DataModel.GetChannel();
            string resultset_name = "default";
            string title = "中国人";
            string author = "王勇";

            string BuildQuery1(string dbname)
            {
                string orderElement = "";
                if (string.IsNullOrEmpty(queryStyle.OrderInXml) == false)
                    orderElement = $"<order>{queryStyle.OrderInXml}</order>";
                return $"<target list='{dbname}:题名'><item><word>{title}</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>{queryStyle.LimitCount}</maxCount>{orderElement}</item><lang>chi</lang></target>";
            }

            string BuildQuery2(string dbname)
            {
                string orderElement = "";
                if (string.IsNullOrEmpty(queryStyle.OrderInXml) == false)
                    orderElement = $"<order>{queryStyle.OrderInXml}</order>";
                return $"<target list='{dbname}:责任者'><item><word>{author}</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>{queryStyle.LimitCount}</maxCount>{orderElement}</item><lang>chi</lang></target>";
            }

            string BuildOutputStyle()
            {
                return $"explain,{queryStyle.SortInOutputStyle}";
            }

            var ctr = token.Register(() =>
            {
                try
                {
                    channel.BeginStop();
                }
                catch
                {

                }
            });

            try
            {
                foreach (var database_name in database_names)
                {
                    token.ThrowIfCancellationRequested();

                    // AND
                    {
                        string query1 = BuildQuery1(database_name);
                        string query2 = BuildQuery2(database_name);
                        string query = "<group>" + query1 + "<operator value='AND'/>" + query2 + "</group>";
                        var ret = channel.DoSearch(query,
                            resultset_name,
                            BuildOutputStyle(),
                            out string explain_info,
                            out string strError);
                        if (ret == -1)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"DoSearch() 出错: {strError}"
                            };
                        DataModel.SetMessage($"逻辑检索 AND 验证成功");
                        DataModel.SetMessage($"---\r\nexplain:\r\n {explain_info}\r\n---", "recpath");
                    }

                    token.ThrowIfCancellationRequested();

                    // OR
                    {
                        string query1 = BuildQuery1(database_name);
                        string query2 = BuildQuery2(database_name);
                        string query = "<group>" + query1 + "<operator value='OR'/>" + query2 + "</group>";
                        var ret = channel.DoSearch(query,
                            resultset_name,
                            BuildOutputStyle(),
                            out string explain_info,
                            out string strError);
                        if (ret == -1)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"DoSearch() 出错: {strError}"
                            };


                        DataModel.SetMessage($"逻辑检索 OR 验证成功");
                        DataModel.SetMessage($"---\r\nexplain:\r\n {explain_info}\r\n---", "recpath");
                    }

                    token.ThrowIfCancellationRequested();

                    // SUB
                    {
                        string query1 = BuildQuery1(database_name);
                        string query2 = BuildQuery2(database_name);
                        string query = "<group>" + query1 + "<operator value='SUB'/>" + query2 + "</group>";
                        var ret = channel.DoSearch(query,
                            resultset_name,
                            BuildOutputStyle(),
                            out string explain_info,
                            out string strError);
                        if (ret == -1)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"DoSearch() 出错: {strError}"
                            };

                        DataModel.SetMessage($"逻辑检索 SUB 验证成功");
                        DataModel.SetMessage($"---\r\nexplain:\r\n {explain_info}\r\n---", "recpath");
                    }

                    token.ThrowIfCancellationRequested();

                    break;
                }

                return new NormalResult();
            }
            finally
            {
                ctr.Dispose();
            }
        }


    }

}