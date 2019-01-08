using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
   public class MergeFeatrueDetail
    {
        /// <summary>
        /// 铣削特征集合
        /// </summary>
        public List<SwCAM_Mill> swCAM_Mill;
        /// <summary>
        /// 铣削刀轨时间明细集合
        /// </summary>
        public List<ProcessDetail> processDetail;
    }
}
