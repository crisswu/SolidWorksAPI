using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    public class SwCAM_Turn
    {
        /// <summary>
        /// 特征名称
        /// </summary>
         public string FeatureName { get; set; }
        /// <summary>
        /// 特征类型
        /// </summary>
        public int FeatureType { get; set; }
        /// <summary>
        /// 最大直径
        /// </summary>
        public double MaxDiameter { get; internal set; }
        /// <summary>
        /// 最小直径
        /// </summary>
        public double MinDiameter { get; internal set; }
        /// <summary>
        /// 精加工半径 mm
        /// </summary>
        public double Mindiameter { get; internal set; }
        /// <summary>
        /// 长度
        /// </summary>
        public double Length { get; set; }
        /// <summary>
        /// 最大后角
        /// </summary>
        public double MaxReliefAngle { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }
    }
}
