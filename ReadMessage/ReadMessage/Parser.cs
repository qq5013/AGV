﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brilliantech.Framwork.Utils.ConvertUtil;
using Brilliantech.Framwork.Utils.LogUtil;

namespace ReadMessage
{
    public class Parser
    {

        public static string readMessage(byte[] MessageBytes)
        {
            int MessageCount = MessageBytes.Count();
            string mean = "";
            if (MessageCount> 9)
            {

                //string name = "FF FF 12 00 01 01 01 01 05 48 65 6C 6C 6F 0A 0B 0B 0B 01  09 08  0A 0B";

                // string name = "FF FF 08 00 01 02 01 01 01  09 08  0A 0B";

                //定义指令头
                string head = "FF FF";
                byte[] heads = ScaleConvertor.HexStringToHexByte(head);
                //定义停止位
                string end = "0A 0B";
                byte[] ends = ScaleConvertor.HexStringToHexByte(end);
                //指令类型          
                string TypeMean = "";
                string MessageId = "第 " + (MessageBytes[3] <<8| MessageBytes[4]) + " 条";
                
                //定义CRC校验
                byte[] CRC = new byte[2] { MessageBytes[MessageCount - 4], MessageBytes[MessageCount - 3] };
                //是否通过CRC校验
                bool CRCpass = ScaleConvertor.IsCrc16Good(CRC);
                string BoxType = "箱型为：";
                string LoadInfo = "装载信息：";
                string CarNr = "小车编号: ";
                string CallingWorkstation = "呼唤工位为: ";
                string StartWorkstation = "出发点的工位编号为： ";
                string LocationNow = "小车当前位置: ";
                string AimedPlace = "小车目标位置: ";
                string speed = "小车速度: ";
                string direction = "小车方向: ";
                string route = "小车路线: ";
                string FailureReason = "失败原因：";
                string LastId = "上一次呼唤的指令Id: ";
                string SuccessOrFailure = "";
                string Ctx = "条码内容为:";
                char CtxContext = new char();
                string StoreNr = "仓库号: ";
                string StoreFloor = "仓库层: " ;
                string StoreColum = "仓库列: ";
                string StoreRow = "仓库排: " ;
                string Priority = "优先级: " ;
                string BarCodeStatus = "条码状态码： ";
                string MachineType = "设备类型编号为: ";
                string MachineId = "设备编号为： ";
                int    BarCodeLength;//条码长度
                string BatchNr = "批次数量: ";
                string WholeDragNR = "整托数量: ";
                string WholeDragNo = "整托第几个：";
                string StartOrStop = "";
                string WarningReason = "警报原因: ";

                //判断指令头和指令我ie
                if (MessageBytes[0] == heads[0] && MessageBytes[1] == heads[1] && MessageBytes[MessageCount - 2] == ends[0] && MessageBytes[MessageCount - 1] == ends[1])
                {

                    switch (MessageBytes[5])
                    {
                        case (byte)01:
                            {

                                TypeMean = "呼唤小车的指令。";
                                

                                CallingWorkstation = CallingWorkstation + MessageBytes[7] + ",";
                                
                                
                                StoreNr    +=  MessageBytes[MessageCount - 9] + ",";
                                StoreFloor +=  MessageBytes[MessageCount - 8] + ",";
                                StoreColum +=  MessageBytes[MessageCount - 7] + ",";
                                StoreRow   +=  MessageBytes[MessageCount - 6] + ",";
                                Priority   +=  MessageBytes[MessageCount - 5] + "。";



                                switch (MessageBytes[6])
                                {
                                    case (byte)01: BoxType += "小箱，"; break;
                                    case (byte)02: BoxType += "大箱，"; break;
                                    default: BoxType += "未知箱型"; break;

                                }
                                for (int i = 1; i <= MessageBytes[8]; i++)
                                {
                                    CtxContext = Convert.ToChar(MessageBytes[8 + i]);
                                    Ctx += CtxContext;

                                }
                                Ctx += ",";
                                mean = MessageId + TypeMean + BoxType + CallingWorkstation + Ctx + StoreNr + StoreFloor + StoreColum + StoreRow + Priority;

                                return mean;

                            }

                        case (byte)02:
                            {
                                TypeMean = "呼唤小车响应";
                               
                                CarNr = CarNr + MessageBytes[7] + "。";
                                switch (MessageBytes[6])
                                {
                                    case (byte)01:
                                        {
                                            SuccessOrFailure = "指令成功,";
                                            mean = MessageId + TypeMean + SuccessOrFailure + CarNr;

                                        }
                                        break;


                                    case (byte)02:
                                        {
                                            SuccessOrFailure = "指令失败，";
                                            switch (MessageBytes[8])
                                            {
                                                case (byte)01: FailureReason += "小车未响应"; break;
                                                case (byte)02: FailureReason += "工位未设置"; break;
                                                case (byte)03: FailureReason += "未知"; break;
                                                default: FailureReason += "未知错误"; break;
                                            }
                                            mean = MessageId + TypeMean + SuccessOrFailure + CarNr + FailureReason;
                                        }
                                        break;
                                    default: SuccessOrFailure = "未知"; break;

                                }
                                return mean;
                            }


                        case (byte)03:
                            {
                                TypeMean = "小车出发指令。";
                                StartWorkstation = StartWorkstation + MessageBytes[6] + ",";
                                CarNr = CarNr + MessageBytes[7] + "。";
                                mean = MessageId + TypeMean + StartWorkstation + CarNr;
                                return mean;
                            }

                        case (byte)04:
                            {
                                TypeMean = "小车出发响应指令。";
                                CarNr = CarNr + MessageBytes[6] + "。";
                                mean = MessageId + TypeMean + CarNr;
                                return mean;
                            }
                        case (byte)05:
                            {
                                TypeMean = "小车状态指令。";
                                CarNr = CarNr + MessageBytes[6] + "。";
                                switch (MessageBytes[7])
                                {
                                    case (byte)01: LoadInfo += "未装箱，"; break;
                                    case (byte)02: LoadInfo += "装满箱，"; break;
                                    case (byte)03: LoadInfo += "装空箱，"; break;
                                    default: LoadInfo += "未知装载信息,"; break;

                                }


                                switch (MessageBytes[8])
                                {
                                    case (byte)01: BoxType += "小箱，"; break;
                                    case (byte)02: BoxType += "大箱，"; break;
                                    default: BoxType += "未知箱型,"; break;

                                }
                                LocationNow += MessageBytes[9]+" , ";
                                AimedPlace += MessageBytes[10] + ", ";
                                speed += MessageBytes[11] + ",";
                                direction += (MessageBytes[12] << 8 | MessageBytes[13]) + ",";
                                route += MessageBytes[14] + "。";
                                mean = MessageId + TypeMean + CarNr + LoadInfo + BoxType + LocationNow + AimedPlace + speed + direction + route;


                                return mean;
                            }

                        case (byte)06:
                            {
                                TypeMean = "小车到达指令。";
                                CallingWorkstation = CallingWorkstation + MessageBytes[6] + ",";
                                CarNr = CarNr + MessageBytes[7] + "。";
                                mean = MessageId + TypeMean + CallingWorkstation + CarNr;
                                return mean;
                            }
                        case (byte)07:
                            {
                                TypeMean = "小车到达响应指令。";
                                CarNr = CarNr + MessageBytes[6] + "。";
                                mean = MessageId + TypeMean + CarNr;
                                return mean;

                            }


                        case (byte)08:
                            {
                                TypeMean = "取消小车呼唤指令。";
                                CallingWorkstation = CallingWorkstation + MessageBytes[6] + ",";
                                CarNr = CarNr + MessageBytes[7] + "。";
                                LastId += MessageBytes[8] + "。";
                                mean= MessageId + TypeMean +CallingWorkstation+ CarNr+LastId;
                                return mean;
                                
                            }
                        case (byte)09:
                            {
                                TypeMean = "取消小车呼唤响应";
                                CarNr = CarNr + MessageBytes[7] + "。";
                                switch (MessageBytes[6])
                                {
                                    case (byte)01:
                                        {
                                            SuccessOrFailure = "指令成功,";
                                            mean = MessageId + TypeMean + SuccessOrFailure + CarNr;

                                        }
                                        break;


                                    case (byte)02:
                                        {
                                            SuccessOrFailure = "指令失败，";
                                            switch (MessageBytes[8])
                                            {
                                                case (byte)01: FailureReason += "小车未响应"; break;
                                                case (byte)02: FailureReason += "工位未设置"; break;
                                                case (byte)255: FailureReason += "未知"; break;
                                                default: FailureReason += "未知错误"; break;
                                            }
                                            mean = MessageId + TypeMean + SuccessOrFailure + CarNr + FailureReason;
                                        }
                                        break;
                                    default: SuccessOrFailure = "未知"; break;

                                }
                                return mean;
                            }
                        case (byte)10:
                            {
                                TypeMean = "获取货物库位信息指令。"; 
                                for (int i = 1; i <= MessageBytes[6]; i++)
                                {
                                    CtxContext = Convert.ToChar(MessageBytes[6 + i]);
                                    Ctx += CtxContext;

                                }
                                Ctx += "。";
                                mean = MessageId + TypeMean + Ctx;
                                return mean;
                            }

                        case 0x0B:
                            {
                                TypeMean = "获出库物库位信息响应指令。 ";

                                switch(MessageBytes[6])
                                {
                                    case (byte)01: BarCodeStatus += "条码不存在，"; break;
                                    case (byte)02: BarCodeStatus += "条码未入库，"; break;
                                    case (byte)03: BarCodeStatus += "条码已入库，"; break;
                                    case (byte)04: BarCodeStatus += "条码已出库，"; break;
                                    case (byte)05: BarCodeStatus += "条码已锁定不可入库，"; break;
                                    case (byte)06: BarCodeStatus += "条码已锁定不可出库，"; break;
                                    default: BarCodeStatus += "未知条码状态, "; break;
                                }

                                switch (MessageBytes[6])
                                {
                                    case (byte)01: BarCodeStatus += "条码不存在，"; break;
                                    case (byte)02: BarCodeStatus += "条码未入库，"; break;
                                    case (byte)03: BarCodeStatus += "条码已入库，"; break;
                                    case (byte)04: BarCodeStatus += "条码已出库，"; break;
                                    case (byte)05: BarCodeStatus += "条码已锁定不可入库，"; break;
                                    case (byte)06: BarCodeStatus += "条码已锁定不可出库，"; break;
                                    default: BarCodeStatus += "未知条码状态, "; break;
                                }
                                switch (MessageBytes[7])
                                {
                                    case (byte)01: BoxType += "小箱，"; break;
                                    case (byte)02: BoxType += "大箱，"; break;
                                    default: BoxType += "未知箱型"; break;

                                }

                                StoreNr += MessageBytes[8] + ", ";
                                StoreFloor += MessageBytes[9] + ", ";
                                StoreColum += MessageBytes[10] + ", ";
                                StoreRow += MessageBytes[11] + ", ";

                                for (int i = 1; i <= MessageBytes[12]; i++)
                                {
                                    CtxContext = Convert.ToChar(MessageBytes[12 + i]);
                                    Ctx += CtxContext;

                                }
                                Ctx += "。";



                                mean = MessageId + TypeMean + BarCodeStatus + BoxType + StoreNr + StoreFloor + StoreColum + StoreRow + Ctx;

                                return mean;
                            }
                        case (byte)12:
                            {
                                TypeMean = "入库操作完成指令。 ";
                               
                                BarCodeLength = MessageBytes[10];
                                MachineType += MessageBytes[6]+", ";
                                MachineId   += (MessageBytes[7] << 8 | MessageBytes[8]) + ", ";


                                switch (MessageBytes[9]) 
                                {
                                    case (byte)01: BoxType += "小箱，"; break;
                                    case (byte)02: BoxType += "大箱，"; break;
                                    default: BoxType += "未知箱型"; break;

                                }

                                for (int i = 1; i <= BarCodeLength; i++)
                                {
                                   
                                    CtxContext = Convert.ToChar(MessageBytes[10 + i]);
                                    Ctx += CtxContext;

                                }
                                Ctx += ",";



                                switch (MessageBytes[MessageCount-6])
                                //switch (MessageBytes[11+MessageBytes[10]])
                                {
                                    case (byte)01:
                                        {
                                            SuccessOrFailure = "入库成功,";
                                            mean = MessageId + TypeMean + SuccessOrFailure + MachineType + MachineId + BoxType + Ctx;

                                        }
                                        break;


                                    case (byte)02:
                                        {
                                            SuccessOrFailure = "入库失败，";
                                            switch (MessageBytes[MessageCount-5])
                                            {
                                                case (byte)01: FailureReason += "目标库位已被占用"; break;
                                                case (byte)02: FailureReason += "库位错误"; break;
                                                case (byte)03: FailureReason += "设备故障"; break;
                                                default: FailureReason += "未知错误"; break;
                                            }
                                            mean = MessageId + TypeMean + SuccessOrFailure + MachineType + MachineId + BoxType + Ctx + FailureReason;
                                        }
                                        break;
                                    default: SuccessOrFailure = "未知"; break;

                                }
                                return mean;

                            }
                        case (byte)13:
                            {
                                TypeMean = "入库操作完成响应指令。 ";
                                MachineType += MessageBytes[6] + ", ";
                                MachineId += (MessageBytes[7] << 8 | MessageBytes[8]) + "。 ";
                                mean = MessageId + TypeMean + MachineType + MachineId;
                                return mean;


                            }
                        case 0x0E: //14
                            {
                                TypeMean = "出库指令。 ";
                                
                                switch (MessageBytes[6])
                                {
                                    case (byte)01: BoxType += "小箱，"; break;
                                    case (byte)02: BoxType += "大箱，"; break;
                                    default: BoxType += "未知箱型"; break;

                                }
                                BatchNr += MessageBytes[7]+"，";
                                WholeDragNR += MessageBytes[8] + "，";
                                WholeDragNo += MessageBytes[9] + ", ";
                                BarCodeLength = MessageBytes[14];

                                StoreNr += MessageBytes[10] + ", ";
                                StoreFloor += MessageBytes[11] + ", ";
                                StoreColum += MessageBytes[12] + ", ";
                                StoreRow += MessageBytes[13] + ", ";


                                for (int i = 1; i <= BarCodeLength; i++)
                                {

                                    CtxContext = Convert.ToChar(MessageBytes[14 + i]);
                                    Ctx += CtxContext;

                                }
                                Ctx += "。";
                             
                                mean = MessageId + TypeMean + BoxType + BatchNr + WholeDragNR +WholeDragNo+ StoreNr + StoreFloor + StoreColum + StoreRow + Ctx;
                                return mean;
                            }
                        case (byte)15:
                            {
                                TypeMean = "出库响应指令。 "; 
                                switch (MessageBytes[6])
                               
                                {
                                    case (byte)01:
                                        {
                                            SuccessOrFailure = "出库成功, ";
                                            mean = MessageId + TypeMean + SuccessOrFailure ;

                                        }
                                        break;


                                    case (byte)02:
                                        {
                                            SuccessOrFailure = "出库失败，";
                                            switch (MessageBytes[MessageCount - 5])
                                            {
                                                case (byte)01: FailureReason += "待定。"; break;
                                                case (byte)02: FailureReason += "待定。"; break;
                                                case (byte)03: FailureReason += "待定。"; break;
                                                default: FailureReason += "待定。"; break;
                                            }
                                            mean = MessageId + TypeMean + SuccessOrFailure + FailureReason;
                                        }
                                        break;
                                    default: SuccessOrFailure = "未知"; break;

                                }
                                return mean;

                            }
                        case (byte)16:
                            {
                                TypeMean = "出库操作完成指令。 ";
                                MachineType += MessageBytes[6] + ", ";
                                MachineId += (MessageBytes[7] << 8 | MessageBytes[8]) + ", ";
                                BarCodeLength = MessageBytes[10];

                                switch (MessageBytes[9])
                                {
                                    case (byte)01: BoxType += "小箱，"; break;
                                    case (byte)02: BoxType += "大箱，"; break;
                                    default: BoxType += "未知箱型"; break;

                                }

                                for (int i = 1; i <= BarCodeLength; i++)
                                {

                                    CtxContext = Convert.ToChar(MessageBytes[10 + i]);
                                    Ctx += CtxContext;

                                }
                                Ctx += ",";



                                switch (MessageBytes[MessageCount - 6])
                                //switch (MessageBytes[11+MessageBytes[10]])
                                {
                                    case (byte)01:
                                        {
                                            SuccessOrFailure = "出库成功,";
                                            mean = MessageId + TypeMean + SuccessOrFailure +  MachineType + MachineId + BoxType + Ctx;

                                        }
                                        break;


                                    case (byte)02:
                                        {
                                            SuccessOrFailure = "出库失败，";
                                            switch (MessageBytes[MessageCount - 5])
                                            {
                                                case (byte)01: FailureReason += "目标库位不存在货物"; break;
                                                case (byte)02: FailureReason += "出库条码和在库条码内容不匹配"; break;
                                                case (byte)03: FailureReason += "库位错误"; break;
                                                case (byte)04: FailureReason += "设备故障"; break;
                                                default: FailureReason += "未知错误"; break;
                                            }
                                            mean = MessageId + TypeMean + SuccessOrFailure + MachineType + MachineId + BoxType + Ctx + FailureReason;
                                        }
                                        break;
                                    default: SuccessOrFailure = "未知"; break;

                                }
                                return mean;


                            }

                        case (byte)17:
                            {
                                TypeMean = "出库操作完成响应";
                                MachineType += MessageBytes[6] + ", ";
                                MachineId += (MessageBytes[7] << 8 | MessageBytes[8]) + "。 ";
                                mean = MessageId + TypeMean + MachineType + MachineId;
                                return mean;

                            }
                        case (byte)18:
                            {
                                
                                MachineType += MessageBytes[6];

                                TypeMean = "码垛（整个托盘）";
                                MachineId += (MessageBytes[7] << 8 | MessageBytes[8]) + "。 ";
                                switch (MessageBytes[9])
                                {
                                    case (byte)01: BoxType += "小箱，"; break;
                                    case (byte)02: BoxType += "大箱，"; break;
                                    default: BoxType += "未知箱型"; break;

                                }
                                WholeDragNR += MessageBytes[10] + "，";
                                switch (MessageBytes[MessageCount - 6])
                                //switch (MessageBytes[11+MessageBytes[10]])
                                {
                                    case (byte)01:
                                        {
                                            SuccessOrFailure = "成功,";
                                            mean = MessageId + TypeMean + SuccessOrFailure + MachineId + BoxType + WholeDragNR;

                                        }
                                        break;


                                    case (byte)02:
                                        {
                                            SuccessOrFailure = "失败，";
                                            switch (MessageBytes[MessageCount - 5])
                                            {
                                                case (byte)01: FailureReason += "待定"; break;
                                                case (byte)02: FailureReason += "待定"; break;
                                                case (byte)03: FailureReason += "待定"; break;
                                                case (byte)04: FailureReason += "待定"; break;
                                                default: FailureReason += "未知错误"; break;
                                            }
                                            mean = MessageId + TypeMean + SuccessOrFailure + MachineType+ MachineId + BoxType + WholeDragNR + FailureReason;
                                        }
                                        break;
                                    default: SuccessOrFailure = "未知"; break;

                                }
                                return mean;

                            }
                        case (byte)19:
                            {
                                TypeMean = "码垛（整个托盘）完成响应指令。 ";
                                MachineType += MessageBytes[6];
                                MachineId += (MessageBytes[7] << 8 | MessageBytes[8]) + "。 ";
                                mean = MessageId + TypeMean + MachineType + MachineId;
                                return mean;

                            }

                        case (byte)20:
                            {
                                TypeMean = "请求启动或停止设备指令。 ";
                                MachineType += MessageBytes[6];
                                MachineId += (MessageBytes[7] << 8 | MessageBytes[8]) + "。 ";
                                
                                switch(MessageBytes[9])
                                {
                                    case (byte)01:
                                        {
                                            StartOrStop += "启动设备请求， ";
                                            break;

                                        }
                                    case (byte)02:
                                        {
                                            StartOrStop += "停止设备请求， ";
                                            break;
                                        }
                                                                               
                                }
                                mean = MessageId +TypeMean+ StartOrStop + MachineType + MachineId;
                                return mean;


                            }
                        case (byte)21:
                            {
                                TypeMean = "请求启动或停止设备响应指令。 ";
                                MachineType += MessageBytes[6];
                                MachineId += (MessageBytes[7] << 8 | MessageBytes[8]) + "。 ";

                                switch (MessageBytes[9])
                                {
                                    case (byte)01:
                                        {
                                            StartOrStop += "启动设备请求， ";
                                            break;

                                        }
                                    case (byte)02:
                                        {
                                            StartOrStop += "停止设备请求， ";
                                            break;
                                        }

                                }
                                mean = MessageId +TypeMean+ StartOrStop + MachineType + MachineId;
                                return mean;
                                
                            }
                        case (byte)22:
                            {
                                TypeMean = "启动或停止设备指令。 ";
                                MachineType += MessageBytes[6];
                                MachineId += (MessageBytes[7] << 8 | MessageBytes[8]) + "。 ";

                                switch (MessageBytes[9])
                                {
                                    case (byte)01:
                                        {
                                            StartOrStop += "启动设备， ";
                                            break;

                                        }
                                    case (byte)02:
                                        {
                                            StartOrStop += "停止设备， ";
                                            break;
                                        }

                                }
                                mean = MessageId + TypeMean+ StartOrStop + MachineType + MachineId;
                                return mean;

                            }
                        case (byte)23:
                            {
                                TypeMean = "启动或停止设备响应指令。 ";
                                MachineType += MessageBytes[6];
                                MachineId += (MessageBytes[7] << 8 | MessageBytes[8]) + "。 ";

                                switch (MessageBytes[9])
                                {
                                    case (byte)01:
                                        {
                                            StartOrStop += "启动设备， ";
                                            break;

                                        }
                                    case (byte)02:
                                        {
                                            StartOrStop += "停止设备";
                                            break;
                                        }
                                    default:StartOrStop += "未知错误";break;

                                }
                                switch (MessageBytes[MessageCount - 6])
                                {
                                    case (byte)01:
                                        {
                                            SuccessOrFailure = "成功,";
                                            mean = MessageId + TypeMean +StartOrStop+SuccessOrFailure + MachineType + MachineId;
                                            break;
                                        }
                                    case (byte)02:
                                        {
                                            SuccessOrFailure = "失败，";
                                            switch (MessageBytes[MessageCount - 5])
                                            {
                                                case (byte)01: FailureReason += "待定"; break;
                                                case (byte)02: FailureReason += "待定"; break;
                                                case (byte)03: FailureReason += "待定"; break;
                                                default: FailureReason += "未知错误"; break;
                                            }
                                            mean = MessageId + TypeMean + StartOrStop+ SuccessOrFailure + MachineType + MachineId + FailureReason;
                                            break;
                                        }
                                    default: SuccessOrFailure = "未知"; break;
                                }
                               
                                return mean;


                            }
                        case (byte)24:
                            {
                                TypeMean = "警报";
                                MachineType += MessageBytes[6];
                                MachineId += (MessageBytes[7] << 8 | MessageBytes[8]) + "。 ";

                                switch (MessageBytes[9])
                                {
                                    case (byte)01: WarningReason += "运行故障"; break;
                                    case (byte)02: WarningReason += "急停故障"; break;
                                    case (byte)03: WarningReason += "通信故障"; break;
                                    case (byte)04: WarningReason += "夹具故障"; break;
                                    case (byte)05: WarningReason += "低电"; break;
                                    case (byte)06: WarningReason += "危险区有人进入"; break;
                                    case (byte)07: WarningReason += "货物坠落"; break;
                                    default: WarningReason += "待定"; break;
                                }
                                mean = MessageId + TypeMean +  MachineType + MachineId+WarningReason;
                                return mean;
                            }

                        case (byte)25:
                            {
                                TypeMean = "警报响应";
                                MachineType += MessageBytes[6];
                                MachineId += (MessageBytes[7] << 8 | MessageBytes[8]) + "。 ";
                                switch (MessageBytes[9])
                                {
                                    case (byte)01:
                                        {
                                            StartOrStop += "警报启动";
                                            break;

                                        }
                                    case (byte)02:
                                        {
                                            StartOrStop += "警报停止";
                                            break;
                                        }
                                    default: StartOrStop += "未知错误, ";break;
                                        
                                }
                                switch (MessageBytes[MessageCount-6])
                                {
                                    case (byte)01:
                                        {
                                            SuccessOrFailure = "指令成功,";
                                            mean = MessageId + TypeMean + SuccessOrFailure + MachineType + MachineId;
                                            break;
                                        }
                                    case (byte)02:
                                        {
                                            SuccessOrFailure = "指令失败，";
                                            switch (MessageBytes[MessageCount-5])
                                            {
                                                case (byte)01: FailureReason += "待定"; break;
                                                case (byte)02: FailureReason += "待定"; break;
                                                case (byte)03: FailureReason += "待定"; break;
                                                default: FailureReason += "未知错误"; break;
                                            }
                                            mean = MessageId + TypeMean + SuccessOrFailure + MachineType + MachineId + FailureReason;
                                            break;
                                        }
                                    default: SuccessOrFailure = "未知"; break;
                                }
                                return mean;

                            }
                        default: TypeMean = "未知类型"; break;
                       
                    }

                }

                else
                {
                    mean = ("指令格式有误");

                }

               
            }
            else
            {
                mean = ("指令格式有误");
            }

            LogUtil.Logger.Info("【消息解析】"+mean);
            return mean;

        }
    }
}
