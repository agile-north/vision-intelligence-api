using System.Collections;
using System.Collections.Generic;

namespace Contracts
{



    public class AnyyOrCriteria<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public bool Any { get; set; } = false;
    }


}