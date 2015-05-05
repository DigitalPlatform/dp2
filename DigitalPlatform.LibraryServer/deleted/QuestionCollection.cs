using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DigitalPlatform.LibraryServer
{
#if NO
    /// <summary>
    /// Summary description for QuestionCollection.
    /// </summary>
    public class QuestionCollection : ArrayList
    {
        public string Name = "";

        public override void Clear()
        {
            Name = "";
            base.Clear();
        }

        public Question GetQuestion(int index)
        {
            if (index >= this.Count)
                return null;
            return (Question)this[index];
        }

        public Question NewQuestion(int index,
            string strText)
        {
            Question result = null;



            for (; ; )
            {
                if (index >= this.Count)
                {
                    result = new Question();
                    this.Add(result);
                }
                else
                    break;
            }

            if (index < this.Count)
            {
                result = (Question)this[index];
                result.Text = strText;
                result.Answer = "";
                return result;
            }

            Debug.Assert(false, "");	// 不可能走到这里
            return null;
        }
    }


    public class Question
    {
        public string Text = "";	// 问题正文
        public string Answer = "";	// 问题答案

    }
#endif
}
