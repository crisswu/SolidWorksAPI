using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    /// <summary>
    /// 
    /// </summary>
    interface ICutte
    {
        /// <summary>
        /// 走刀次数
        /// </summary>
        int CutterCount { get; set; }
        /// <summary>
        /// 深度
        /// </summary>
         double Depth { get; set; }
        /// <summary>
        /// 裁剪次数 （深度/刀具直径*0.25）
        /// </summary>
        /// <returns></returns>
        int NumberOfWalkCut();
    }
}
