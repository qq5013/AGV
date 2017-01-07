﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGVCenterLib.Data;
using AGVCenterLib.Data.Repository.Implement;
using AGVCenterLib.Data.Repository.Interface;
using AGVCenterLib.Enum;
using AGVCenterLib.Model;
using AGVCenterLib.Model.Message;
using AGVCenterLib.Model.OPC;
using AGVCenterLib.Model.SearchModel;
using Brilliantech.Framwork.Utils.LogUtil;

namespace AGVCenterLib.Service
{
    public class StockTaskService : ServiceBase
    {

        public StockTaskService(string dbString) : base(dbString)
        {
        }
         
        /// <summary>
        /// 创建入库任务
        /// </summary>
        /// <returns></returns>
        public bool CreateInStockTask(StockTaskItem task)
        {
            StockTask st = new StockTask()
            {
                BoxType = task.BoxType,
                RoadMachineIndex = task.RoadMachineIndex,
                PositionNr = task.PositionNr,
                PositionFloor = task.PositionFloor,
                PositionColumn = task.PositionColumn,
                PositionRow = task.PositionRow,
                AgvPassFlag = task.AgvPassFlag,
                RestPositionFlag = task.RestPositionFlag,
                BarCode = task.Barcode,
                State = (int)task.State,
                Type = (int)StockTaskType.IN,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            IStockTaskRepository stRep = new StockTaskRepository(this.Context);
            stRep.Create(st);
            this.Context.SaveAll();
            task.DbId = st.Id;

            new StockTaskLogService(this.DbString).CreateByStockTask(st);
            return true;
        }
        public void CreateTask(StockTask st)
        {
            IStockTaskRepository stRep = new StockTaskRepository(this.Context);
            stRep.Create(st);
            this.Context.SaveAll();
        }

        /// <summary>
        /// 更新任务状态
        /// </summary>
        /// <param name="taskStock"></param>
        /// <returns></returns>
        public bool UpdateTaskState(StockTask taskStock)
        {
            IStockTaskRepository stockTaskRep = new StockTaskRepository(this.Context);
            StockTask t = stockTaskRep.FindById(taskStock.Id);
            if (t != null)
            {
                t.State = (int)taskStock.State;

                t.RoadMachineIndex = taskStock.RoadMachineIndex;
                t.PositionNr = taskStock.PositionNr;
                t.PositionFloor = taskStock.PositionFloor;
                t.PositionColumn = taskStock.PositionColumn;
                t.PositionRow = taskStock.PositionRow;
                t.UpdatedAt = DateTime.Now;
                if (t.Type.Value == (int)StockTaskType.OUT
                    && t.State.HasValue
                    && t.State.Value == (int)StockTaskState.ManOutStocked)
                {
                    new StorageService(this.Context).OutStockByBarCode(t.BarCode);
                }
                else if (t.Type.Value == (int)StockTaskType.IN && t.State.HasValue && t.State.Value == (int)StockTaskState.ManInStocked)
                {
                    new StorageService(this.Context).InStockByCheckCode(taskStock.PositionNr, t.BarCode);
                }
                this.Context.SaveAll();


                new StockTaskLogService(this.DbString).CreateByStockTask(t);
            }

            return true;
        }

        /// <summary>
        /// 根据运单生成出库任务，写入MSMQ
        /// </summary>
        /// <param name="deliveryNr"></param>
        /// <returns></returns>
        public ResultMessage CreateOutStockTaskByDeliery(string deliveryNr)
        {
            ResultMessage message = new ResultMessage();
            try {
                IDeliveryRepository deliveryRep = new DeliveryRepository(this.Context);
                Delivery delivery = deliveryRep.FindByNr(deliveryNr);

                if (delivery == null)
                {
                    message.Content = string.Format("运单{0}不存在", deliveryNr);
                    return message;
                }

                // 按照箱子类型排序
                List<DeliveryStorageView> deliveryStorages =
                    deliveryRep.GetStorageList(deliveryNr).ToList();

                int deliveryStoragesCount = deliveryStorages.Count;

                if (deliveryStoragesCount == 0)
                {
                    message.Content = string.Format("运单{0}不存在库存项", deliveryNr);
                    return message;
                }
                List<Data.BoxType> boxTypes = new BoxTypeRepository(this.Context).All();

                List<StockTask> stockTasks = new List<StockTask>();

                IPositionRepository posiRep = new PositionRepository(this.Context);
                 
                foreach (var boxType in boxTypes)
                {
                    //List<DeliveryStorageView> deliveryStoragesByBoxType =
                    //    deliveryStorages.Where(s => s.UniqueItemBoxTypeId == boxType.Id)
                    //    .OrderBy(s => s.StoragePositionNr).ToList();

                    List<DeliveryStorageView> deliveryStoragesByBoxType = new List<DeliveryStorageView>();

                    List<DeliveryStorageView> roadMachine1Tasks =
                        deliveryStorages.Where(s => s.UniqueItemBoxTypeId == boxType.Id
                        && s.PositionRoadMachineIndex == 1).OrderBy(s => s.StoragePositionNr).ToList();

                    List<DeliveryStorageView> roadMachine2Tasks =
                        deliveryStorages.Where(s => s.UniqueItemBoxTypeId == boxType.Id
                        && s.PositionRoadMachineIndex == 2).OrderBy(s => s.StoragePositionNr).ToList();

                    int count = roadMachine1Tasks.Count > roadMachine2Tasks.Count ? roadMachine1Tasks.Count : roadMachine2Tasks.Count;

                    for(var i = 0; i < count; i++)
                    {
                        if (i < roadMachine1Tasks.Count)
                        {
                            deliveryStoragesByBoxType.Add(roadMachine1Tasks[i]);
                        }

                        if (i < roadMachine2Tasks.Count)
                        {
                            deliveryStoragesByBoxType.Add(roadMachine2Tasks[i]);
                        }
                    }

                    int totalItemCount = deliveryStoragesByBoxType.Count;
                    if (totalItemCount > 0)
                    {
                        int trayCount =
                            deliveryStoragesByBoxType.Count % boxType.TrayQty.Value == 0 ?
                            deliveryStoragesByBoxType.Count / boxType.TrayQty.Value
                            :
                            deliveryStoragesByBoxType.Count / boxType.TrayQty.Value + 1;
                        for (int i = 0; i < trayCount; i++)
                        {
                            string trayBatchId = Guid.NewGuid().ToString();
                            int currentTrayItemCount = (i + 1 == trayCount) ? (totalItemCount - i * boxType.TrayQty.Value) : boxType.TrayQty.Value;
                            for (int j = 0; j < currentTrayItemCount; j++)
                            {
                                var s = deliveryStoragesByBoxType[i * boxType.TrayQty.Value + j];

                                Position position = posiRep.FindByNr(s.StoragePositionNr);

                                stockTasks.Add(new StockTask()
                                {
                                    Type = (int)StockTaskType.OUT,

                                    RoadMachineIndex = position.RoadMachineIndex,
                                    BoxType=s.UniqueItemBoxTypeId,
                                    BarCode=s.UniqueItemNr,
                                    PositionNr = position.Nr,
                                    PositionFloor = position.Floor,
                                    PositionColumn = position.Column,
                                    PositionRow = position.Row,

                                    State = (int)StockTaskState.RoadMachineOutStockInit,
                                    PickItemNum = deliveryStoragesCount,
                                    TrayNum= currentTrayItemCount,
                                    TrayReverseNo= currentTrayItemCount-j,
                                    PickBatchId = deliveryNr,
                                    TrayBatchId = trayBatchId,
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                });

                            }
                        }
                    }
                }
                IStockTaskRepository stRep = new StockTaskRepository(this.Context);
                stRep.Creates(stockTasks);
                this.Context.SaveAll();

                message.Success = true;


            }catch(Exception ex)
            {
                LogUtil.Logger.Error(ex.Message, ex);
                message.Content = ex.Message;
                message.MessageType = MessageType.Exception;
            }
            return message;
        }

        /// <summary>
        /// 根据择货单生成出库任务，写入MSMQ
        /// </summary>
        /// <param name="pickListNr"></param>
        /// <returns></returns>
        public ResultMessage CreateOutStockTaskByPickList(string pickListNr)
        {
            ResultMessage message = new ResultMessage();
            try
            {
                IPickListRepository pickListRep = new PickListRepository(this.Context);
                PickList  pickList = pickListRep.FindByNr(pickListNr);

                if (pickList == null)
                {
                    message.Content = string.Format("择货单{0}不存在", pickListNr);
                    return message;
                }
                
                // 按照箱子类型排序
                List<PickListStorageView> deliveryStorages =
                    pickListRep.GetStorageList(pickListNr).ToList();

                int deliveryStoragesCount = deliveryStorages.Count;

                if (deliveryStoragesCount == 0)
                {
                    message.Content = string.Format("择货单{0}不存在库存项", pickListNr);
                    return message;
                }
                List<Data.BoxType> boxTypes = new BoxTypeRepository(this.Context).All();

                List<StockTask> stockTasks = new List<StockTask>();

                IPositionRepository posiRep = new PositionRepository(this.Context);

                foreach (var boxType in boxTypes)
                {
                    //List<DeliveryStorageView> deliveryStoragesByBoxType =
                    //    deliveryStorages.Where(s => s.UniqueItemBoxTypeId == boxType.Id)
                    //    .OrderBy(s => s.StoragePositionNr).ToList();

                    List<PickListStorageView> pickListStoragesByBoxType = new List<PickListStorageView>();

                    List<PickListStorageView> roadMachine1Tasks =
                        deliveryStorages.Where(s => s.UniqueItemBoxTypeId == boxType.Id
                        && s.PositionRoadMachineIndex == 1).OrderBy(s => s.StoragePositionNr).ToList();

                    List<PickListStorageView> roadMachine2Tasks =
                        deliveryStorages.Where(s => s.UniqueItemBoxTypeId == boxType.Id
                        && s.PositionRoadMachineIndex == 2).OrderBy(s => s.StoragePositionNr).ToList();

                    int count = roadMachine1Tasks.Count > roadMachine2Tasks.Count ? roadMachine1Tasks.Count : roadMachine2Tasks.Count;

                    for (var i = 0; i < count; i++)
                    {
                        if (i < roadMachine1Tasks.Count)
                        {
                            pickListStoragesByBoxType.Add(roadMachine1Tasks[i]);
                        }

                        if (i < roadMachine2Tasks.Count)
                        {
                            pickListStoragesByBoxType.Add(roadMachine2Tasks[i]);
                        }
                    }

                    int totalItemCount = pickListStoragesByBoxType.Count;
                    if (totalItemCount > 0)
                    {
                        int trayCount =
                            pickListStoragesByBoxType.Count % boxType.TrayQty.Value == 0 ?
                            pickListStoragesByBoxType.Count / boxType.TrayQty.Value
                            :
                            pickListStoragesByBoxType.Count / boxType.TrayQty.Value + 1;
                        for (int i = 0; i < trayCount; i++)
                        {
                            string trayBatchId = Guid.NewGuid().ToString();
                            int currentTrayItemCount = (i + 1 == trayCount) ? (totalItemCount - i * boxType.TrayQty.Value) : boxType.TrayQty.Value;
                            for (int j = 0; j < currentTrayItemCount; j++)
                            {
                                var s = pickListStoragesByBoxType[i * boxType.TrayQty.Value + j];

                                Position position = posiRep.FindByNr(s.StoragePositionNr);

                                stockTasks.Add(new StockTask()
                                {
                                    Type = (int)StockTaskType.OUT,

                                    RoadMachineIndex = position.RoadMachineIndex,
                                    BoxType = s.UniqueItemBoxTypeId,
                                    BarCode = s.UniqueItemNr,
                                    PositionNr = position.Nr,
                                    PositionFloor = position.Floor,
                                    PositionColumn = position.Column,
                                    PositionRow = position.Row,

                                    State = (int)StockTaskState.RoadMachineOutStockInit,
                                    PickItemNum = deliveryStoragesCount,
                                    TrayNum = currentTrayItemCount,
                                    TrayReverseNo = currentTrayItemCount - j,
                                    PickBatchId = pickListNr,
                                    TrayBatchId = trayBatchId,

                                    PickListItemId = s.PickListItemId,

                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                });

                            }
                        }
                    }
                }
                IStockTaskRepository stRep = new StockTaskRepository(this.Context);
                stRep.Creates(stockTasks);
                pickList.State = (int)PickListState.PickTaskCreated;
                this.Context.SaveAll();

                message.Success = true;
            }
            catch (Exception ex)
            {
                LogUtil.Logger.Error(ex.Message, ex);
                message.Content = ex.Message;
                message.MessageType = MessageType.Exception;
            }
            return message;
        }

        /// <summary>
        /// 获取新创建的出库任务，每次只加载一托
        /// </summary>
        /// <param name="dispatchedBatchId"></param>
        /// <returns></returns>
        public List<StockTask> GetInitOutStockTasksAndUpdateState(List<string> dispatchedBatchId)
        {
            dispatchedBatchId = new List<string>();
               IStockTaskRepository stockTaskRep = new StockTaskRepository(this.Context);
            List<StockTask> tasks = new List<StockTask>();
            // 等待执行的任务
            List<StockTask> waitTasks = stockTaskRep.GetByState(StockTaskState.RoadMachineWaitOutStock);
            // 等待分配的任务
            List<StockTask> waitDispatchTasks = stockTaskRep.GetByState(StockTaskState.RoadMachineWaitOutStockDispatch);
            // 执行中的任务
            List<StockTask> outingTasks = stockTaskRep.GetByState(StockTaskState.RoadMachineOutStocking);

            if (waitTasks.Count == 0 && waitDispatchTasks.Count == 0 && outingTasks.Count == 0)
            {
                tasks = stockTaskRep
                    .GetByState(StockTaskState.RoadMachineOutStockInit)
                    .Where(s => (!dispatchedBatchId.Contains(s.TrayBatchId)))
                    .OrderBy(s => s.PickBatchId)
                    .ThenBy(s=>s.CreatedAt)
                    .ThenBy(s=>s.BoxType)
                    .ThenBy(s=>s.TrayNum)
                    .ThenBy(s => s.TrayBatchId).ToList();

                StockTask st = tasks.FirstOrDefault();
                if (st != null)
                {
                    tasks = tasks.Where(s => s.TrayBatchId == st.TrayBatchId).ToList();
                }
                if (tasks.Count > 0)
                {
                    stockTaskRep.UpdateTasksState(tasks.Select(s => s.Id).ToList(),
                        StockTaskState.RoadMachineWaitOutStockDispatch);
                }
            }
            return tasks;
        }

        /// <summary>
        /// 根据状态获取任务
        /// </summary>
        /// <param name="states"></param>
        /// <returns></returns>
        public List<StockTask> GetTaskByStates(List<StockTaskState> states)
        {
            return new StockTaskRepository(this.Context).GetByStates(states);
        }

        public StockTask GetTaskByStatesAndRoadMachine(List<StockTaskState> states,int? roadMachineIndex = null)
        {
            return new StockTaskRepository(this.Context).GetByStatesAndRoadMachine(states, roadMachineIndex);
        }

        public List<StockTask> GetLastTasks(int take = 300)
        {
            return new StockTaskRepository(this.Context).GetLast(take);
        }

        public IQueryable<StockTask> Search(StockTaskSearchModel searchModel)
        {
            return new StockTaskRepository(this.Context).Search(searchModel);
        }
    }
}
