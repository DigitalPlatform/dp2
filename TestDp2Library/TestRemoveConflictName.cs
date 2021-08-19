using DigitalPlatform.LibraryServer;
using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestDp2Library
{
    public class TestRemoveConflictName
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("name", "name")]
        [InlineData("?name,name", "name")]
        [InlineData("?name,,name", ",name")]
        [InlineData("?name,1,name", "1,name")]
        [InlineData("1,?name,2,name", "1,2,name")]
        [InlineData("1,?name,2,name,3", "1,2,name,3")]
        [InlineData("?name", "?name")]
        [InlineData("?name,1", "?name,1")]
        [InlineData("1,?name,2", "1,?name,2")]
        [InlineData("libraryCode,readerType,barcode,cardNumber,refID,oi,info,borrows,overdues,reservations,outofReservations,state,createDate,expireDate,name,namePinyin,displayName,gender,nation,comment,department,post,address,tel,email,rights,access,personalLibrary,friends,idCardNumber,dateOfBirth,borrowHistory,preference,hire,foregift,fingerprint,palmprint,face,http://dp2003.com/dprms:file,?name,?tel,?department,r_delete", 
            "libraryCode,readerType,barcode,cardNumber,refID,oi,info,borrows,overdues,reservations,outofReservations,state,createDate,expireDate,namePinyin,displayName,gender,nation,comment,post,address,email,rights,access,personalLibrary,friends,idCardNumber,dateOfBirth,borrowHistory,preference,hire,foregift,fingerprint,palmprint,face,http://dp2003.com/dprms:file,?name,?tel,?department,r_delete")]
        public void TestRemoveConflictName_01(string input, string result)
        {
            var input_list = StringUtil.SplitList(input);
            var result_list = LibraryApplication.RemoveConflictName(input_list);
            Assert.Equal(result, StringUtil.MakePathList(result_list));
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("name", "name")]
        [InlineData("name,?name", "?name")]
        [InlineData("name,,?name", ",?name")]
        [InlineData("name,1,?name", "1,?name")]
        [InlineData("1,name,2,?name", "1,2,?name")]
        [InlineData("1,name,2,?name,3", "1,2,?name,3")]
        [InlineData("name,1", "name,1")]
        [InlineData("1,name,2", "1,name,2")]
        public void TestRemoveConflictName_02(string input, string result)
        {
            var input_list = StringUtil.SplitList(input);
            var result_list = LibraryApplication.RemoveConflictName(input_list);
            Assert.Equal(result, StringUtil.MakePathList(result_list));
        }
    }
}
