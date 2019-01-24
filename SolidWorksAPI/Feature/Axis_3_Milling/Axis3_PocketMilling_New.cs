using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    /// <summary>
    /// 3轴 -- 矩形槽,腔 （自更改裁剪长度算法）
    /// </summary>
    public class Axis3_PocketMilling_New : Axis3Milling,ICutte
    {
        /// <summary>
        /// 长度
        /// </summary>
        private double Length { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        private double Width { get; set; }
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

        public Axis3_PocketMilling_New(double Dia,double Length,double Width,double Depth, int NoOfPlaces, Materials _Materials)
        {
            this.Dia = Dia;
            this.Depth = Depth;
            this.Length = Length;
            this.Width = Width;
            this.NoOfPlaces = NoOfPlaces;
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
            // (Length - Dia)= 长度，两边刀具半径+起来就是1个刀具直径，
            //this.Width / (this.Dia * 0.9) = 计算重叠率，保留刀具直径 10%，
            this.CuttingLength =  (Length - Dia) * Math.Ceiling(this.Width / (this.Dia * 0.7)) * NumberOfWalkCut();
        }
        /// <summary>
        /// 裁剪时间
        /// </summary>
        protected override void Calculate_CuttingTime()
        {
            this.CuttingTime = (this.CuttingLength + this.ReserveLength) * this.NoOfPlaces * 60 / this.FeedRate;
        }
        /// <summary>
        /// 计算 进给速率
        /// </summary>
        protected override void Calculate_FeedRate()
        {
            //进给率根据 刀具的直径 给出 进给率
            Cutter_Drill.Pocket_FeedRate(this.Dia);
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
        protected override int GetCuttingSpeed()
        {
            switch (this._Materials)
            {
                case Materials.Carbon:
                    return 120;
                case Materials.Alloy:
                    return 100;
                case Materials.Stainless:
                    return 80;
                case Materials.Aluminum:
                    return 200;
                default:
                    return 200;
            }
        }
    }
}
