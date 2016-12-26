﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AGVCenterLib.Data;
using AGVCenterLib.Enum;
using AGVCenterLib.Model;
using AGVCenterLib.Model.OPC;
using AGVCenterLib.Service;
using AGVCenterWPF.Config;
using Brilliantech.Framwork.Utils.LogUtil;
using OPCAutomation;

namespace AGVCenterWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        #region 服务变量
        OPCServer AnOPCServer;
        OPCServer ConnectedOPCServer;
        #endregion

        #region
        /// AGV扫描任务队列
        // 显示
        List<StockTaskItem> TaskCenterForDisplayQueue;
        // 任务
        Dictionary<string, StockTaskItem> AgvScanTaskQueue;
        private object WriteTaskCenterQueueLocker = new object();
        private object AgvScanTaskQueueLocker = new object();

        //小车放行队列
        Queue AgvInStockPassQueue;
        

        #endregion
        #region 入库信息数据 OPC

        // 入库条码扫描
        OPCCheckInStockBarcode OPCCheckInStockBarcodeData;
        OPCGroup OPCCheckInstockBarcodeOPCGroup;
        // 小车入库放行
        OPCAgvInStockPass OPCAgvInStockPassData;
        OPCGroup OPCAgvInStockPassOPCGroup;
        // 入库机械手抓取
        OPCInRobootPick OPCInRobootPickData;
        OPCGroup OPCInRobootPickOPCGroup;

        // 库存任务, 巷道机1&2
        // 1
        OPCSetStockTask OPCSetStockTaskRoadMachine1Data;
        OPCGroup OPCSetStockTaskRoadMachine1OPCGroup;
        // 2
        OPCSetStockTask OPCSetStockTaskRoadMachine2Data;
        OPCGroup OPCSetStockTaskRoadMachine2OPCGroup;

        // 巷道机任务反馈，巷道机1&2
        // 1
        OPCRoadMachineTaskFeed OPCRoadMachine1TaskFeedData;
        OPCGroup OPCRoadMachine1TaskFeedOPCGroup;
        // 2
        OPCRoadMachineTaskFeed OPCRoadMachine2TaskFeedData;
        OPCGroup OPCRoadMachine2TaskFeedOPCGroup;


        // 出库机械手码垛
        OPCOutRobootPick OPCOutRobootPickData;
        OPCGroup OPCOutRobootPickOPCGroup;


        OPCDataReset OPCDataResetData;
        OPCGroup OPCDataResetOPCGroup;

        #endregion

        #region 监控定时器
        /// <summary>
        /// 写入OPC小车放行定时器，将队列AgvInStockPassQueue的任务写入OPC
        /// </summary>
        System.Timers.Timer SetOPCAgvPassTimer;

        /// <summary>
        /// 写入OPC入库机械手信息，将对列InRobootPickQueue的任务写入OPC
        /// </summary>
        System.Timers.Timer SetOPCInRobootPickTimer;


        /// <summary>
        /// 逐托分发出库任务定时器
        /// </summary>
        System.Timers.Timer DispatchTrayOutStockTaskTimer;
        private object DispatchTrayOutStockTaskLocker = new object();

        /// <summary>
        /// 写入OPC库存任务定时器
        /// </summary>
        System.Timers.Timer SetOPCStockTaskTimer;
        #endregion

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            this.InitOPC();
            // 自动连接OPC
            if (BaseConfig.AutoConnectOPC)
            {
                this.ConnectOPC();
            }
            #region 加载初始化数据
            // 初始化并从数据库加载任务
            this.InitQueueAndLoadTaskFromDb();
            #endregion


            this.InitAndStartUpdateStackTaskStateComponent();

            Thread.Sleep(2000);


            this.InitAndStartTimers();
        }


        /// <summary>
        /// 初始化并启动定时器
        /// </summary>
        private void InitAndStartTimers()
        {
            #region 初始化定时器
            // AGV入库放行Timer
            SetOPCAgvPassTimer = new System.Timers.Timer();
            SetOPCAgvPassTimer.Interval = OPCConfig.SetOPCAgvInStockPassTimerInterval;
            SetOPCAgvPassTimer.Enabled = true;
            SetOPCAgvPassTimer.Elapsed += SetOPCAgvPassTimer_Elapsed;

            // 入库机械手Timer
            SetOPCInRobootPickTimer = new System.Timers.Timer();
            SetOPCInRobootPickTimer.Interval = OPCConfig.SetOPCInRobootPickTimerInterval;
            SetOPCInRobootPickTimer.Enabled = true;
            SetOPCInRobootPickTimer.Elapsed += SetOPCInRobootPickTimer_Elapsed;

            // 逐托分发出库任务定时器，查看是可以分发
            DispatchTrayOutStockTaskTimer = new System.Timers.Timer();
            DispatchTrayOutStockTaskTimer.Interval = OPCConfig.DispatchTrayOutStockTaskTimerInterval;
            DispatchTrayOutStockTaskTimer.Enabled = true;
            DispatchTrayOutStockTaskTimer.Elapsed += DispatchTrayOutStockTaskTimer_Elapsed;

            // 库存任务定时器，查看巷道机是否可以工作
            SetOPCStockTaskTimer = new System.Timers.Timer();
            SetOPCStockTaskTimer.Interval = OPCConfig.SetOPCStockTaskTimerInterval;
            SetOPCStockTaskTimer.Enabled = true;
            SetOPCStockTaskTimer.Elapsed += SetOPCStockTaskTimer_Elapsed;

            /// 启动定时器
            LogUtil.Logger.Info("【启动SetOPCAgvPassTimer定时器】");
            SetOPCAgvPassTimer.Start();
            LogUtil.Logger.Info("【启动SetOPCInRobootPickTimer定时器】");
            SetOPCInRobootPickTimer.Start();

            LogUtil.Logger.Info("【启动DispatchTrayOutStockTaskTimer定时器】");
            DispatchTrayOutStockTaskTimer.Start();

            LogUtil.Logger.Info("【启动SetOPCStockTaskTimer定时器】");
            SetOPCStockTaskTimer.Start();
            #endregion
        }


        /// <summary>
        /// 读取AGV放行队列中的任务，写入OPC值并可读
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetOPCAgvPassTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SetOPCAgvPassTimer.Stop();

            if(AgvInStockPassQueue.Count > 0)
            {
                StockTaskItem taskItem = AgvInStockPassQueue.Peek() as StockTaskItem;
                if (taskItem.IsCanceled)
                {
                    AgvInStockPassQueue.Dequeue();
                }
            }

            if (AgvInStockPassQueue.Count > 0 && OPCAgvInStockPassData.CanWrite)
            {
                StockTaskItem taskItem = AgvInStockPassQueue.Peek() as StockTaskItem;
                if (taskItem.IsCanceled)
                {
                    AgvInStockPassQueue.Dequeue();
                }
                else
                {
                    OPCAgvInStockPassData.AgvPassFlag = taskItem.AgvPassFlag;
                    if (taskItem.AgvPassFlag == (byte)AgvPassFlag.Pass)
                    {
                        taskItem.State = StockTaskState.AgvInStcoking;
                    }
                    else
                    {
                        taskItem.State = StockTaskState.AgvPassFail;
                    }

                    this.UpdateDbTask(taskItem);

                    if (OPCAgvInStockPassData.SyncWrite(OPCAgvInStockPassOPCGroup))
                    {
                        // 进入机械手抓取
                        if (taskItem.AgvPassFlag == (byte)AgvPassFlag.Pass)
                        {
                            /// 进入机械手队列
                            //InRobootPickQueue.Enqueue(AgvInStockPassQueue.Dequeue());
                            AgvInStockPassQueue.Dequeue();
                            /// 从AGV扫描任务中移除
                            this.RemoveTaskFromAgvScanTaskQueue(taskItem.Barcode);
                        }
                        else if (taskItem.AgvPassFlag == (byte)AgvPassFlag.Alarm)
                        {
                            AgvInStockPassQueue.Dequeue();
                            /// 从AGV扫描任务中移除
                            this.RemoveTaskFromAgvScanTaskQueue(taskItem.Barcode);
                        }
                        else
                        {
                            AgvInStockPassQueue.Dequeue();
                        }
                        //# RefreshList();
                    }
                }
            }
            SetOPCAgvPassTimer.Start();
        }

        private int prevRoadMahineIndex = 0;
        /// <summary>
        /// 读取入库机械手数据队列中的任务，写入OPC值并可读
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private StockTaskItem currentInRobotTask = null;
        private void SetOPCInRobootPickTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SetOPCInRobootPickTimer.Stop();


            if (OPCInRobootPickData.CanWrite)
            {
                currentInRobotTask = this.GetDbTask(StockTaskItem.InPickRobotGetDbStates);
                currentInRobotTask.State = StockTaskState.RobootInStocking;

                if (currentInRobotTask == null)
                {
                    //InRobootPickQueue.Dequeue();
                }
                else
                {
                    // OPCInRobootPickData.BoxType = taskItem.BoxType;
                    int roadMachineIndex = 0;

                    // 判断将其写入巷道机的任务队列
                    // 两个都空闲的话使用平均原则，1,2,1,2,1,2 间隔入库
                    if ((!OPCDataResetData.Xdj1InPaltformIsBuff) && (!OPCDataResetData.Xdj2InPaltformIsBuff))
                    {
                        if (prevRoadMahineIndex == 1 && BaseConfig.RoadMachine2Enabled)
                        {
                            roadMachineIndex = 2;
                        }
                        else
                        {
                            if (BaseConfig.RoadMachine1Enabled)
                            {
                                roadMachineIndex = 1;
                            }
                        }
                    }
                    else if ((!OPCDataResetData.Xdj1InPaltformIsBuff) && BaseConfig.RoadMachine1Enabled)
                    {
                        roadMachineIndex = 1;

                    }
                    else if ((!OPCDataResetData.Xdj2InPaltformIsBuff) && BaseConfig.RoadMachine2Enabled)
                    {
                        roadMachineIndex = 2;

                    }


                    if (roadMachineIndex != 0)
                    {

                        prevRoadMahineIndex = roadMachineIndex;

                        currentInRobotTask.RoadMachineIndex = roadMachineIndex;
                        currentInRobotTask.State = StockTaskState.RoadMachineStockBuffing;

                        if (roadMachineIndex == 1)
                        {
                            OPCInRobootPickData.BoxType = currentInRobotTask.BoxType;
                        }
                        else if (roadMachineIndex == 2)
                        {
                            if (currentInRobotTask.BoxType == (byte)1)
                            {
                                OPCInRobootPickData.BoxType = (byte)3;
                            }
                            else
                            {
                                OPCInRobootPickData.BoxType = (byte)4;
                            }
                        }


                        if (OPCInRobootPickData.SyncWrite(OPCInRobootPickOPCGroup))
                        {
                            this.UpdateDbTask(currentInRobotTask);
                        }
                    }
                }
            }
            SetOPCInRobootPickTimer.Start();
        }


        /// <summary>
        /// 验证可读和读取入库条码信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private StockTaskItem rm1CurrentTask = null;
        private StockTaskItem rm2CurrentTask = null;

        private void SetOPCStockTaskTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SetOPCStockTaskTimer.Stop();
             

            if (OPCSetStockTaskRoadMachine1Data.CanWrite)
            {
                rm1CurrentTask  = this.GetDbTask( StockTaskItem.RoadMachineTaskGetDbStates, 1);
                if (rm1CurrentTask == null)
                {
                    
                }
                else   
                {
                    StockTaskItem taskItem = this.DequeueRoadMachineTaskQueueForStcok(1);
                    if (taskItem != null)
                    {
                        OPCSetStockTaskRoadMachine1Data.StockTaskType = (byte)taskItem.StockTaskType;

                        OPCSetStockTaskRoadMachine1Data.BoxType = taskItem.BoxType;
                        OPCSetStockTaskRoadMachine1Data.PositionFloor = taskItem.PositionFloor;
                        OPCSetStockTaskRoadMachine1Data.PositionColumn = taskItem.PositionColumn;
                        OPCSetStockTaskRoadMachine1Data.PositionRow = taskItem.PositionRow;


                        OPCSetStockTaskRoadMachine1Data.RestPositionFlag = taskItem.RestPositionFlag;

                        OPCSetStockTaskRoadMachine1Data.TrayReverseNo = taskItem.TrayReverseNo;
                        OPCSetStockTaskRoadMachine1Data.TrayNum = taskItem.TrayNum;
                        OPCSetStockTaskRoadMachine1Data.DeliveryItemNum = taskItem.DeliveryItemNum;


                        OPCSetStockTaskRoadMachine1Data.Barcode = taskItem.Barcode;
                        if (OPCSetStockTaskRoadMachine1Data.SyncWrite(OPCSetStockTaskRoadMachine1OPCGroup))
                        {
                            if (taskItem.StockTaskType == StockTaskType.IN)
                            {
                                taskItem.State = StockTaskState.RoadMachineInStocking;
                            }
                            else if (taskItem.StockTaskType == StockTaskType.OUT)
                            { 
                                taskItem.State = StockTaskState.RoadMachineOutStocking;


                                //if (OPCOutRobootPickData.CanWrite)
                                //{
                                //    this.OPCOutRobootPickData.BoxType = taskItem.BoxType;
                                //    this.OPCOutRobootPickData.TrayNum = taskItem.TrayNum;
                                //    this.OPCOutRobootPickData.SyncWrite(this.OPCOutRobootPickOPCGroup);
                                //}

                            }
                            this.UpdateDbTask(rm1CurrentTask);
                        }
                    }
                }
            }

            if (OPCSetStockTaskRoadMachine2Data.CanWrite  )
            {
               rm2CurrentTask = this.GetDbTask(StockTaskItem.RoadMachineTaskGetDbStates, 2);
                if (rm2CurrentTask==null)
                {
                   
                }
                else
                {
                    StockTaskItem taskItem = this.DequeueRoadMachineTaskQueueForStcok(2);
                    if (taskItem != null)
                    {
                        OPCSetStockTaskRoadMachine2Data.StockTaskType = (byte)taskItem.StockTaskType;

                        OPCSetStockTaskRoadMachine2Data.BoxType = taskItem.BoxType;
                        OPCSetStockTaskRoadMachine2Data.PositionFloor = taskItem.PositionFloor;
                        OPCSetStockTaskRoadMachine2Data.PositionColumn = taskItem.PositionColumn;
                        OPCSetStockTaskRoadMachine2Data.PositionRow = taskItem.PositionRow;


                        OPCSetStockTaskRoadMachine2Data.RestPositionFlag = taskItem.RestPositionFlag;

                        OPCSetStockTaskRoadMachine2Data.TrayReverseNo = taskItem.TrayReverseNo;
                        OPCSetStockTaskRoadMachine2Data.TrayNum = taskItem.TrayNum;
                        OPCSetStockTaskRoadMachine2Data.DeliveryItemNum = taskItem.DeliveryItemNum;


                        OPCSetStockTaskRoadMachine2Data.Barcode = taskItem.Barcode;
                        if (OPCSetStockTaskRoadMachine2Data.SyncWrite(OPCSetStockTaskRoadMachine2OPCGroup))
                        {
                            if (taskItem.StockTaskType == StockTaskType.IN)
                            {
                                taskItem.State = StockTaskState.RoadMachineInStocking;
                            }
                            else if (taskItem.StockTaskType == StockTaskType.OUT)
                            {
                                taskItem.State = StockTaskState.RoadMachineOutStocking;

                                //if (OPCOutRobootPickData.CanWrite)
                                //{
                                //    this.OPCOutRobootPickData.BoxType = taskItem.BoxType;
                                //    this.OPCOutRobootPickData.TrayNum = taskItem.TrayNum;
                                //    this.OPCOutRobootPickData.SyncWrite(this.OPCOutRobootPickOPCGroup);
                                //}
                            }

                            this.UpdateDbTask(rm1CurrentTask);
                        }
                    }
                }
            }
             
            SetOPCStockTaskTimer.Start();

        }


        /// <summary>
        /// 列出OPC服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListOPCServerBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AnOPCServer = new OPCServer();
                OPCServersLB.Items.Clear();
                dynamic allServerList = OPCNodeNameTB.Text.Length == 0 ? AnOPCServer.GetOPCServers() : AnOPCServer.GetOPCServers(OPCNodeNameTB.Text);

                foreach (var s in (allServerList as Array))
                {
                    OPCServersLB.Items.Add(s);
                }
                AnOPCServer = null;
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error(ex.Message, ex);
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 选择OPC服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OPCServersLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OPCServerTB.Text = OPCServersLB.SelectedValue.ToString();
        }

      

        /// <summary>
        /// OPC 服务关闭事件处理
        /// </summary>
        /// <param name="Reason"></param>
        //private void ConnectedOPCServer_ServerShutDown(string Reason)
        //{
        //    LogUtil.Logger.Info(string.Format("【OPC Sever 自停止】{0}", Reason));
        //    #region 关闭Timer\线程 等活动
        //    ShutDownComponents();
        //    #endregion
        //}

        /// <summary>
        /// 断开OPC服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisConnectOPCServerBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DisconnectOPCServer();
        }




        /// <summary>
        /// 初始化组
        /// </summary>
        private bool InitOPCGroup()
        {
            try
            {
                #region 初始化入库扫描验证组
                // 初始化入库扫描验证组
                OPCCheckInstockBarcodeOPCGroup = ConnectedOPCServer.OPCGroups.Add(OPCConfig.OPCCheckInStockBarcodeOPCGroupName);
                OPCCheckInstockBarcodeOPCGroup.UpdateRate = OPCConfig.OPCCheckInStockBarcodeOPCGroupRate;
                OPCCheckInstockBarcodeOPCGroup.DeadBand = OPCConfig.OPCCheckInStockBarcodeOPCGroupDeadBand;
                OPCCheckInstockBarcodeOPCGroup.IsSubscribed = true;
                OPCCheckInstockBarcodeOPCGroup.IsActive = true;

                // 添加item
                OPCCheckInStockBarcodeData.AddItemToGroup(OPCCheckInstockBarcodeOPCGroup);
                OPCCheckInstockBarcodeOPCGroup.DataChange += OPCCheckInStockBarcodeOPCGroup_DataChange;
                #endregion

                #region Agv入库放行组
                OPCAgvInStockPassOPCGroup = ConnectedOPCServer.OPCGroups.Add(OPCConfig.OPCAgvInStockPassOPCGroupName);
                OPCAgvInStockPassOPCGroup.UpdateRate = OPCConfig.OPCAgvInStockPassOPCGroupRate;
                OPCAgvInStockPassOPCGroup.DeadBand = OPCConfig.OPCAgvInStockPassOPCGroupDeadBand;
                OPCAgvInStockPassOPCGroup.IsSubscribed = true;
                OPCAgvInStockPassOPCGroup.IsActive = true;

                // 添加item
                OPCAgvInStockPassData.AddItemToGroup(OPCAgvInStockPassOPCGroup);
                OPCAgvInStockPassOPCGroup.DataChange += OPCAgvInStockPassOPCGroup_DataChange;
                #endregion

                #region 入库机械手抓取信息组
                OPCInRobootPickOPCGroup = ConnectedOPCServer.OPCGroups.Add(OPCConfig.OPCInRobootPickOPCGroupName);
                OPCInRobootPickOPCGroup.UpdateRate = OPCConfig.OPCInRobootPickOPCGroupRate;
                OPCInRobootPickOPCGroup.DeadBand = OPCConfig.OPCInRobootPickOPCGroupDeadBand;
                OPCInRobootPickOPCGroup.IsSubscribed = true;
                OPCInRobootPickOPCGroup.IsActive = true;

                // 添加item
                OPCInRobootPickData.AddItemToGroup(OPCInRobootPickOPCGroup);
                OPCInRobootPickOPCGroup.DataChange += OPCInRobootPickOPCGroup_DataChange;

                #endregion

                #region 初始化巷道机入库任务组
                // 初始化 巷道机1 入库任务组
                OPCSetStockTaskRoadMachine1OPCGroup = ConnectedOPCServer.OPCGroups.Add(OPCConfig.OPCSetStockTaskRM1OPCGroupName);
                OPCSetStockTaskRoadMachine1OPCGroup.UpdateRate = OPCConfig.OPCSetStockTaskRM1OPCGroupRate;
                OPCSetStockTaskRoadMachine1OPCGroup.DeadBand = OPCConfig.OPCSetStockTaskRM1OPCGroupDeadBand;
                OPCSetStockTaskRoadMachine1OPCGroup.IsSubscribed = true;
                OPCSetStockTaskRoadMachine1OPCGroup.IsActive = true;
                // 添加item
                OPCSetStockTaskRoadMachine1Data.AddItemToGroup(OPCSetStockTaskRoadMachine1OPCGroup);
                OPCSetStockTaskRoadMachine1OPCGroup.DataChange += OPCSetStockTaskRoadMachine1OPCGroup_DataChange;

                // 初始化 巷道机2 入库任务组
                OPCSetStockTaskRoadMachine2OPCGroup = ConnectedOPCServer.OPCGroups.Add(OPCConfig.OPCSetStockTaskRM2OPCGroupName);
                OPCSetStockTaskRoadMachine2OPCGroup.UpdateRate = OPCConfig.OPCSetStockTaskRM2OPCGroupRate;
                OPCSetStockTaskRoadMachine2OPCGroup.DeadBand = OPCConfig.OPCSetStockTaskRM2OPCGroupDeadBand;
                OPCSetStockTaskRoadMachine2OPCGroup.IsSubscribed = true;
                OPCSetStockTaskRoadMachine2OPCGroup.IsActive = true;
                // 添加item
                OPCSetStockTaskRoadMachine2Data.AddItemToGroup(OPCSetStockTaskRoadMachine2OPCGroup);
                OPCSetStockTaskRoadMachine2OPCGroup.DataChange += OPCSetStockTaskRoadMachine2OPCGroup_DataChange;

                // 初始化 巷道机1 入库任务反馈组
                OPCRoadMachine1TaskFeedOPCGroup = ConnectedOPCServer.OPCGroups.Add(OPCConfig.OPCTaskFeedRM1OPCGroupName);
                OPCRoadMachine1TaskFeedOPCGroup.UpdateRate = OPCConfig.OPCTaskFeedRM1OPCGroupRate;
                OPCRoadMachine1TaskFeedOPCGroup.DeadBand = OPCConfig.OPCTaskFeedRM1OPCGroupDeadBand;
                OPCRoadMachine1TaskFeedOPCGroup.IsSubscribed = true;
                OPCRoadMachine1TaskFeedOPCGroup.IsActive = true;
                // 添加item
                OPCRoadMachine1TaskFeedData.AddItemToGroup(OPCRoadMachine1TaskFeedOPCGroup);
                OPCRoadMachine1TaskFeedOPCGroup.DataChange += OPCRoadMachine1TaskFeedOPCGroup_DataChange;

                // 初始化 巷道机2 入库任务反馈组
                OPCRoadMachine2TaskFeedOPCGroup = ConnectedOPCServer.OPCGroups.Add(OPCConfig.OPCTaskFeedRM2OPCGroupName);
                OPCRoadMachine2TaskFeedOPCGroup.UpdateRate = OPCConfig.OPCTaskFeedRM2OPCGroupRate;
                OPCRoadMachine2TaskFeedOPCGroup.DeadBand = OPCConfig.OPCTaskFeedRM2OPCGroupDeadBand;
                OPCRoadMachine2TaskFeedOPCGroup.IsSubscribed = true;
                OPCRoadMachine2TaskFeedOPCGroup.IsActive = true;
                // 添加item
                OPCRoadMachine2TaskFeedData.AddItemToGroup(OPCRoadMachine2TaskFeedOPCGroup);
                OPCRoadMachine2TaskFeedOPCGroup.DataChange += OPCRoadMachine2TaskFeedOPCGroup_DataChange;


                // 初始化 出库抓手 任务
                OPCOutRobootPickOPCGroup = ConnectedOPCServer.OPCGroups.Add(OPCConfig.OPCOutRobootPickOPCGroupName);
                OPCOutRobootPickOPCGroup.UpdateRate = OPCConfig.OPCOutRobootPickOPCGroupRate;
                OPCOutRobootPickOPCGroup.DeadBand = OPCConfig.OPCOutRobootPickOPCGroupDeadBand;
                OPCOutRobootPickOPCGroup.IsSubscribed = true;
                OPCOutRobootPickOPCGroup.IsActive = true;
                // 添加item
                OPCOutRobootPickData.AddItemToGroup(OPCOutRobootPickOPCGroup);
                OPCOutRobootPickOPCGroup.DataChange += OPCOutRobootPickOPCGroup_DataChange;

                // 设置OPC
                OPCDataResetOPCGroup = ConnectedOPCServer.OPCGroups.Add(OPCConfig.OPCDataResetOPCGroupName);
                OPCDataResetOPCGroup.UpdateRate = OPCConfig.OPCDataResetOPCGroupRate;
                OPCDataResetOPCGroup.DeadBand = OPCConfig.OPCInRobootPickOPCGroupDeadBand;
                OPCDataResetOPCGroup.IsSubscribed = true;
                OPCDataResetOPCGroup.IsActive = true;
                // 添加item
                OPCDataResetData.AddItemToGroup(OPCDataResetOPCGroup);
                OPCDataResetOPCGroup.DataChange += OPCDataResetOPCGroup_DataChange;

                #endregion


                return true;

            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error(ex.Message, ex);
                MessageBox.Show(ex.Message);
            }
            return false;
        }



        // 第一次会获取到opcserver的数据，即使没有触发，相当于初始化
        // 扫描入库的信息获取
        private void OPCCheckInStockBarcodeOPCGroup_DataChange(
            int TransactionID,
            int NumItems,
            ref Array ClientHandles,
            ref Array ItemValues,
            ref Array Qualities,
            ref Array TimeStamps)
        {
            try
            {
                // 从1开始
                for (var i = 1; i <= NumItems; i++)
                {
                    LogUtil.Logger.InfoFormat("【数据改变】【入库条码扫描】【{0}】{1}",
                        OPCCheckInStockBarcodeData.GetSimpleOpcKey(i, ClientHandles),
                        ItemValues.GetValue(i));
                }
                OPCCheckInStockBarcodeData.SetValue(NumItems, ClientHandles, ItemValues);
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error(ex.Message, ex);
                //MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// AGV放行数据改变事件
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="ItemValues"></param>
        /// <param name="Qualities"></param>
        /// <param name="TimeStamps"></param>
        private void OPCAgvInStockPassOPCGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            try
            {
                // 从1开始
                for (var i = 1; i <= NumItems; i++)
                {
                    LogUtil.Logger.InfoFormat("【数据改变】【AGV放行】【{0}】{1}",
                        OPCAgvInStockPassData.GetSimpleOpcKey(i, ClientHandles),
                        ItemValues.GetValue(i));
                }
                OPCAgvInStockPassData.SetValue(NumItems, ClientHandles, ItemValues);
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error(ex.Message, ex);
                //MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 入库机械手抓手信息
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="ItemValues"></param>
        /// <param name="Qualities"></param>
        /// <param name="TimeStamps"></param>
        private void OPCInRobootPickOPCGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            try
            {
                // 从1开始
                for (var i = 1; i <= NumItems; i++)
                {
                    LogUtil.Logger.InfoFormat("【数据改变】【机械手】【{0}】{1}",
                       OPCInRobootPickData.GetSimpleOpcKey(i, ClientHandles),
                        ItemValues.GetValue(i));
                }
                OPCInRobootPickData.SetValue(NumItems, ClientHandles, ItemValues);
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error(ex.Message, ex);
                //MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 设置 巷道机1 入库任务的数据改变事件处理
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="ItemValues"></param>
        /// <param name="Qualities"></param>
        /// <param name="TimeStamps"></param>
        private void OPCSetStockTaskRoadMachine1OPCGroup_DataChange(int TransactionID,
            int NumItems,
            ref Array ClientHandles,
            ref Array ItemValues,
            ref Array Qualities,
            ref Array TimeStamps)
        {
            try
            {
                // 从1开始
                for (var i = 1; i <= NumItems; i++)
                {
                    LogUtil.Logger.InfoFormat("【数据改变】【任务接受】【巷道机1】【{0}】{1}",
                        OPCSetStockTaskRoadMachine1Data.GetSimpleOpcKey(i, ClientHandles),
                        ItemValues.GetValue(i));
                }

                OPCSetStockTaskRoadMachine1Data.SetValue(NumItems, ClientHandles, ItemValues);
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error(ex.Message, ex);
                //MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 设置 巷道机2 入库任务的数据改变事件处理
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="ItemValues"></param>
        /// <param name="Qualities"></param>
        /// <param name="TimeStamps"></param>
        private void OPCSetStockTaskRoadMachine2OPCGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            try
            {
                // 从1开始
                for (var i = 1; i <= NumItems; i++)
                {
                    LogUtil.Logger.InfoFormat("【数据改变】【任务接受】【巷道机 2】【{0}】{1}",
                        OPCSetStockTaskRoadMachine2Data.GetSimpleOpcKey(i, ClientHandles),
                        ItemValues.GetValue(i).ToString());
                }
                OPCSetStockTaskRoadMachine2Data.SetValue(NumItems, ClientHandles, ItemValues);
               
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error(ex.Message, ex);
                try
                {
                    for (var i = 1; i <= NumItems; i++)
                    {
                        LogUtil.Logger.InfoFormat("【程序错误】【数据改变】【任务接受】【巷道机 2】{0}",
                            ItemValues.GetValue(i).ToString());
                    }
                }
                catch (Exception eex)
                {
                    LogUtil.Logger.Error(eex.Message, eex);
                }
                //MessageBox.Show(ex.Message);
            }
        }



        /// <summary>
        /// 设置 巷道机1 入库任务的反馈 数据改变事件处理
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="ItemValues"></param>
        /// <param name="Qualities"></param>
        /// <param name="TimeStamps"></param>
        private void OPCRoadMachine1TaskFeedOPCGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            try
            {
                // 从1开始
                //for (var i = 1; i <= NumItems; i++)
                //{
                //    LogUtil.Logger.InfoFormat("【数据改变】【任务反馈】【巷道机 1】【{0}】{1}",
                //        OPCRoadMachine1TaskFeedData.GetSimpleOpcKey(i),
                //        ItemValues.GetValue(i));
                //}
                OPCRoadMachine1TaskFeedData.SetValue(NumItems, ClientHandles, ItemValues);
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error(ex.Message, ex);
                //MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 设置 巷道机2 入库任务的反馈 数据改变事件处理
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="ItemValues"></param>
        /// <param name="Qualities"></param>
        /// <param name="TimeStamps"></param>
        private void OPCRoadMachine2TaskFeedOPCGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            try
            {
                //// 从1开始
                //for (var i = 1; i <= NumItems; i++)
                //{
                //    LogUtil.Logger.InfoFormat("【数据改变】【任务反馈】【巷道机 2】【{0}】{1}",
                //        OPCRoadMachine2TaskFeedData.GetSimpleOpcKey(i),
                //        ItemValues.GetValue(i));
                //}
                OPCRoadMachine2TaskFeedData.SetValue(NumItems, ClientHandles, ItemValues);
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error(ex.Message, ex);
                // MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 出库机械手 数据改变事件处理
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="ItemValues"></param>
        /// <param name="Qualities"></param>
        /// <param name="TimeStamps"></param>
        private void OPCOutRobootPickOPCGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            try
            {
                // 从1开始
                for (var i = 1; i <= NumItems; i++)
                {
                    LogUtil.Logger.InfoFormat("【数据改变】【出库机械手】【{0}】{1}",
                        OPCOutRobootPickData.GetSimpleOpcKey(i, ClientHandles),
                        ItemValues.GetValue(i));
                }
                OPCOutRobootPickData.SetValue(NumItems, ClientHandles, ItemValues);
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error(ex.Message, ex);
                //   MessageBox.Show(ex.Message);
            }
        }



        /// <summary>
        /// 出库机械手 数据改变事件处理
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="ItemValues"></param>
        /// <param name="Qualities"></param>
        /// <param name="TimeStamps"></param>
        private void OPCDataResetOPCGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            try
            {
                // 从1开始
                for (var i = 1; i <= NumItems; i++)
                {
                    LogUtil.Logger.InfoFormat("【数据改变】【OPC 设置】【{0}】{1}",
                        OPCDataResetData.GetSimpleOpcKey(i, ClientHandles),
                        ItemValues.GetValue(i));
                }
                OPCDataResetData.SetValue(NumItems, ClientHandles, ItemValues);
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error(ex.Message, ex);
                //   MessageBox.Show(ex.Message);
            }
        }



        /// <summary>
        /// 读取入库条码信息读写标记改变处理
        /// </summary>
        /// <param name="b"></param>
        /// <param name="toFlag"></param>
        private void OPCCheckInStockBarcodeData_RwFlagChangedEvent(OPCDataBase b, byte toFlag)
        {
            if (b.CanRead)
            {
                // 读取条码，获取放行信息写入队列
                LogUtil.Logger.InfoFormat("【根据-条码-判断放行】{0}", OPCCheckInStockBarcodeData.ScanedBarcode);
                try
                {
                   // BaseConfig.PreScanBar = OPCCheckInStockBarcodeData.ParseBarcode(OPCCheckInStockBarcodeData.ScanedBarcode);
                    this.CreateInTaskIntoAgvScanTaskQueue(OPCCheckInStockBarcodeData.ScanedBarcode);
                }
                catch (Exception ex)
                {
                    LogUtil.Logger.Error(ex.Message, ex);
                }
                finally
                {
                    // 置为可写
                    this.OPCCheckInStockBarcodeData.SyncSetWriteableFlag(OPCCheckInstockBarcodeOPCGroup);
                }
            }
        }

        /// <summary>
        /// agv入库放行读写标记改变处理
        /// </summary>
        /// <param name="b"></param>
        /// <param name="toFlag"></param>
        private void OPCAgvInStockPassData_RwFlagChangedEvent(OPCDataBase b, byte toFlag)
        {
            //if (b.CanWrite)
            //{
            /// 将放行信息队列中的信息写入OPC
            /// 使用定时器去写
            /// throw new NotImplementedException();
            //}
        }

        /// <summary>
        /// 入库机械手抓取信息读写标记改变处理
        /// </summary>
        /// <param name="b"></param>
        /// <param name="toFlag"></param>
        private void OPCInRobootPickData_RwFlagChangedEvent(OPCDataBase b, byte toFlag)
        {
            if (b.CanWrite)
            {
                /// 将入库机械手抓取信息写入OPC
                /// 使用定时器去写
            }
        }


        /// <summary>
        /// 入库任务 巷道机1
        /// </summary>
        /// <param name="b"></param>
        /// <param name="toFlag"></param>
        private void OPCSetStockTaskRoadMachine1Data_RwFlagChangedEvent(OPCDataBase b, byte toFlag)
        {
            LogUtil.Logger.InfoFormat("【OPC  巷道机 1 入库任务读写标记改变】{0}->{1}", b.OPCRwFlagWas, b.OPCRwFlag);
            //if (b.CanWrite)
            //{
            // 这边就不写了，使用定时器写入库任务！
            //}
        }

        /// <summary>
        /// 入库任务 巷道机2
        /// </summary>
        /// <param name="b"></param>
        /// <param name="toFlag"></param>
        private void OPCSetStockTaskRoadMachine2Data_RwFlagChangedEvent(OPCDataBase b, byte toFlag)
        {
            LogUtil.Logger.InfoFormat("【OPC  巷道机 2 入库任务读写标记改变】{0}->{1}", b.OPCRwFlagWas, b.OPCRwFlag);
        }

        /// <summary>
        /// 任务动作反馈 巷道机1
        /// </summary>
        /// <param name="taskFeed"></param>
        /// <param name="toActionFlag"></param>
        private void OPCRoadMachine1TaskFeedData_ActionFlagChangeEvent(OPCRoadMachineTaskFeed taskFeed, byte toActionFlag)
        {
            LogUtil.Logger.InfoFormat("【OPC  巷道机 {0} 库存动作标记改变】【{1}】{2}->{3}",
                taskFeed.RoadMachineIndex,
                taskFeed.CurrentBarcode,
                taskFeed.ActionFlagWas,
                taskFeed.ActionFlag);
            // 更改任务状态
            if ((StockTaskActionFlag)taskFeed.ActionFlagWas == StockTaskActionFlag.Excuting)
            {
                LogUtil.Logger.Info("********************************************************************");
                LogUtil.Logger.InfoFormat("{0}->{1}", taskFeed.ActionFlagWas, taskFeed.ActionFlag);
                LogUtil.Logger.Info("********************************************************************");
                UpdateTaskByFeed(taskFeed.RoadMachineIndex, 
                    (StockTaskActionFlag)taskFeed.ActionFlag,
                    taskFeed.CurrentBarcode);
            }
        }

        /// <summary>
        /// 出库机械手可写标记改变事件
        /// </summary>
        /// <param name="b"></param>
        /// <param name="toFlag"></param>
        private void OPCOutRobootPickData_RwFlagChangedEvent(OPCDataBase b, byte toFlag)
        {
            LogUtil.Logger.InfoFormat("【OPC  入库机械手 读写标记改变】{0}->{1}", b.OPCRwFlagWas, b.OPCRwFlag);
            //if (b.CanWrite)
            //{

            //}
        }


        /// <summary>
        /// 根据巷道机反馈返回
        /// </summary>
        /// <param name="roadMachineIndex"></param>
        /// <param name="actionFlag"></param>
        /// <param name="barCode"></param>
        public void UpdateTaskByFeed(int roadMachineIndex, StockTaskActionFlag actionFlag, string barcode)
        { 
            if (roadMachineIndex <= 0)
            {
                return;
            } 

            var taskItem = roadMachineIndex == 1 ? rm1CurrentTask : rm2CurrentTask;

            if (taskItem != null)
            { 
                switch ((StockTaskActionFlag)((int)actionFlag))
                {
                    case StockTaskActionFlag.InSuccess:
                        taskItem.State = StockTaskState.InStocked;
                        new StorageService(OPCConfig.DbString).InStockByCheckCode(taskItem.PositionNr, taskItem.Barcode);
                        
                        break;
                    case StockTaskActionFlag.InFailPositionWasStored:
                    case StockTaskActionFlag.InFailPositionNotExists:
                        throw new NotImplementedException("入库反馈错误未实现!");
                        break;
                    case StockTaskActionFlag.OutSuccess:
                        taskItem.State = StockTaskState.OutStocked;
                        new StorageService(OPCConfig.DbString).OutStockByBarCode(taskItem.Barcode);
                       
                        break;
                    case StockTaskActionFlag.OutFailStoreNotFound:
                    case StockTaskActionFlag.OutFailBarNotMatch:
                    case StockTaskActionFlag.OutFailPositionNotExists:
                        throw new NotImplementedException("出库反馈错误未实现!");
                        break;
                    case StockTaskActionFlag.Success:

                        LogUtil.Logger.Info("********【成功状态反馈】************************************************************");
                        LogUtil.Logger.InfoFormat("条码: {0}---DbId:{1} -- 库位:{2}", taskItem.Barcode, taskItem.DbId, taskItem.PositionNr);
                        LogUtil.Logger.Info("********************************************************************");


                        if (taskItem.StockTaskType == StockTaskType.IN)
                        {
                            taskItem.State = StockTaskState.InStocked;
                            if (TestConfig.InStockCreateStorage)
                            {
                                new StorageService(OPCConfig.DbString)
                                    .InStockByCheckCode(taskItem.PositionNr, taskItem.Barcode);
                            }
                           
                        }
                        else if (taskItem.StockTaskType == StockTaskType.OUT)
                        {
                            taskItem.State = StockTaskState.OutStocked;
                            if (TestConfig.OutStockTaskDelStorage)
                            {
                                new StorageService(OPCConfig.DbString).OutStockByBarCode(taskItem.Barcode);
                            }
                        }
                        this.UpdateDbTask(taskItem);
                        break;
                    case StockTaskActionFlag.Fail:
                        if (taskItem.StockTaskType == StockTaskType.IN)
                        {
                            taskItem.State = StockTaskState.ErrorInStock;
                            
                        }
                        else if (taskItem.StockTaskType == StockTaskType.OUT)
                        {
                            taskItem.State = StockTaskState.ErrorOutStock;
                             
                        }
                        break;
                    default: break;
                }
                 
  
            }
        }

        //    private string prevScanedBarcode = string.Empty;
        /// <summary>
        /// 将入库任务写入AGV扫描任务队列，并派发到AGV放行队列
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        private bool CreateInTaskIntoAgvScanTaskQueue(string barcode)
        {

            lock (WriteTaskCenterQueueLocker)
            {
                StockTaskItem taskItem = new StockTaskItem()
                {
                    Barcode = barcode,
                    StockTaskType = StockTaskType.IN
                };
                // taskItem.TaskStateChangeEvent += new StockTaskItem.TaskStateChangeEventHandler(TaskItem_TaskStateChangeEvent);

                if (!string.IsNullOrEmpty(barcode))
                {
                    #region 入库
                    UniqueItemService uniqItemService = new UniqueItemService(OPCConfig.DbString);
                    // UniqueItem item = uniqItemService.FindByCheckCode(barcode);
                    UniqueItem item = uniqItemService.FindByNr(barcode);
                    if (item != null)
                    {
                        //// 是否是重复扫描
                        if (BaseConfig.PreScanBar == barcode)
                        {
                            BaseConfig.PreScanBar = barcode;
                            // 重复扫描的不再生成任务
                            taskItem.State = StockTaskState.ErrorBarcodeReScan;
                            if (TestConfig.ShowRescanErrorBarcode)
                            {

                                this.AddOrUpdateItemToTaskDisplay(taskItem);
                                // TaskCenterForDisplayQueue.Add(taskItem);
                            }
                            return true;
                        }

                        // 是否可以入库
                        if (uniqItemService.CanUniqInStock(barcode))
                        {
                            // 是否是重复扫描
                            if (AgvScanTaskQueue.Keys.Contains(barcode))
                            {
                                  BaseConfig.PreScanBar = barcode;
                                // prevScanedBarcode = barcode;
                                // 重复扫描的不再生成任务
                                taskItem.State = StockTaskState.ErrorBarcodeReScan;
                                if (TestConfig.ShowRescanErrorBarcode)
                                {
                                    this.AddOrUpdateItemToTaskDisplay(taskItem);
                                    // TaskCenterForDisplayQueue.Add(taskItem);
                                }
                                return true;
                            }

                            taskItem.AgvPassFlag = (byte)AgvPassFlag.Pass;
                            // 先放小车，不计算库位!
                            taskItem.BoxType = (byte)item.BoxTypeId;
                        }
                        else
                        {
                            // 不可入库
                            taskItem.AgvPassFlag = (byte)AgvPassFlag.Alarm;
                            taskItem.State = StockTaskState.ErrorUniqCannotInStock;
                        }
                    }
                    else
                    {
                        taskItem.AgvPassFlag = (byte)AgvPassFlag.Alarm;
                        taskItem.State = StockTaskState.ErrorUniqNotExsits;
                    }
                }
                else
                {
                    taskItem.AgvPassFlag = (byte)AgvPassFlag.ReScan;
                }
                #endregion

                BaseConfig.PreScanBar = barcode;
                // prevScanedBarcode = barcode;
                /// 加入到AGV扫描队列
                // 先插入数据库Task再加入队列，最后置可读
                // StockTaskService ts = new StockTaskService(OPCConfig.DbString);
                if (!this.CreateDbTask(taskItem))
                {
                    taskItem.AgvPassFlag = (byte)AgvPassFlag.ReScan;
                    taskItem.State = StockTaskState.ErrorCreateDbTask;
                }


                EnqueueAgvScanTaskQueue(taskItem);
                return false;
            }
        }


        #region AGV扫描队列任务
        /// <summary>
        /// 进入AGV扫描任务队列
        /// </summary>
        /// <param name="taskItem"></param>
        private void EnqueueAgvScanTaskQueue(StockTaskItem taskItem)
        {
            lock (AgvScanTaskQueueLocker)
            {
                //加入AGV扫描队列
                if (AgvScanTaskQueue.Keys.Contains(taskItem.Barcode))
                {
                    AgvScanTaskQueue[taskItem.Barcode] = taskItem;
                }
                else
                {
                    AgvScanTaskQueue.Add(taskItem.Barcode, taskItem);
                }

                this.AddOrUpdateItemToTaskDisplay(taskItem);

                //  TaskCenterForDisplayQueue.Add(taskItem);
                if (taskItem.StockTaskType == StockTaskType.IN)
                {
                    // 立刻加入到放行队列
                    StockTaskItem passTaskItem = AgvScanTaskQueue
                        .Where(s => s.Value.IsInProcessing == false)
                        .Select(s => s.Value).FirstOrDefault();
                    if (passTaskItem != null)
                    {
                        passTaskItem.IsInProcessing = true;

                        passTaskItem.State = StockTaskState.AgvWaitPassing;
                        this.UpdateDbTask(passTaskItem);
                        // 进入agv放行
                        AgvInStockPassQueue.Enqueue(passTaskItem);
                    }
                }
                // 刷新界面列表
                //# RefreshList();
            }
        }
        #endregion

        #region 获得最优先的库存操作任务
         


        /// <summary>
        /// 分发任务给巷道机1 或 2
        /// </summary>
        /// <param name="roadMachineIndex"></param>
        /// <returns></returns>
        private StockTaskItem DequeueRoadMachineTaskQueueForStcok(int roadMachineIndex)
        {


            StockTaskItem taskItem = roadMachineIndex == 1 ? rm1CurrentTask : rm2CurrentTask;
            if (taskItem.StockTaskType == StockTaskType.IN)
            {

                Position position = GetPositionForDispatch(roadMachineIndex);

                taskItem.PositionNr = position.Nr;
                taskItem.PositionFloor = position.Floor;
                taskItem.PositionColumn = position.Column;
                taskItem.PositionRow = position.Row;
            }
            return taskItem;
        }

        /// <summary>
        /// 将任务从队列中移除
        /// </summary>
        /// <param name="barcode"></param>
        private void RemoveTaskFromAgvScanTaskQueue(string barcode)
        {
            lock (AgvScanTaskQueueLocker)
            {
                if (this.AgvScanTaskQueue.ContainsKey(barcode))
                {
                    this.AgvScanTaskQueue.Remove(barcode);
                }
            }
        }
        #endregion


        #region 任务状态改变队列处理
        private Queue comDataQ = new Queue();
        private Queue receiveMessageQueue;
        private Thread receiveMessageThread;
        private ManualResetEvent receivedEvent = new ManualResetEvent(false);

        /// <summary>
        /// 初始化并启动 任务状态改变队列处理组件
        /// </summary>
        private void InitAndStartUpdateStackTaskStateComponent()
        {
            //receiveMessageThread = new Thread(this.ReceiveMessageThread);
           // receiveMessageQueue = Queue.Synchronized(comDataQ);
            //receiveMessageThread.IsBackground = true;

           // receiveMessageThread.Start();
        }
        /// <summary>
        /// 停止 任务状态改变队列处理组件
        /// </summary>
        private void ShutDownUpdateStackTaskStateComponent()
        {
         //  receiveMessageThread.Abort();
        }

        private void ReceiveMessageThread()
        {
            while (true)
            {
                while (receiveMessageQueue.Count > 0)
                {
                    StockTaskService sts = new StockTaskService(OPCConfig.DbString);
                    sts.UpdateTaskState(receiveMessageQueue.Dequeue() as StockTask);
                }

                receivedEvent.WaitOne();
                receivedEvent.Reset();
            }
        }
        #endregion
        

        #region 组件操作
        /// <summary>
        /// 启动组件
        /// </summary>
        private void StartComponents()
        {

        }

        /// <summary>
        /// 关闭运行的组件
        /// </summary>
        private void ShutDownComponents()
        {
            this.StopTimers();
            this.ShutDownUpdateStackTaskStateComponent();
            this.DisconnectOPCServer();
        }

        /// <summary>
        /// 关闭OPC服务
        /// </summary>
        private void DisconnectOPCServer()
        {
            try
            {
                if (ConnectedOPCServer != null)
                {
                    ConnectedOPCServer.Disconnect();
                }
            }
            catch (Exception ex)
            {
                ConnectedOPCServer = null;
                ConnectOPCServerBtn.IsEnabled = true;
                LogUtil.Logger.Error(ex.Message, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ConnectedOPCServer = null;
                ConnectOPCServerBtn.IsEnabled = true;
            }
        }

        /// <summary>
        /// 停止定时器
        /// </summary>
        private void StopTimers()
        {
            if (SetOPCAgvPassTimer != null)
            {
                SetOPCAgvPassTimer.Enabled = false;
                SetOPCAgvPassTimer.Stop();
            }
            if (SetOPCInRobootPickTimer != null)
            {
                SetOPCInRobootPickTimer.Enabled = false;
                SetOPCInRobootPickTimer.Stop();
            }

            if (DispatchTrayOutStockTaskTimer != null)
            {
                DispatchTrayOutStockTaskTimer.Enabled = false;
                DispatchTrayOutStockTaskTimer.Stop();
            }

            if (SetOPCStockTaskTimer != null)
            {
                SetOPCStockTaskTimer.Enabled = false;
                SetOPCStockTaskTimer.Stop();
            }
          
        }
        #endregion

        /// <summary>
        /// 写入入库条码信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WirteGetPisitionBarBtn_Click(object sender, RoutedEventArgs e)
        {
            //if (OPCCheckInStockBarcodeData.CanWrite)
            if (true)
            {
                OPCCheckInStockBarcodeData.ScanedBarcode = ScanedBarCodeTB.Text;
                if (OPCCheckInStockBarcodeData.SyncWrite(OPCCheckInstockBarcodeOPCGroup))
                {
                    LogUtil.Logger.Info("【条码读取成功】");
                    // MessageBox.Show("条码读取成功");
                }
            }
            else
            {
                MessageBox.Show("OPC暂不可以写入数据");
            }
        }


        /// <summary>
        /// 读取入库条码信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadScanedBarCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (OPCCheckInStockBarcodeData.CanRead)
            {
                if (OPCCheckInStockBarcodeData.SyncSetWriteableFlag(OPCCheckInstockBarcodeOPCGroup))
                {
                    MessageBox.Show("条码读取成功");
                }
            }
            else
            {
                MessageBox.Show("不存在任务，OPC暂不可以读取数据");
            }

        }

        /// <summary>
        /// 读取任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadInStockTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (OPCSetStockTaskRoadMachine1Data.CanRead)
            {

            }
            else
            {
                MessageBox.Show("不存在任务，OPC暂不可以读取入库数据");
            }
        }

        /// <summary>
        /// 更新显示列表
        /// </summary>
        private void RefreshList()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {

                CenterStockTaskDisplayDG.Items.Refresh();

            }));
        }

        /// <summary>
        /// 关闭窗口时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ShutDownComponents();
        }




        #region 从数据库加载出库任务
        /// <summary>
        /// 将数据库中的任务加载到Task中,#TO-DO#
        /// </summary>
        private void InitQueueAndLoadTaskFromDb()
        {
            AgvScanTaskQueue = new Dictionary<string, StockTaskItem>();
            AgvInStockPassQueue = new Queue();


            TaskCenterForDisplayQueue = new List<StockTaskItem>();

            // this.RefreshList();
            this.Dispatcher.Invoke(new Action(() =>
            {
                CenterStockTaskDisplayDG.ItemsSource = TaskCenterForDisplayQueue;  // CenterStockTaskDisplayDG.Items.Refresh();
            }));
        }

        
        /// <summary>
        /// 将任务放入显示列表
        /// </summary>
        /// <param name="taskItem"></param>
        private void AddOrUpdateItemToTaskDisplay(StockTaskItem taskItem)
        {
            if (TaskCenterForDisplayQueue.Where(s => s.DbId == taskItem.DbId && taskItem.DbId > 0).FirstOrDefault() != null)
            {
                var i = TaskCenterForDisplayQueue.Where(s => s.DbId == taskItem.DbId && taskItem.DbId > 0).FirstOrDefault();
              //  i = taskItem;

                i.RoadMachineIndex = taskItem.RoadMachineIndex;

                i.BoxType = taskItem.BoxType;

                i.PositionNr = taskItem.PositionNr;
                i.PositionFloor = taskItem.PositionFloor;
                i.PositionColumn = taskItem.PositionColumn;
                i.PositionRow = taskItem.PositionRow;
                i.AgvPassFlag = taskItem.AgvPassFlag;
                i.RestPositionFlag = taskItem.RestPositionFlag;
                i.Barcode = taskItem.Barcode;
                i.State = taskItem.State;
                i.StockTaskType = taskItem.StockTaskType;
                i.TrayReverseNo = taskItem.TrayReverseNo;
                i.TrayNum = taskItem.TrayNum;
                i.DeliveryItemNum = taskItem.DeliveryItemNum;
                i.DbId = taskItem.DbId;
                i.CreatedAt = taskItem.CreatedAt;
                i.IsInProcessing = true;

            }
            else
            {
                TaskCenterForDisplayQueue.Add(taskItem);
            }
            if(TaskCenterForDisplayQueue.Count> BaseConfig.MaxMonitorTaskNum)
            {
                TaskCenterForDisplayQueue.RemoveRange(0, TaskCenterForDisplayQueue.Count - BaseConfig.KeepMonitorTaskNum);
            }
            RefreshList();
        }


        private   object getPostionLocker = new object();

        private Position GetPositionForDispatch(int roadMachineIndex)
        {
            lock (getPostionLocker)
            {
                PositionService ps = new PositionService(OPCConfig.DbString);
                Position position = ps.FindInStockPosition(roadMachineIndex,null,true);

                return position;
            }
        }



        /// <summary>
        /// 逐托分发出库任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatchTrayOutStockTaskTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DispatchTrayOutStockTaskTimer.Stop();

            if (OPCOutRobootPickData.CanWrite)
            { 

                if (this.rm1CurrentTask!=null && this.rm1CurrentTask.StockTaskType==StockTaskType.OUT)
                {
                    this.OPCOutRobootPickData.SyncWrite(this.OPCOutRobootPickOPCGroup);
                }else if(this.rm2CurrentTask != null && this.rm2CurrentTask.StockTaskType == StockTaskType.OUT)
                {
                    this.OPCOutRobootPickData.SyncWrite(this.OPCOutRobootPickOPCGroup);
                }
            }

            DispatchTrayOutStockTaskTimer.Start();
        }
        #endregion


    }
}