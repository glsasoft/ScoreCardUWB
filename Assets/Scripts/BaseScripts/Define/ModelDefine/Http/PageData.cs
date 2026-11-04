using System.Collections.Generic;

namespace LetsPiu.Common.Network.Model.Http
{
    /// <summary>
    /// 分页数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageData<T>
    {
        public List<T> records;
        public int total;
        public int size;
        public int current;
        public int pages;
    }
}