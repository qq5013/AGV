﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace AGVCenterLib.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=Charlot-PC\\MSSQLSERVER20082;Initial Catalog=AgvWarehoueDb;Persist Sec" +
            "urity Info=True;User ID=sa;Password=123456@")]
        public string AgvWarehoueDbConnectionString {
            get {
                return ((string)(this["AgvWarehoueDbConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=WANGSONG-PC\\MSSQLSERVER2008R;Initial Catalog=AgvWarehoueDb;Persist Se" +
            "curity Info=True;User ID=sa;Password=123456@")]
        public string AgvWarehoueDbConnectionString1 {
            get {
                return ((string)(this["AgvWarehoueDbConnectionString1"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=Charlot-PC\\MSSQLSERVER20082;Initial Catalog=AgvWarehoueDb;Persist Sec" +
            "urity Info=True;User ID=sa")]
        public string AgvWarehoueDbConnectionString2 {
            get {
                return ((string)(this["AgvWarehoueDbConnectionString2"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=Charlot-PC\\MSSQLSERVER20082;Initial Catalog=AgvWarehouseDb;Persist Se" +
            "curity Info=True;User ID=sa")]
        public string AgvWarehouseDbConnectionString {
            get {
                return ((string)(this["AgvWarehouseDbConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=Charlot-PC\\MSSQLSERVER20082;Initial Catalog=AgvWarehouseDb;Persist Se" +
            "curity Info=True;User ID=sa;Password=123456@")]
        public string AgvWarehouseDbConnectionString1 {
            get {
                return ((string)(this["AgvWarehouseDbConnectionString1"]));
            }
        }
    }
}
