using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalPlatform.rms
{
    //自定义的异常类型
    //ReversePolishStack栈Pop()或Seek()或SeekType()时可能会抛出此异常
    public class StackUnderflowException : Exception
    {
        //构造函数
        //strEx: 信息
        public StackUnderflowException(string strEx)
            : base(strEx)
        { }
    }
    //自定义的异常类型:检索时类型不区配抛出的异常
    public class NoMatchException : Exception
    {
        //构造函数
        public NoMatchException(string strEx)
            : base(strEx)
        { }
    }
}
