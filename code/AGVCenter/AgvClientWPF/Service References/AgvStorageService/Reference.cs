﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace AgvClientWPF.AgvStorageService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="AgvStorageService.IStorageService")]
    public interface IStorageService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IStorageService/GetAll", ReplyAction="http://tempuri.org/IStorageService/GetAllResponse")]
        AGVCenterLib.Model.ViewModel.StorageModel[] GetAll();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IStorageServiceChannel : AgvClientWPF.AgvStorageService.IStorageService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class StorageServiceClient : System.ServiceModel.ClientBase<AgvClientWPF.AgvStorageService.IStorageService>, AgvClientWPF.AgvStorageService.IStorageService {
        
        public StorageServiceClient() {
        }
        
        public StorageServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public StorageServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public StorageServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public StorageServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public AGVCenterLib.Model.ViewModel.StorageModel[] GetAll() {
            return base.Channel.GetAll();
        }
    }
}