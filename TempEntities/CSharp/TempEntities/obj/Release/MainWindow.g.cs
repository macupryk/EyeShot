﻿#pragma checksum "..\..\MainWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "6315B1BA6BB64975A5262EFE8749F59F27198BC0"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using WpfApplication1;
using devDept.Eyeshot;
using devDept.Graphics;


namespace WpfApplication1 {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 28 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.GroupBox groupBox1;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox addItemCombo;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.ToggleButton addItemToggle;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox planeCombo;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.ToggleButton moveToggle;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TreeView treeView1;
        
        #line default
        #line hidden
        
        
        #line 69 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.ToggleButton blinkToggle;
        
        #line default
        #line hidden
        
        
        #line 76 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox bboxCheckBox;
        
        #line default
        #line hidden
        
        
        #line 77 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView listView1;
        
        #line default
        #line hidden
        
        
        #line 111 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal devDept.Eyeshot.Model model1;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/TempEntities;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.groupBox1 = ((System.Windows.Controls.GroupBox)(target));
            return;
            case 2:
            this.addItemCombo = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 3:
            this.addItemToggle = ((System.Windows.Controls.Primitives.ToggleButton)(target));
            return;
            case 4:
            this.planeCombo = ((System.Windows.Controls.ComboBox)(target));
            
            #line 42 "..\..\MainWindow.xaml"
            this.planeCombo.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.planeCombo_SelectedIndexChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.moveToggle = ((System.Windows.Controls.Primitives.ToggleButton)(target));
            
            #line 47 "..\..\MainWindow.xaml"
            this.moveToggle.Click += new System.Windows.RoutedEventHandler(this.moveCheckBox_CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.treeView1 = ((System.Windows.Controls.TreeView)(target));
            
            #line 53 "..\..\MainWindow.xaml"
            this.treeView1.SelectedItemChanged += new System.Windows.RoutedPropertyChangedEventHandler<object>(this.treeView1_SelectedChanged);
            
            #line default
            #line hidden
            return;
            case 7:
            this.blinkToggle = ((System.Windows.Controls.Primitives.ToggleButton)(target));
            
            #line 69 "..\..\MainWindow.xaml"
            this.blinkToggle.Click += new System.Windows.RoutedEventHandler(this.blinkToggle_CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 8:
            this.bboxCheckBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 9:
            this.listView1 = ((System.Windows.Controls.ListView)(target));
            
            #line 78 "..\..\MainWindow.xaml"
            this.listView1.DragEnter += new System.Windows.DragEventHandler(this.listView1_DragEnter);
            
            #line default
            #line hidden
            
            #line 78 "..\..\MainWindow.xaml"
            this.listView1.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.listView1_SelectedChanged);
            
            #line default
            #line hidden
            
            #line 78 "..\..\MainWindow.xaml"
            this.listView1.MouseUp += new System.Windows.Input.MouseButtonEventHandler(this.ListView_OnMouseUp);
            
            #line default
            #line hidden
            return;
            case 10:
            this.model1 = ((devDept.Eyeshot.Model)(target));
            
            #line 111 "..\..\MainWindow.xaml"
            this.model1.DragEnter += new System.Windows.DragEventHandler(this.viewport_dragEnter);
            
            #line default
            #line hidden
            
            #line 111 "..\..\MainWindow.xaml"
            this.model1.Drop += new System.Windows.DragEventHandler(this.viewport_dragDrop);
            
            #line default
            #line hidden
            
            #line 111 "..\..\MainWindow.xaml"
            this.model1.PreviewMouseUp += new System.Windows.Input.MouseButtonEventHandler(this.model1_MouseUp);
            
            #line default
            #line hidden
            
            #line 111 "..\..\MainWindow.xaml"
            this.model1.MouseMove += new System.Windows.Input.MouseEventHandler(this.model1_MouseMove);
            
            #line default
            #line hidden
            
            #line 111 "..\..\MainWindow.xaml"
            this.model1.PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler(this.model1_MouseDown);
            
            #line default
            #line hidden
            
            #line 111 "..\..\MainWindow.xaml"
            this.model1.DragLeave += new System.Windows.DragEventHandler(this.viewport_dragLeave);
            
            #line default
            #line hidden
            
            #line 111 "..\..\MainWindow.xaml"
            this.model1.DragOver += new System.Windows.DragEventHandler(this.viewport_dragOver);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
