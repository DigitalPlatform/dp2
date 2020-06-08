using System;

namespace GreenInstall
{
    public static class Class1
    {
        public static string Test()
        {
            bool bret = StringUtil0.IsInList("1", "1,3");
            return $"test{bret}";
        }
    }
}
