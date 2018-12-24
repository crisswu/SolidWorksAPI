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
}
