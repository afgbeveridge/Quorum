#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
namespace QuorumWindowsService {
    partial class ProjectInstaller {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.QWASIInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.QuorumWindowsService = new System.ServiceProcess.ServiceInstaller();
            // 
            // QWASIInstaller
            // 
            this.QWASIInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.QWASIInstaller.Password = null;
            this.QWASIInstaller.Username = null;
            // 
            // QuorumWindowsService
            // 
            this.QuorumWindowsService.Description = "Quorum Windows Service";
            this.QuorumWindowsService.DisplayName = "Quorum Windows Service";
            this.QuorumWindowsService.ServiceName = "Quorum Windows Service";
            this.QuorumWindowsService.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.QWASIInstaller,
            this.QuorumWindowsService});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller QWASIInstaller;
        private System.ServiceProcess.ServiceInstaller QuorumWindowsService;
    }
}