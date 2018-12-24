using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    /// <summary>
    /// 阶梯参数
    /// </summary>
    public class SwMultiStep
    {
        /// <summary>
        /// 阶梯类型  1圆柱，2倒角，3圆弧过度
        /// </summary>
        public int MultiSetpType { get; set; }
        /// <summary>
        /// 枚举字符串
        /// </summary>
        public string MultiSetpName { get; set; }
        /// <summary>
        /// 顶部直径
        /// </summary>
        public double TopDiameter { get; set; }
        /// <summary>
        /// 螺纹内径
        /// </summary>
        public double MinorDiameter { get; set; }
        /// <summary>
        /// 直径
        /// </summary>
        public double Diameter { get; set; }
        /// <summary>
        /// 距离
        /// </summary>
        public double Distance { get; set; }
        /// <summary>
        /// 深度
        /// </summary>
        public double Depth { get; set; }
        /// <summary>
        /// 角
        /// </summary>
        public double Angle { get; set; }
    }
}
