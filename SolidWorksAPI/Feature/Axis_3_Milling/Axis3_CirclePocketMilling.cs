using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    /// <summary>
    /// 3轴 -- 圆形凹腔
    /// </summary>
    public class Axis3_CirclePocketMilling : Axis3Milling,ICutte
    {
        /// <summary>
        /// 特征直径
        /// </summary>
        private double FeatureDia { get; set; }
        /// <summary>
        /// 深度
        /// </summary>
        public double Depth { get; set; }
        /// <summary>
        /// 裁剪长度
        /// </summary>
        public double CuttingLength { get; set; }
        /// <summary>
        /// 走刀次数
        /// </summary>
        public int CutterCount { get; set; }

        public Axis3_CirclePocketMilling(double Dia,double FeatureDia,double Depth, Materials _Materials)
        {
            this.Dia = Dia;
            this.Depth = Depth;
            this.FeatureDia = FeatureDia;
            this.NoOfPlaces = 1;
            this.No = ChangeNo(); 
            this.FeedPer = 0.06;
            this.ReserveLength = 2;
            this._Materials = _Materials;
            this.CuttingSpeed = GetCuttingSpeed();
            Calculate_SpindleSpeed();
            Calculate_FeedRate();
            Calculate_CuttingLength();
            Calculate_CuttingTime();
            Calculate_TotalTime();
        }
        /// <summary>
        /// 裁剪次数 （深度/刀具直径*0.25）
        /// </summary>
        /// <returns></returns>
        public int NumberOfWalkCut()
        {
            double cutTools = this.GetDepthOfCut();//每次的切割深度
            double sumWalk = Depth / cutTools;//换算出 总共需要走多少次
             CutterCount = Convert.ToInt32(Math.Ceiling(sumWalk))+1 ;//获取最大整数  例： 3.1 = 4
            return CutterCount;
        }
        /// <summary>
        /// 根据刀具直径给定齿数
        /// </summary>
        /// <returns></returns>
        public int ChangeNo()
        {
            if (this.Dia >= 10)
                return 4;
            else
                return 2;
        }
        /// <summary>
        /// 裁剪长度
        /// </summary>
        protected void Calculate_CuttingLength()
        {
            this.CuttingLength = Recursion(this.FeatureDia,0) * NumberOfWalkCut();
        }

        /// <summary>
        /// 递归计算切割长度
        /// </summary>
        /// <param name="resDia">剩余直径</param>
        /// <param name="cuttingLength">已切割的长度</param>
        /// <returns></returns>
        private double Recursion(double resDia,double cuttingLength)
        {
            if (resDia <= this.Dia) // 如果特征直径 小于刀具直径 则计算最后一次 后返回总长度
            {
                if (resDia > (Dia / 2))
                {
                    cuttingLength += resDia - (Dia / 2) * 3.14;// 外直径 减掉 刀具半径 计算周长
                    return cuttingLength;
                }
                else
                    return cuttingLength;
            }
            else
            {
                cuttingLength += resDia - (Dia / 2) * 3.14;// 外直径 减掉 刀具半径 计算周长
                resDia = resDia - Dia * 0.7;//每次 直径缩小至 刀具 直径的70% 为重叠率
                return Recursion(resDia, cuttingLength);
            }

        }
        /// <summary>
        /// 裁剪时间
        /// </summary>
        protected override void Calculate_CuttingTime()
        {
            this.CuttingTime = this.CuttingLength * 60 / (this.FeedRate * this.CuttingSpeed);
        }
        /// <summary>
        /// 计算 进给速率
        /// </summary>
        protected override void Calculate_FeedRate()
        {
            //进给率根据 刀具的直径 给出 进给率
           this.FeedRate = Cutter_Drill.Pocket_FeedRate(this.Dia);
        }
        /// <summary>
        /// 计算 主轴转速
        /// </summary>
        protected override void Calculate_SpindleSpeed()
        {
            if ((this.CuttingSpeed * 1000 / (this.Dia * 3.14) - 6500) >= 0)
            {
                this.SpindleSpeed = 6500;
            }
            else
            {
                this.SpindleSpeed = this.CuttingSpeed * 1000 / (this.Dia * 3.14);
            }
        }

        protected override void Calculate_TotalTime()
        {
            this.TotalTime = this.AtcTime + this.OtherTime + this.CuttingTime;
        }
        /// <summary>
        /// 获取材料切割速度
        /// </summary>
        /// <returns></returns>
        protected override double GetCuttingSpeed()
        {
            switch (this._Materials)
            {
                case Materials.Carbon:
                    return 0.6;
                case Materials.Alloy:
                    return 0.5;
                case Materials.Stainless:
                    return 0.4;
                case Materials.Aluminum:
                    return 1;
                default:
                    return 1;
            }
        }
    }
}
