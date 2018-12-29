using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    /// <summary>
    /// CAMWork 特征类
    /// </summary>
    public class SwCAM_Mill
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
        /// 轴
        /// </summary>
        public double[] Axis { get; set; }
        /// <summary>
        /// 是否抑制
        /// </summary>
        public bool Suppressed { get; set; }
        /// <summary>
        /// 开始位置 x,y,z
        /// </summary>
        public double[] StartPosition { get; set; }
        /// <summary>
        /// 形状(CWShpeType_e)
        /// </summary>
        public int Shape { get; set; }
        /// <summary>
        /// 角
        /// </summary>
        public double Sharp { get; set; }
        /// <summary>
        /// 深度
        /// </summary>
        public double Depth { get; set; }
        /// <summary>
        /// 策略名称
        /// </summary>
        public string Tactics { get; internal set; }
        /// <summary>
        /// 策略ID
        /// </summary>
        public int TacticsID { get; internal set; }
        /// <summary>
        /// 同 策略ID  tacticsID
        /// </summary>
        public int TdbIdForAttribute { get; internal set; }
        /// <summary>
        /// 顶部圆角半径
        /// </summary>
        public double TopFilletRadius { get; internal set; }
        /// <summary>
        /// 直径
        /// </summary>
        public double Maxdiameter { get; internal set; }
        /// <summary>
        /// 特征类型字符串
        /// </summary>
        public string VolumeTypeString { get; internal set; }
        /// <summary>
        /// 精加工半径 mm
        /// </summary>
        public double Mindiameter { get; internal set; }
        /// <summary>
        /// 底部半径 mm
        /// </summary>
        public double BottomRadius { get; set; }
        /// <summary>
        /// 倒角
        /// </summary>
        public double Chamfer { get; set; }
        /// <summary>
        /// 边界框，长,宽,高
        /// </summary>
        public double[] Bound { get; internal set; }
        /// <summary>
        /// 子特征个数
        /// </summary>
        public int SubFeatureCount { get; internal set; }
        /// <summary>
        /// 圆角半径
        /// </summary>
        public double CornerRadius { get; internal set; }
        /// <summary>
        /// 是否为锥形
        /// </summary>
        public bool Taper { get; internal set; }
        /// <summary>
        /// 锥角度
        /// </summary>
        public double TaperAngle { get; internal set; }
        /// <summary>
        /// 特征类型，（枚举CWVolumeType_e）
        /// </summary>
        public int VolumeType { get; internal set; }
        /// <summary>
        /// 子特征
        /// </summary>
        public List<SwCAM_Mill> SubFeatureList { get; internal set; }
        /// <summary>
        /// 多阶 MS孔
        /// </summary>
        public List<SwMultiStep> SubMultiStep { get; set; }

        public double DistanceUptoStock { get; internal set; }
        /// <summary>
        /// 岛屿
        /// </summary>
        public List<Island> Islands { get; internal set; }
        /// <summary>
        /// 岛屿数量
        /// </summary>
        public int IslandCount { get; internal set; }
        /// <summary>
        /// 是否穿过 
        /// </summary>
        public int ThroughOrblind { get; internal set; }
        /// <summary>
        /// 镗削直径
        /// </summary>
        public double BoreDiameter { get; set; }
        /// <summary>
        /// 镗削深度
        /// </summary>
        public double BoreDepth { get; set; }
        /// <summary>
        /// 沉孔直径
        /// </summary>
        public double SinkDiameter { get; set; }
        /// <summary>
        /// 沉孔角度
        /// </summary>
        public double SinkAngle { get; set; }
        /// <summary>
        /// 锥度信息
        /// </summary>
        public TaperInfo Tapers { get; set; }
        public int TDBFeatID { get; internal set; }
    }
}
