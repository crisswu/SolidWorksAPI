using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    /// <summary>
    /// 锥度信息
    /// </summary>
    public class TaperInfo
    {
        /// <summary>
        /// 锥度-角度
        /// </summary>
        public double TaperAngle { get; set; }
        /// <summary>
        /// 锥度-高
        /// </summary>
        public double TaperHeight { get; set; }
        /// <summary>
        /// 顶部倒角-角度
        /// </summary>
        public double TopChamferAngle { get; set; }
        /// <summary>
        /// 顶部倒角-高
        /// </summary>
        public double TopChamferHeight { get; set; }
        /// <summary>
        /// 底部圆弧过度面-半径
        /// </summary>
        public double BottomArcFaceRadius { get; set; }
        /// <summary>
        /// 底部圆弧过度面-高
        /// </summary>
        public double BottomArcFaceHeight { get; set; }
    }

    /// <summary>
    /// 材料
    /// </summary>
    public enum Materials
    {
        /// <summary>
        /// 铝
        /// </summary>
        Aluminum = 0,
        /// <summary>
        /// 铜
        /// </summary>
        Copper = 1,
        /// <summary>
        /// 合金钢
        /// </summary>
        Alloy = 2,
        /// <summary>
        /// 碳钢
        /// </summary>
        Carbon = 3,
        /// <summary>
        /// 不锈钢
        /// </summary>
        Stainless = 4,
        /// <summary>
        /// 塑料
        /// </summary>
        Plastic = 5
    }

}
