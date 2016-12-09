﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace AgvClientWPF.AgvDeliveryService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="AgvDeliveryService.IDeliveryService")]
    public interface IDeliveryService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDeliveryService/DeliveryExists", ReplyAction="http://tempuri.org/IDeliveryService/DeliveryExistsResponse")]
        bool DeliveryExists(string nr);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDeliveryService/CanDeliverySend", ReplyAction="http://tempuri.org/IDeliveryService/CanDeliverySendResponse")]
        AGVCenterLib.Model.Message.ResultMessage CanDeliverySend(string nr);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDeliveryService/CanItemAddToDelivery", ReplyAction="http://tempuri.org/IDeliveryService/CanItemAddToDeliveryResponse")]
        AGVCenterLib.Model.Message.ResultMessage CanItemAddToDelivery(string uniqNr);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDeliveryService/CanItemAddToTray", ReplyAction="http://tempuri.org/IDeliveryService/CanItemAddToTrayResponse")]
        AGVCenterLib.Model.Message.ResultMessage CanItemAddToTray(string uniqNr, string deliveryNr);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDeliveryService/CreateDelivery", ReplyAction="http://tempuri.org/IDeliveryService/CreateDeliveryResponse")]
        AGVCenterLib.Model.Message.ResultMessage CreateDelivery(string delieryNr, string[] uniqItemsNrs);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDeliveryService/GetDeliveryUniqItemsByNr", ReplyAction="http://tempuri.org/IDeliveryService/GetDeliveryUniqItemsByNrResponse")]
        AGVCenterLib.Model.ViewModel.UniqueItemModel[] GetDeliveryUniqItemsByNr(string nr);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDeliveryService/CreateTray", ReplyAction="http://tempuri.org/IDeliveryService/CreateTrayResponse")]
        AGVCenterLib.Model.Message.ResultMessage CreateTray(string delieryNr, string trayNr, string[] uniqItemsNrs);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IDeliveryServiceChannel : AgvClientWPF.AgvDeliveryService.IDeliveryService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class DeliveryServiceClient : System.ServiceModel.ClientBase<AgvClientWPF.AgvDeliveryService.IDeliveryService>, AgvClientWPF.AgvDeliveryService.IDeliveryService {
        
        public DeliveryServiceClient() {
        }
        
        public DeliveryServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DeliveryServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DeliveryServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DeliveryServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool DeliveryExists(string nr) {
            return base.Channel.DeliveryExists(nr);
        }
        
        public AGVCenterLib.Model.Message.ResultMessage CanDeliverySend(string nr) {
            return base.Channel.CanDeliverySend(nr);
        }
        
        public AGVCenterLib.Model.Message.ResultMessage CanItemAddToDelivery(string uniqNr) {
            return base.Channel.CanItemAddToDelivery(uniqNr);
        }
        
        public AGVCenterLib.Model.Message.ResultMessage CanItemAddToTray(string uniqNr, string deliveryNr) {
            return base.Channel.CanItemAddToTray(uniqNr, deliveryNr);
        }
        
        public AGVCenterLib.Model.Message.ResultMessage CreateDelivery(string delieryNr, string[] uniqItemsNrs) {
            return base.Channel.CreateDelivery(delieryNr, uniqItemsNrs);
        }
        
        public AGVCenterLib.Model.ViewModel.UniqueItemModel[] GetDeliveryUniqItemsByNr(string nr) {
            return base.Channel.GetDeliveryUniqItemsByNr(nr);
        }
        
        public AGVCenterLib.Model.Message.ResultMessage CreateTray(string delieryNr, string trayNr, string[] uniqItemsNrs) {
            return base.Channel.CreateTray(delieryNr, trayNr, uniqItemsNrs);
        }
    }
}
